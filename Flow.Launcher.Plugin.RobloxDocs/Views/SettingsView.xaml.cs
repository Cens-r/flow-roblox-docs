using System.Windows.Controls;

namespace Flow.Launcher.Plugin.RobloxDocs.Views;

public partial class SettingsView : UserControl {
    public PluginSettings Settings { get; }

    public SettingsView(PluginSettings settings) {
        Settings = settings;
        DataContext = this;
        InitializeComponent();
    }
}