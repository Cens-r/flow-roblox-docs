using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.RobloxDocs.Views;

namespace Flow.Launcher.Plugin.RobloxDocs {
    public class RobloxDocs : IAsyncPlugin, IAsyncReloadable, ISettingProvider {
        private PluginInitContext _context;
        private PluginSettings _settings;
        private RobloxApi _api;

        // Documentation loader is stored to ensure plugin initialization doesn't
        // take an exhaustive amount of time (which prevents Flow Launcher from starting)
        private Task _loader;
        
        public Task InitAsync(PluginInitContext context) {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<PluginSettings>();
            
            _api = new RobloxApi();
            _loader = _api.LoadRecords(context);
            
            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token) {
            await _loader; // Ensure the loader has finished before processing queries
            
            var results = new List<Result>();
            var records = _api.Search(
                query.Search, 
                _settings.MaxResults,
                _settings.ScoreThreshold,
                _settings.ShowDeprecated);
            
            for (var index = 0; index < records.Count; index++) {
                var (record, score) = records[index];

                var subtitle = $"(Score: {score}%)";

                var sortedTags = record.Tags.OrderBy(item => item).ToList();
                if (sortedTags.Any()) {
                    subtitle = $"[{string.Join("] ⋮ [", sortedTags)}] ⋮ {subtitle}";
                }

                results.Add(new Result() {
                    Title = record.GetFullName(),
                    SubTitle = $"↪ {subtitle}",
                    Score = _settings.MaxResults - index,
                    CopyText = record.Url,
                    Action = _ => {
                        try {
                            _context.API.OpenUrl(record.Url);
                            return true;
                        } catch {
                            return false;
                        }
                    },
                    IcoPath = record.ImagePath,
                });
            };

            return results;
        }

        public Task ReloadDataAsync() {
            _loader = _api.LoadRecords(_context);
            return _loader;
        }

        public Control CreateSettingPanel() {
            return new SettingsView(_settings);
        }
    }
}