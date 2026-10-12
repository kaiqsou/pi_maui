using System.Windows.Input;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class AulaEditViewModel : BaseViewModel
    {
        private Materia? _materia;
        private Aula? _aula;
        private string _titulo = string.Empty;
        private string _descricao = string.Empty;
        private string _videoId = string.Empty;
        private string _duracao = "0";
        private bool _isGratuita;

        public AulaEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
        }

        public string Titulo { get => _titulo; set => SetProperty(ref _titulo, value); }
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
        public string VideoId { get => _videoId; set => SetProperty(ref _videoId, value); }
        public string Duracao { get => _duracao; set => SetProperty(ref _duracao, value); }
        public bool IsGratuita { get => _isGratuita; set => SetProperty(ref _isGratuita, value); }
        public bool IsEdicao => _aula != null;

        public ICommand SalvarCommand { get; }

        public void Carregar(Materia? materia, Aula? aula)
        {
            _materia = materia;
            _aula = aula;
            Title = aula == null ? "Nova aula" : "Editar aula";
            OnPropertyChanged(nameof(IsEdicao));

            Titulo = aula?.Titulo ?? string.Empty;
            Descricao = aula?.Descricao ?? string.Empty;
            VideoId = aula?.VideoId ?? string.Empty;
            Duracao = (aula?.DuracaoSegundos ?? 0).ToString();
            IsGratuita = aula?.IsGratuita ?? false;
        }

        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Titulo))
            {
                await Shell.Current.DisplayAlert("Aula", "Informe o título.", "OK");
                return;
            }

            var duracao = int.TryParse(Duracao, out var d) ? d : 0;

            if (_aula == null && _materia != null)
            {
                MockDataService.AddAula(_materia, new Aula
                {
                    Titulo = Titulo,
                    Descricao = Descricao,
                    VideoId = VideoId,
                    DuracaoSegundos = duracao,
                    IsGratuita = IsGratuita
                });
            }
            else if (_aula != null)
            {
                _aula.Titulo = Titulo;
                _aula.Descricao = Descricao;
                _aula.VideoId = VideoId;
                _aula.DuracaoSegundos = duracao;
                _aula.IsGratuita = IsGratuita;
                MockDataService.UpdateAula(_aula);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
