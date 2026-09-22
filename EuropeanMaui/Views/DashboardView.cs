using System.ComponentModel;
using EuropeanMaui.Controls;
using EuropeanMaui.Services;
using EuropeanMaui.ViewModels;

namespace EuropeanMaui.Views
{
    /// <summary>Dashboard pós-login (equivalente ao HomeScreen).</summary>
    public class DashboardView : Grid
    {
        private readonly DashboardViewModel _vm;
        private readonly VerticalStackLayout _content;
        private readonly Label _greetingLabel;
        private readonly Label _subtitleLabel;
        private readonly VerticalStackLayout _programSection;
        private readonly VerticalStackLayout _statsSection;
        private readonly VerticalStackLayout _continueHost;
        private readonly VerticalStackLayout _dueHost;
        private readonly VerticalStackLayout _studyHost;
        private readonly CalendarPanelView _calendarPanel;
        private readonly StudyChartView _studyChart;

        public DashboardView(DashboardViewModel vm)
        {
            _vm = vm;
            BackgroundColor = Colors.Transparent;

            _greetingLabel = new Label
            {
                FontSize = 30,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Text"),
            };
            _subtitleLabel = new Label
            {
                FontSize = 14,
                TextColor = ThemeColors.Get("TextMuted"),
            };

            var header = _header = new VerticalStackLayout { Spacing = 4 };
            header.Children.Add(_greetingLabel);
            header.Children.Add(_subtitleLabel);
            _greetingLabel.Text = $"{AppText.Dashboard.Greeting}, {_vm.FirstName}!";
            _subtitleLabel.Text = AppText.Dashboard.WelcomeBack;

            _programSection = new VerticalStackLayout { Spacing = 12 };

            _statsSection = new VerticalStackLayout { Spacing = 12 };

            _continueHost = new VerticalStackLayout { Spacing = 12 };

            _studyHost = new VerticalStackLayout { Spacing = 12 };

            var rightCol = new VerticalStackLayout { Spacing = 16 };
            _dueHost = new VerticalStackLayout { Spacing = 12 };
            _calendarPanel = new CalendarPanelView(
                _ => { },
                () => _vm.RetryDueItemsCommand.Execute(null));
            _dueHost.Children.Add(_calendarPanel);

            _studyChart = new StudyChartView();

            var leftCol = new VerticalStackLayout { Spacing = 16 };
            leftCol.Children.Add(_continueHost);
            leftCol.Children.Add(_studyHost);

            var twoCol = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(1.4, GridUnitType.Star)),
                    new ColumnDefinition(GridLength.Star),
                },
                ColumnSpacing = 16,
            };
            twoCol.AddAt(leftCol, 0, 0);
            twoCol.AddAt(rightCol, 1, 0);

            _desktopGrid = twoCol;
            _continueCard = new ContinueCardView { ContinueCommand = _vm.OnContinueCommand };
            _mobileOrder = new VerticalStackLayout { Spacing = 16 };

            _content = new VerticalStackLayout { Spacing = 28, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 1200 };
            _content.Children.Add(header);

            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            this.AddAt(_content, 0, 0);
            VerticalOptions = LayoutOptions.Fill;

            _vm.PropertyChanged += OnVmChanged;
            ApplyProgramSection();
            ApplyLayout();
        }

        private readonly Grid _desktopGrid;
        private readonly VerticalStackLayout _mobileOrder;
        private readonly ContinueCardView _continueCard;
        private VerticalStackLayout _header = new();

        private void OnVmChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DashboardViewModel.FirstName):
                    _greetingLabel.Text = $"{AppText.Dashboard.Greeting}, {_vm.FirstName}!";
                    break;
                case nameof(DashboardViewModel.IsDesktop):
                case nameof(DashboardViewModel.IsMobile):
                    ApplyLayout();
                    break;
                case nameof(DashboardViewModel.ProgramLoading):
                case nameof(DashboardViewModel.ProgramError):
                case nameof(DashboardViewModel.Program):
                case nameof(DashboardViewModel.ContinueModule):
                    ApplyProgramSection();
                    break;
                case nameof(DashboardViewModel.DueItemsLoading):
                case nameof(DashboardViewModel.DueItemsError):
                case nameof(DashboardViewModel.HasDueItems):
                    ApplyDueSection();
                    break;
                case nameof(DashboardViewModel.StudyStatsLoading):
                case nameof(DashboardViewModel.StudyStatsError):
                    ApplyStudySection();
                    break;
            }
        }

        private void ApplyProgramSection()
        {
            _programSection.Children.Clear();
            _programSection.Children.Add(new SectionTitleView { Title = AppText.Dashboard.ProgressLabel });

            if (_vm.ProgramLoading)
            {
                _programSection.Children.Add(new SkeletonCardView(4));
                return;
            }
            if (_vm.ProgramError)
            {
                _programSection.Children.Add(new ErrorStateView(AppText.Common.Empty, () => _vm.RetryProgramCommand.Execute(null)));
                return;
            }
            if (_vm.Program != null)
            {
                var card = new ProgramCardView { Program = _vm.Program };
                _programSection.Children.Add(card);
            }

            // Stats
            _statsSection.Children.Clear();
            var stats = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                },
                ColumnSpacing = 12,
            };
            stats.Add(new StatCardView(Icons.CheckmarkCircleOutline, _vm.CompletedCount, AppText.Dashboard.StatsCompleted, _vm.TotalModules), 0, 0);
            stats.Add(new StatCardView(Icons.TrendingUpOutline, _vm.AverageProgress, AppText.Dashboard.StatsProgress), 1, 0);
            stats.Add(new StatCardView(Icons.TimeOutline, _vm.DueCount, AppText.Dashboard.StatsDue), 2, 0);
            _statsSection.Children.Add(stats);
        }

        private void ApplyDueSection()
        {
            _calendarPanel.SetDueItems(_vm.DueItems);
        }

        private void ApplyStudySection()
        {
            _studyChart.SetDays(_vm.StudyDays);
            var labels = DayLabels(7);
            for (int i = 0; i < _vm.StudyDays.Count && i < 7; i++)
            {
                var day = _vm.StudyDays[i];
                bool isToday = i == _vm.StudyDays.Count - 1;
                _studyChart.SetDayColumn(i, labels[i], day.Logged, isToday);
            }
        }

        private static string[] DayLabels(int n)
        {
            var result = new string[n];
            var now = DateTime.Today;
            var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            for (int i = n - 1; i >= 0; i--)
            {
                var d = now.AddDays(-(n - 1 - i));
                result[i] = culture.DateTimeFormat.AbbreviatedDayNames[(int)d.DayOfWeek][0..3].ToUpperInvariant();
            }
            return result;
        }

        private void ApplyLayout()
        {
            if (_content == null) return;

            bool desktop = _vm.IsDesktop;

            _continueHost.Children.Clear();
            _continueHost.Children.Add(new SectionTitleView { Title = AppText.Dashboard.ContinueWhere });
            var module = _vm.ContinueModule;
            if (module != null)
            {
                _continueCard.Title = module.Title;
                _continueCard.Subtitle = $"{_vm.Program?.Code ?? ""} · {Math.Round(module.Progress)}%";
                _continueCard.Progress = module.Progress;
                _continueHost.Children.Add(_continueCard);
            }

            _studyHost.Children.Clear();
            _studyHost.Children.Add(_studyChart);

            _mobileOrder.Children.Clear();
            _mobileOrder.Children.Add(_continueHost);
            _mobileOrder.Children.Add(_dueHost);
            _mobileOrder.Children.Add(_studyHost);

            _content.Children.Clear();
            _content.Children.Add(_header);
            _content.Children.Add(_programSection);
            _content.Children.Add(_statsSection);
            _content.Children.Add(desktop ? _desktopGrid : _mobileOrder);

            ApplyDueSection();
            ApplyStudySection();
        }
    }
}