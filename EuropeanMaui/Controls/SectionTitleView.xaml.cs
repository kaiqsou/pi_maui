using System.Windows.Input;

namespace EuropeanMaui.Controls
{
    public partial class SectionTitleView : ContentView
    {
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title), typeof(string), typeof(SectionTitleView), string.Empty);

        public static readonly BindableProperty ActionLabelProperty = BindableProperty.Create(
            nameof(ActionLabel), typeof(string), typeof(SectionTitleView), null,
            propertyChanged: (b, _, _) => ((SectionTitleView)b).OnPropertyChanged(nameof(HasAction)));

        public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
            nameof(ActionCommand), typeof(ICommand), typeof(SectionTitleView), null);

        public SectionTitleView()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string? ActionLabel
        {
            get => (string?)GetValue(ActionLabelProperty);
            set => SetValue(ActionLabelProperty, value);
        }

        public bool HasAction => !string.IsNullOrEmpty(ActionLabel) && ActionCommand != null;

        public ICommand? ActionCommand
        {
            get => (ICommand?)GetValue(ActionCommandProperty);
            set
            {
                SetValue(ActionCommandProperty, value);
                OnPropertyChanged(nameof(HasAction));
            }
        }

        private void OnActionTapped(object? sender, TappedEventArgs e)
        {
            if (ActionCommand?.CanExecute(null) == true)
                ActionCommand.Execute(null);
        }
    }
}