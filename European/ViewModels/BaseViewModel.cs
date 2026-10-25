using System.ComponentModel;
using System.Runtime.CompilerServices;
using European.Services;
using MongoDB.Driver;

namespace European.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Executa uma operação no banco mostrando o indicador de carregamento e um aviso em caso de falha.</summary>
        protected async Task<bool> ExecutarAsync(Func<Task> operacao)
        {
            if (IsBusy)
                return false;

            IsBusy = true;
            try
            {
                await operacao();
                return true;
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(GetType().Name, ex);
                var mensagem = ex is TimeoutException or MongoConnectionException
                    ? "Não foi possível conectar ao banco de dados. Verifique se o MongoDB está em execução."
                    : $"Ocorreu um erro: {ex.Message}";

                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Erro", mensagem, "OK");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected static Task AvisarAsync(string titulo, string mensagem)
            => Shell.Current.DisplayAlert(titulo, mensagem, "OK");

        protected static Task<bool> ConfirmarAsync(string mensagem, string aceitar = "Excluir")
            => Shell.Current.DisplayAlert("Confirmar", mensagem, aceitar, "Cancelar");

        protected static Task VoltarAsync() => Shell.Current.GoToAsync("..");
    }
}
