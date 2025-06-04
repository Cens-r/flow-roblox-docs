using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Flow.Launcher.Plugin.RobloxDocs;

public class PluginSettings: INotifyPropertyChanged {
    private bool _showDeprecated = false;
    public bool ShowDeprecated {
        get => _showDeprecated;
        set {
            _showDeprecated = value;
            OnPropertyChanged();
        }
    }
    
    private int _scoreThreshold = 30;
    public int ScoreThreshold {
        get => _scoreThreshold;
        set {
            _scoreThreshold = value;
            OnPropertyChanged();
        }
    }
    
    private int _maxResults = 25;
    public int MaxResults {
        get => _maxResults;
        set {
            _maxResults = value;
            OnPropertyChanged();
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}