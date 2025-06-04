using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.Composite;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.RobloxDocs;

/// <summary>
/// Provides methods for interacting with the Roblox API documentation and type database.
/// </summary>
public class RobloxApi {
    private const string CurrentVersionUrl = "https://clientsettings.roblox.com/v1/client-version/WindowsStudio";
    private const string ApiDumpUrlTemplate = "https://s3.amazonaws.com/setup.roblox.com/{0}-API-Dump.json";
    private const string ApiDocsUrl =
        "https://raw.githubusercontent.com/MaximumADHD/Roblox-Client-Tracker/roblox/api-docs/en-us.json";

    private ApiDump _api;
    private Dictionary<string, DocEntry> _docs;
    private List<ApiRecord> _active;
    private List<ApiRecord> _deprecated;
    private HashSet<string> _datatypes = new();

    private static readonly HttpClient HttpClient = new();

    private static readonly string PluginDirectory =
        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    private static readonly string ImagesDirectory = Path.Combine(PluginDirectory, "images");

    private static async Task<T> FetchJson<T>(string url) {
        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }

    private static async Task<string> GetCurrentVersion() {
        var versions = await FetchJson<Dictionary<string, string>>(CurrentVersionUrl);
        return versions["clientVersionUpload"];
    }

    private static string GetImagePath(string name) {
        var path = Path.Combine(ImagesDirectory, $"{name}.png");
        return File.Exists(path) ? path : Path.Combine(ImagesDirectory, "Placeholder.png");
    }

    private static async Task<ApiDump> FetchApiDump() {
        var version = await GetCurrentVersion();
        var url = string.Format(ApiDumpUrlTemplate, version);
        return await FetchJson<ApiDump>(url);
    }

    private static async Task<Dictionary<string, DocEntry>> FetchApiDocs() {
        return await FetchJson<Dictionary<string, DocEntry>>(ApiDocsUrl);
    }

    private void ConstructRecord(string type, DescriptionBase info, string className = null, string image = null) {
        var docKey = $"@roblox/{type}/" + (className != null ? $"{className}.{info.Name}" : info.Name);
        if (!_docs.TryGetValue(docKey, out var docsEntry)) { return; }

        var container = info.Tags.Contains("Deprecated") ? _deprecated : _active;
        container.Add(
            new ApiRecord(
            new List<string> { info.Name, className },
            docsEntry.Description,
            docsEntry.Url,
            image,
            info.Tags)
        );

        if (className != null && info is MemberDump member) {
            member.Parameters?.ForEach(dump => {
                var datatype = dump.Type;
                _datatypes.Add(datatype.Name);
            });
        }
    }

    public async Task LoadRecords(PluginInitContext context) {
        _datatypes.Clear();
        
        _api = await FetchApiDump();
        _docs = await FetchApiDocs();

        _active = new List<ApiRecord>();
        _deprecated = new List<ApiRecord>();

        foreach (var classEntry in _api.Classes) {
            var classImage = GetImagePath(classEntry.Name);
            ConstructRecord("globaltype", classEntry, null, classImage);
            classEntry.Members.ForEach(memberEntry => {
                ConstructRecord("globaltype", memberEntry, classEntry.Name, classImage);
            });
        }
        
        var enumImage = GetImagePath("Enum");
        var memberImage = GetImagePath("EnumMember");
        
        foreach (var enumEntry in _api.Enums) {
            ConstructRecord("global", enumEntry, "Enum", enumImage);
            enumEntry.Items.ForEach(itemEntry => { ConstructRecord("enum", itemEntry, enumEntry.Name, memberImage); });
        }
        
        var typeImage = GetImagePath("ACCodeSnippet");
        foreach (var datatype in _datatypes) {
            var dictKey = $"@roblox/global/{datatype}";
            if (!_docs.TryGetValue(dictKey, out var docsEntry)) { continue; }
            _active.Add(new ApiRecord(
                new List<string>() { datatype },
                docsEntry.Description,
                docsEntry.Url,
                typeImage,
                new List<string>())
            );
        }
    }

    public List<(ApiRecord, int)> Search(string query, int limit = 20, int threshold = 50, bool includeDeprecated = false) {
        if (_active == null || _deprecated == null) {
            throw new MemberAccessException("LoadRecords must be called once before searching!");
        }

        if (string.IsNullOrWhiteSpace(query)) {
            return new List<(ApiRecord, int)>();
        }

        query = query.ToLower();
        var collection = includeDeprecated ? _active.Concat(_deprecated) : _active;

        var scoredResults = new List<(ApiRecord, int)>();
        foreach (var record in collection) {
            var score = Fuzz.WeightedRatio(query, record.Name.ToLower());
            if (record.ClassName != null) {
                var classScore = Fuzz.WeightedRatio(query, record.ClassName.ToLower());

                if (score >= 95 || classScore >= 95) {
                    score = Math.Max(score, classScore);
                } else {
                    score = (int)(score * 0.5 + classScore * 0.5);
                }
            }

            if (score >= threshold) {
                scoredResults.Add((record, score));
            }
        }

        return scoredResults
            .OrderByDescending(x => x.Item2)
            .ThenBy(x => {
                var record = x.Item1;
                if (record.ClassName == "Enum") {
                    // Prioritize enums over their items
                    return record.Name.Length;
                }
                return record.GetFullName().Length;
            })
            .Take(limit)
            .ToList();
    }
}

// Helper classes used for deserialization of the API and documentation dumps.

public record DescriptionBase {
    public string Name { get; set; }
    public List<string> Tags { get; set; }

    public DescriptionBase() {
        Name = string.Empty;
        Tags = new List<string>();
    }
};

public record DataType {
    public string Name { get; set; }
    public string Category { get; set; }
    
    public DataType() {
        Name = string.Empty;
        Category = string.Empty;
    }
}

public record DataDump {
    public string Name { get; set; }
    public DataType Type { get; set; }

    public DataDump() {
        Name = string.Empty;
        Type = new DataType();
    }
}

public record MemberDump : DescriptionBase {
    public string MemberType { get; set; }
    [CanBeNull] public List<DataDump> Parameters { get; init; }
    [CanBeNull] public DataType ReturnType { get; init; }

    public MemberDump() {
        MemberType = string.Empty;
        Parameters = new List<DataDump>();
        ReturnType = new DataType();
    }
}

public record ClassDump : DescriptionBase {
    public List<MemberDump> Members { get; init; }
    [CanBeNull] public List<DataDump> Parameters { get; init; }
    [CanBeNull] public DataType ReturnType { get; init; }

    public ClassDump() {
        Members = new List<MemberDump>();
        Parameters = new List<DataDump>();
        ReturnType = new DataType();
    }
}

public record EnumDump : DescriptionBase {
    public List<DescriptionBase> Items { get; init; }

    public EnumDump() {
        Items = new List<DescriptionBase>();
    }
}

public record ApiDump {
    public List<ClassDump> Classes { get; set; }
    public List<EnumDump> Enums { get; set; }
    public int Version { get; set; }

    public ApiDump() {
        Classes = new List<ClassDump>();
        Enums = new List<EnumDump>();
        Version = 0;
    }
}

public record DocEntry {
    [JsonPropertyName("documentation")] public string Description { get; init; }
    [JsonPropertyName("learn_more_link")] public string Url { get; init; }

    public DocEntry() {
        Description = string.Empty;
        Url = string.Empty;
    }
};

public record ApiRecord : DescriptionBase {
    [CanBeNull] public string ClassName { get; init; }
    public string Description { get; init; }
    public string Url { get; init; }
    public string ImagePath { get; init; }

    public string GetFullName() {
        return ClassName != null ? $"{ClassName}.{Name}" : Name;
    }

    public ApiRecord(List<string> name, string description, string url, string image, List<string> tags) {
        Name = name[0];
        ClassName = name.Count > 1 ? name[1] : null;
        Description = description;
        Url = url;
        ImagePath = image;
        Tags = tags;
    }

    public ApiRecord() : this(new List<string>(), string.Empty, string.Empty, string.Empty, new List<string>()) {}
}