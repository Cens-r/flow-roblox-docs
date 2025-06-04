using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.RobloxDocs.Views
{
    public partial class LabeledInput : UserControl
    {
        public static readonly DependencyProperty TitleProperty = 
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(LabeledInput), new PropertyMetadata("Title"));
            
        public static readonly DependencyProperty SubtitleProperty = 
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(LabeledInput), new PropertyMetadata("Subtitle"));
            
        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(LabeledInput), new PropertyMetadata("N/A"));

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

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public LabeledInput()
        {
            InitializeComponent();
        }
    }
}