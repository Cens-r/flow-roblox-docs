using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.RobloxDocs.Views
{
    public partial class LabeledCheckbox : UserControl
    {
        public static readonly DependencyProperty TitleProperty = 
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(LabeledCheckbox), new PropertyMetadata("Title"));
            
        public static readonly DependencyProperty SubtitleProperty = 
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(LabeledCheckbox), new PropertyMetadata("Subtitle"));
            
        public static readonly DependencyProperty StateProperty = 
            DependencyProperty.Register(nameof(State), typeof(bool), typeof(LabeledCheckbox), new PropertyMetadata(false));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public bool State
        {
            get => (bool)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public LabeledCheckbox()
        {
            InitializeComponent();
        }
    }
}