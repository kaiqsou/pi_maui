using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Perfil do usuário logado: dados da conta, matrícula (aluno) e troca de senha.</summary>
    public partial class PerfilViewModel : BaseViewModel
    {
        public PerfilViewModel()
        {
            Title = "Perfil";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Nome), nameof(Email), nameof(RoleTexto), nameof(Iniciais), nameof(IsAluno),
            nameof(MembroDesdeTexto), nameof(NascimentoTexto), nameof(TemNascimento))]
        public partial Usuario? Usuario { get; private set; }

        public string Nome => Usuario?.Nome ?? string.Empty;
        public string Email => Usuario?.Email ?? string.Empty;
        public string Iniciais => Usuario?.Iniciais ?? "?";
        public bool IsAluno => Usuario?.Roles == Roles.Aluno;
        public string RoleTexto => Usuario?.Roles switch
        {
            Roles.Coordenador or Roles.Administrador => "Coordenação",
            Roles.Professor => "Professor",
            Roles.Aluno => "Aluno",
            _ => string.Empty
        };

        public string MembroDesdeTexto => Usuario?.CreatedAt.ToString("dd/MM/yyyy") ?? string.Empty;
        public string NascimentoTexto => Usuario?.DataNascimento?.ToString("dd/MM/yyyy") ?? string.Empty;
        public bool TemNascimento => Usuario?.DataNascimento != null;

        // ---- Matrícula (aluno) ----

        [ObservableProperty]
        public partial bool TemMatricula { get; private set; }

        [ObservableProperty]
        public partial string CursoNome { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string SituacaoTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial bool CursoConcluido { get; private set; }

        [ObservableProperty]
        public partial string MatriculadoEmTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial double Progresso { get; private set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemTurma))]
        public partial string TurmaTexto { get; private set; } = string.Empty;

        public bool TemTurma => !string.IsNullOrEmpty(TurmaTexto);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemAcessoAte))]
        public partial string AcessoAteTexto { get; private set; } = string.Empty;

        public bool TemAcessoAte => !string.IsNullOrEmpty(AcessoAteTexto);

        // ---- Alterar senha ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SetaSenha))]
        public partial bool AlterandoSenha { get; set; }

        /// <summary>Seta do item "Alterar senha": muda quando a seção está aberta ou fechada.</summary>
        public string SetaSenha => AlterandoSenha ? "" : "";

        [ObservableProperty]
        public partial string SenhaAtual { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string NovaSenha { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ConfirmacaoSenha { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMensagemSenha))]
        public partial string MensagemSenha { get; private set; } = string.Empty;

        public bool TemMensagemSenha => !string.IsNullOrEmpty(MensagemSenha);

        partial void OnAlterandoSenhaChanged(bool value)
        {
            if (value)
                return;

            SenhaAtual = string.Empty;
            NovaSenha = string.Empty;
            ConfirmacaoSenha = string.Empty;
            MensagemSenha = string.Empty;
        }

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
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

        [RelayCommand]
        private Task Logout() => AcessoService.SairAsync();

        [RelayCommand]
        private Task Voltar() => VoltarAsync();

        [RelayCommand]
        private void AlternarSenha() => AlterandoSenha = !AlterandoSenha;

        [RelayCommand]
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
    }
}
