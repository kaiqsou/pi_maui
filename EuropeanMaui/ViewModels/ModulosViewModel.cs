using System.Collections.ObjectModel;
using System.Windows.Input;
using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.ViewModels
{
    public class ModulosViewModel : BaseViewModel
    {
        private readonly AuthSession _auth;
        private readonly ApiService _api;

        private bool _loading = true;
        private bool _error;
        private MasterProgram? _program;
        private string _query = string.Empty;

        public ModulosViewModel(AuthSession auth, LayoutService layout, ApiService api)
            : base(layout)
        {
            _auth = auth;
            _api = api;

            RetryCommand = new RelayCommand(Load);
            Load();
        }

        public MasterProgram? Program => _program;
        public bool Loading => _loading;
        public bool Error => _error;
        public bool HasProgram => _program != null;

        public string Query
        {
            get => _query;
            set
            {
                if (SetProperty(ref _query, value))
                    ApplyFilter();
            }
        }

        public ObservableCollection<Module> Modules { get; } = new();
        public bool HasModules => Modules.Count > 0;

        public ICommand RetryCommand { get; }

        private void Load()
        {
            _loading = true;
            _error = false;
            OnPropertyChanged(nameof(Loading));
            OnPropertyChanged(nameof(Error));
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var programId = _auth.ActiveMasterProgramId;
                var programs = await _api.FetchMasterProgramsAsync();
                _program = string.IsNullOrEmpty(programId)
                    ? programs.FirstOrDefault()
                    : programs.FirstOrDefault(p => p.Id == programId);
            }
            catch
            {
                _error = true;
            }
            finally
            {
                _loading = false;
                OnPropertyChanged(nameof(Loading));
                OnPropertyChanged(nameof(Error));
                OnPropertyChanged(nameof(Program));
                OnPropertyChanged(nameof(HasProgram));
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            var all = _program?.Modules ?? new List<Module>();
            var q = Query.Trim().ToLowerInvariant();

            var filtered = string.IsNullOrEmpty(q)
                ? all
                : all.Where(m => m.Title.ToLowerInvariant().Contains(q)).ToList();

            Modules.Clear();
            foreach (var module in filtered) Modules.Add(module);
            OnPropertyChanged(nameof(HasModules));
        }
    }
}