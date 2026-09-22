using System.ComponentModel;
using EuropeanMaui.Controls;
using EuropeanMaui.Services;
using EuropeanMaui.ViewModels;
using InputView = EuropeanMaui.Controls.InputView;

namespace EuropeanMaui.Views
{
    /// <summary>Lista de módulos do mestrado (equivalente ao ModulesScreen).</summary>
    public class ModulosView : Grid
    {
        private readonly ModulosViewModel _vm;
        private readonly VerticalStackLayout _content;
        private readonly VerticalStackLayout _listHost;
        private readonly InputView _searchInput;

        public ModulosView(ModulosViewModel vm)
        {
            _vm = vm;
            BackgroundColor = Colors.Transparent;

            var header = new HeaderView { Title = AppText.Modulos.Title, Subtitle = AppText.Modulos.Subtitle };

            _searchInput = new InputView
            {
                Placeholder = AppText.Modulos.SearchPlaceholder,
                PrefixGlyph = Icons.Search,
            };
            _searchInput.TextChanged += (_, _) => _vm.Query = _searchInput.Text;

            var programSummary = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = ThemeColors.Get("Accent"),
                Padding = new Thickness(16, 14),
            };
            var programCode = new Label
            {
                FontSize = 18,
                FontFamily = "OpenSansBold",
                TextColor = Colors.White,
            };
            var programName = new Label
            {
                FontSize = 12,
                TextColor = Colors.White,
                Opacity = 0.85,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
            };
            var programCol = new VerticalStackLayout { Spacing = 2 };
            programCol.Children.Add(programCode);
            programCol.Children.Add(programName);
            programSummary.Content = programCol;

            _listHost = new VerticalStackLayout { Spacing = 12 };

            _content = new VerticalStackLayout { Spacing = 16, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 1200 };
            _content.Children.Add(header);
            _content.Children.Add(programSummary);
            _content.Children.Add(_searchInput);
            _content.Children.Add(_listHost);

            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            this.AddAt(_content, 0, 0);

            _vm.PropertyChanged += (_, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ModulosViewModel.Loading):
                    case nameof(ModulosViewModel.Error):
                        ApplyList();
                        break;
                    case nameof(ModulosViewModel.HasModules):
                        ApplyList();
                        break;
                    case nameof(ModulosViewModel.Program):
                        if (_vm.Program != null)
                        {
                            programCode.Text = _vm.Program.Code;
                            programName.Text = _vm.Program.FullName;
                        }
                        break;
                }
            };
            ApplyList();
        }

        private void ApplyList()
        {
            _listHost.Children.Clear();

            if (_vm.Loading)
            {
                for (int i = 0; i < 4; i++)
                    _listHost.Children.Add(new SkeletonCardView(4));
                return;
            }
            if (_vm.Error)
            {
                _listHost.Children.Add(new ErrorStateView(AppText.Common.Empty, () => _vm.RetryCommand.Execute(null)));
                return;
            }
            if (!_vm.HasModules)
            {
                _listHost.Children.Add(new EmptyStateView(AppText.Modulos.EmptyResult));
                return;
            }

            bool desktop = _vm.IsDesktop;
            if (desktop)
            {
                var grid = new Grid { ColumnSpacing = 12, RowSpacing = 12 };
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                int col = 0, row = 0;
                foreach (var m in _vm.Modules)
                {
                    var card = new ModuleCardView { ShowChevron = true };
                    card.Module = m;
                    grid.Add(card, col, row);
                    col++;
                    if (col == 2) { col = 0; row++; }
                }
                _listHost.Children.Add(grid);
            }
            else
            {
                foreach (var m in _vm.Modules)
                {
                    var card = new ModuleCardView { ShowChevron = true };
                    card.Module = m;
                    _listHost.Children.Add(card);
                }
            }
        }
    }
}