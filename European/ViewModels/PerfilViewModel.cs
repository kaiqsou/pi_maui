using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class PerfilViewModel : BaseViewModel
    {
        private Usuario? _usuario;
        private bool _temMatricula;
        private string _cursoNome = string.Empty;
        private string _situacaoTexto = string.Empty;
        private bool _cursoConcluido;
        private string _matriculadoEmTexto = string.Empty;
        private double _progresso;
        private string _turmaTexto = string.Empty;
        private string _acessoAteTexto = string.Empty;
        private bool _alterandoSenha;
        private string _senhaAtual = string.Empty;
        private string _novaSenha = string.Empty;
        private string _confirmacaoSenha = string.Empty;
        private string _mensagemSenha = string.Empty;

        public PerfilViewModel()
        {
            Title = "Perfil";
            LogoutCommand = new Command(async () => await AcessoService.SairAsync());
            VoltarCommand = new Command(async () => await VoltarAsync());
            AlternarSenhaCommand = new Command(() => AlterandoSenha = !AlterandoSenha);
            SalvarSenhaCommand = new Command(async () => await SalvarSenhaAsync());
        }

        public Usuario? Usuario
        {
            get => _usuario;
            set
            {
                if (SetProperty(ref _usuario, value))
                {
                    OnPropertyChanged(nameof(Nome));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(RoleTexto));
                    OnPropertyChanged(nameof(Iniciais));
                    OnPropertyChanged(nameof(IsAluno));
                    OnPropertyChanged(nameof(MembroDesdeTexto));
                    OnPropertyChanged(nameof(NascimentoTexto));
                    OnPropertyChanged(nameof(TemNascimento));
                }
            }
        }

        public string Nome => Usuario?.Nome ?? string.Empty;
        public string Email => Usuario?.Email ?? string.Empty;
        public string RoleTexto => Usuario?.Roles switch
        {
            Roles.Coordenador or Roles.Administrador => "Coordenação",
            Roles.Professor => "Professor",
            Roles.Aluno => "Aluno",
            _ => string.Empty
        };

        public string Iniciais => Usuario?.Iniciais ?? "?";
        public bool IsAluno => Usuario?.Roles == Roles.Aluno;

        public string MembroDesdeTexto => Usuario?.CreatedAt.ToString("dd/MM/yyyy") ?? string.Empty;
        public string NascimentoTexto => Usuario?.DataNascimento?.ToString("dd/MM/yyyy") ?? string.Empty;
        public bool TemNascimento => Usuario?.DataNascimento != null;

        // ---- Matrícula (aluno) ----

        public bool TemMatricula
        {
            get => _temMatricula;
            private set => SetProperty(ref _temMatricula, value);
        }

        public string CursoNome
        {
            get => _cursoNome;
            private set => SetProperty(ref _cursoNome, value);
        }

        public string SituacaoTexto
        {
            get => _situacaoTexto;
            private set => SetProperty(ref _situacaoTexto, value);
        }

        public bool CursoConcluido
        {
            get => _cursoConcluido;
            private set => SetProperty(ref _cursoConcluido, value);
        }

        public string MatriculadoEmTexto
        {
            get => _matriculadoEmTexto;
            private set => SetProperty(ref _matriculadoEmTexto, value);
        }

        public double Progresso
        {
            get => _progresso;
            private set => SetProperty(ref _progresso, value);
        }

        public string TurmaTexto
        {
            get => _turmaTexto;
            private set
            {
                if (SetProperty(ref _turmaTexto, value))
                    OnPropertyChanged(nameof(TemTurma));
            }
        }

        public bool TemTurma => !string.IsNullOrEmpty(TurmaTexto);

        public string AcessoAteTexto
        {
            get => _acessoAteTexto;
            private set
            {
                if (SetProperty(ref _acessoAteTexto, value))
                    OnPropertyChanged(nameof(TemAcessoAte));
            }
        }

        public bool TemAcessoAte => !string.IsNullOrEmpty(AcessoAteTexto);

        // ---- Alterar senha ----

        public bool AlterandoSenha
        {
            get => _alterandoSenha;
            set
            {
                if (SetProperty(ref _alterandoSenha, value))
                {
                    OnPropertyChanged(nameof(SetaSenha));
                    if (!value)
                        LimparSenha();
                }
            }
        }

        /// <summary>Seta do item "Alterar senha": para baixo quando aberto, para o lado quando fechado.</summary>
        public string SetaSenha => AlterandoSenha ? "" : "";

        public string SenhaAtual { get => _senhaAtual; set => SetProperty(ref _senhaAtual, value); }
        public string NovaSenha { get => _novaSenha; set => SetProperty(ref _novaSenha, value); }
        public string ConfirmacaoSenha { get => _confirmacaoSenha; set => SetProperty(ref _confirmacaoSenha, value); }

        public string MensagemSenha
        {
            get => _mensagemSenha;
            private set
            {
                if (SetProperty(ref _mensagemSenha, value))
                    OnPropertyChanged(nameof(TemMensagemSenha));
            }
        }

        public bool TemMensagemSenha => !string.IsNullOrEmpty(MensagemSenha);

        public ICommand LogoutCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand AlternarSenhaCommand { get; }
        public ICommand SalvarSenhaCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            Usuario = SessionService.UsuarioAtual;
            AlterandoSenha = false;
            if (Usuario == null || !IsAluno)
                return;

            // a matrícula vigente (ativa ou concluída dentro da validade); senão, a mais recente
            var matriculas = await MatriculaRepositorio.DoAlunoAsync(Usuario.Id);
            var matricula = matriculas.FirstOrDefault(m => m.AcessoAulasVigente) ?? matriculas.FirstOrDefault();

            TemMatricula = matricula != null;
            if (matricula == null)
                return;

            CursoNome = matricula.CursoNome;
            CursoConcluido = matricula.Status == StatusMatricula.Concluida;
            SituacaoTexto = matricula.Status switch
            {
                StatusMatricula.Ativa => "Em andamento",
                StatusMatricula.Concluida => "Concluído",
                StatusMatricula.Trancada => "Trancado",
                _ => "Encerrado"
            };
            MatriculadoEmTexto = matricula.DataMatricula.ToString("dd/MM/yyyy");
            Progresso = matricula.ProgressoPercentual;
            AcessoAteTexto = matricula.AcessoAte?.ToString("dd/MM/yyyy") ?? string.Empty;

            var salas = await SalaRepositorio.DoAlunoAsync(Usuario.Id);
            TurmaTexto = string.Join(", ", salas.Where(s => s.CursoId == null || s.CursoId == matricula.CursoId).Select(s => s.Nome));
        });

        private async Task SalvarSenhaAsync()
        {
            var usuario = Usuario;
            if (usuario == null)
                return;

            if (!SenhaService.Verificar(SenhaAtual, usuario.SenhaHash))
            {
                MensagemSenha = "A senha atual está incorreta.";
                return;
            }

            if (!SenhaService.EhValida(NovaSenha, out var erro))
            {
                MensagemSenha = erro;
                return;
            }

            if (NovaSenha != ConfirmacaoSenha)
            {
                MensagemSenha = "A confirmação não confere com a nova senha.";
                return;
            }

            var salvou = await ExecutarAsync(async () =>
            {
                var hash = SenhaService.GerarHash(NovaSenha);
                await UsuarioRepositorio.AtualizarSenhaAsync(usuario.Id, hash);
                usuario.SenhaHash = hash;
            });

            if (salvou)
            {
                AlterandoSenha = false;
                await AvisarAsync("Senha", "Sua senha foi alterada.");
            }
        }

        private void LimparSenha()
        {
            SenhaAtual = string.Empty;
            NovaSenha = string.Empty;
            ConfirmacaoSenha = string.Empty;
            MensagemSenha = string.Empty;
        }
    }
}
