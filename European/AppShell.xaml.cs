using European.Services;
using European.Views;

namespace European
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Acesso
            Routing.RegisterRoute(Navegacao.Cadastro, typeof(CadastroPage));
            Routing.RegisterRoute(Navegacao.CursoInfo, typeof(CursoInfoPage));
            Routing.RegisterRoute(Navegacao.PrimeiroAcesso, typeof(PrimeiroAcessoPage));
            Routing.RegisterRoute(Navegacao.Perfil, typeof(PerfilPage));

            // Aluno
            Routing.RegisterRoute(Navegacao.Aula, typeof(AulaDetalhePage));
            Routing.RegisterRoute(Navegacao.ProvaRealizar, typeof(ProvaRealizarPage));
            Routing.RegisterRoute(Navegacao.Certificado, typeof(CertificadoDetalhePage));

            // Conteúdo (coordenação e professor)
            Routing.RegisterRoute(Navegacao.Curso, typeof(CursoDetalhePage));
            Routing.RegisterRoute(Navegacao.CursoEdit, typeof(CursoEditPage));
            Routing.RegisterRoute(Navegacao.ModuloEdit, typeof(ModuloEditPage));
            Routing.RegisterRoute(Navegacao.MateriaEdit, typeof(MateriaEditPage));
            Routing.RegisterRoute(Navegacao.AulaEdit, typeof(AulaEditPage));

            // Professor
            Routing.RegisterRoute(Navegacao.ProvaEdit, typeof(ProvaEditPage));
            Routing.RegisterRoute(Navegacao.ProvaResultados, typeof(ProvaResultadosPage));
            Routing.RegisterRoute(Navegacao.Correcao, typeof(CorrecaoPage));
            Routing.RegisterRoute(Navegacao.Sala, typeof(SalaDetalhePage));
            Routing.RegisterRoute(Navegacao.SalaEdit, typeof(SalaEditPage));

            // Coordenação
            Routing.RegisterRoute(Navegacao.AlunoAcesso, typeof(AlunoAcessoPage));
            Routing.RegisterRoute(Navegacao.Solicitacao, typeof(SolicitacaoPage));
            Routing.RegisterRoute(Navegacao.ProfessorAcesso, typeof(ProfessorAcessoPage));
            Routing.RegisterRoute(Navegacao.ContaNova, typeof(ContaNovaPage));

            // Todos os perfis
            Routing.RegisterRoute(Navegacao.Notificacoes, typeof(NotificacoesPage));

            // Cada tela só abre para o perfil que pode usá-la (AcessoService.PodeAbrir)
            Navigating += (_, e) =>
            {
                if (e.CanCancel && !AcessoService.PodeAbrir(e.Target.Location))
                    e.Cancel();
            };
        }
    }
}
