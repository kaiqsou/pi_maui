using System.Windows.Input;

namespace EuropeanMaui.Controls
{
    public partial class HeaderView : ContentView
    {
        public HeaderView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title), typeof(string), typeof(HeaderView), string.Empty,
            propertyChanged: (_, _, _) => { });

        public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
            nameof(Subtitle), typeof(string), typeof(HeaderView), null,
            propertyChanged: (b, _, _) => ((HeaderView)b).OnPropertyChanged(nameof(HasSubtitle)));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string? Subtitle
        {
            get => (string?)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);
    }
}