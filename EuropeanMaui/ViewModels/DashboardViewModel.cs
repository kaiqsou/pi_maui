using System.Collections.ObjectModel;
using System.Windows.Input;
using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly AuthSession _auth;
        private readonly ApiService _api;
        private readonly NavigationService _navigation;

        private bool _programLoading = true;
        private bool _programError;
        private MasterProgram? _program;
        private Module? _continueModule;
        private string _firstName = AppText.Dashboard.Greeting;
        private bool _dueItemsLoading;
        private bool _dueItemsError;
        private bool _studyStatsLoading;
        private bool _studyStatsError;
        private int _completedCount;
        private int _totalModules;
        private double _averageProgress;
        private int _dueCount;

        public DashboardViewModel(AuthSession auth, LayoutService layout, ApiService api, NavigationService navigation)
            : base(layout)
        {
            _auth = auth;
            _api = api;
            _navigation = navigation;

            OnContinueCommand = new RelayCommand(() => _navigation.Navigate(NavigationKey.Modulos));
            RetryProgramCommand = new RelayCommand(LoadProgram);
            RetryDueItemsCommand = new AsyncRelayCommand(LoadDueItemsAsync);
            RetryStudyStatsCommand = new AsyncRelayCommand(LoadStudyStatsAsync);

            LoadProgram();
            _ = LoadDueItemsAsync();
            _ = LoadStudyStatsAsync();
        }

        public string FirstName => _firstName;
        public MasterProgram? Program => _program;
        public Module? ContinueModule => _continueModule;
        public bool ProgramLoading => _programLoading;
        public bool ProgramError => _programError;
        public bool IsProgramReady => !_programLoading && !_programError;

        public bool DueItemsLoading => _dueItemsLoading;
        public bool DueItemsError => _dueItemsError;
        public ObservableCollection<DueItem> DueItems { get; } = new();
        public bool HasDueItems => DueItems.Count > 0;

        public bool StudyStatsLoading => _studyStatsLoading;
        public bool StudyStatsError => _studyStatsError;
        public ObservableCollection<StudyDay> StudyDays { get; } = new();

        public int CompletedCount => _completedCount;
        public int TotalModules => _totalModules;
        public double AverageProgress => _averageProgress;
        public int DueCount => _dueCount;

        public string CompletedStats => $"{_completedCount}/{_totalModules}";
        public string AverageProgressText => $"{_averageProgress:0}%";

        public ICommand OnContinueCommand { get; }
        public ICommand RetryProgramCommand { get; }
        public ICommand RetryDueItemsCommand { get; }
        public ICommand RetryStudyStatsCommand { get; }

        private void LoadProgram()
        {
            _programLoading = true;
            _programError = false;
            RaiseStateChanged();

            _ = LoadProgramAsync();
        }

        private async Task LoadProgramAsync()
        {
            try
            {
                var programId = _auth.ActiveMasterProgramId;
                var programs = await _api.FetchMasterProgramsAsync();
                _program = string.IsNullOrEmpty(programId)
                    ? programs.FirstOrDefault()
                    : programs.FirstOrDefault(p => p.Id == programId);

                ComputeStats(_program);
                _firstName = _auth.User?.Name?.Split(' ')[0] ?? AppText.Dashboard.Greeting;
            }
            catch
            {
                _programError = true;
            }
            finally
            {
                _programLoading = false;
                RaiseStateChanged();
            }
        }

        private void ComputeStats(MasterProgram? program)
        {
            var modules = program?.Modules ?? new List<Module>();

            var inProgress = modules.Where(m => m.IsInProgress).OrderByDescending(m => m.Progress).ToList();
            var nextUp = modules.Where(m => m.IsNotStarted).OrderBy(m => m.Id, StringComparer.Ordinal).ToList();
            _continueModule = inProgress.FirstOrDefault() ?? nextUp.FirstOrDefault() ?? modules.FirstOrDefault();

            _completedCount = modules.Count(m => m.IsCompleted);
            _totalModules = modules.Count;
            _averageProgress = _totalModules == 0 ? 0 : Math.Round(modules.Sum(m => m.Progress) / _totalModules);
        }

        private async Task LoadDueItemsAsync()
        {
            _dueItemsLoading = true;
            _dueItemsError = false;
            OnPropertyChanged(nameof(DueItemsLoading));
            OnPropertyChanged(nameof(DueItemsError));
            try
            {
                var list = await _api.FetchDueItemsAsync();
                DueItems.Clear();
                foreach (var item in list) DueItems.Add(item);
                _dueCount = list.Count;
                OnPropertyChanged(nameof(DueCount));
                OnPropertyChanged(nameof(HasDueItems));
            }
            catch
            {
                _dueItemsError = true;
            }
            finally
            {
                _dueItemsLoading = false;
                OnPropertyChanged(nameof(DueItemsLoading));
                OnPropertyChanged(nameof(DueItemsError));
            }
        }

        private async Task LoadStudyStatsAsync()
        {
            _studyStatsLoading = true;
            _studyStatsError = false;
            OnPropertyChanged(nameof(StudyStatsLoading));
            OnPropertyChanged(nameof(StudyStatsError));
            try
            {
                var list = await _api.FetchStudyStatsAsync();
                StudyDays.Clear();
                foreach (var day in list) StudyDays.Add(day);
            }
            catch
            {
                _studyStatsError = true;
            }
            finally
            {
                _studyStatsLoading = false;
                OnPropertyChanged(nameof(StudyStatsLoading));
                OnPropertyChanged(nameof(StudyStatsError));
            }
        }

        private void RaiseStateChanged()
        {
            OnPropertyChanged(nameof(ProgramLoading));
            OnPropertyChanged(nameof(ProgramError));
            OnPropertyChanged(nameof(IsProgramReady));
            OnPropertyChanged(nameof(Program));
            OnPropertyChanged(nameof(ContinueModule));
            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(TotalModules));
            OnPropertyChanged(nameof(AverageProgress));
            OnPropertyChanged(nameof(DueCount));
            OnPropertyChanged(nameof(CompletedStats));
            OnPropertyChanged(nameof(AverageProgressText));
        }
    }
}