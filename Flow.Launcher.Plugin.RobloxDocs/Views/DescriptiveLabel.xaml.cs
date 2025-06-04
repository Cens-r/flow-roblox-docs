using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.RobloxDocs.Views
{
    public partial class DescriptiveLabel : UserControl
    {
        public static readonly DependencyProperty TitleProperty = 
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(DescriptiveLabel), new PropertyMetadata("Title"));
            
        public static readonly DependencyProperty SubtitleProperty = 
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(DescriptiveLabel), new PropertyMetadata("Subtitle"));

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

        public DescriptiveLabel()
        {
            InitializeComponent();
        }
    }
}