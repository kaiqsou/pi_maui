using European.Views;

namespace European
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Acesso
            Routing.RegisterRoute("cadastro", typeof(CadastroPage));
            Routing.RegisterRoute("primeiroacesso", typeof(PrimeiroAcessoPage));
            Routing.RegisterRoute("perfil", typeof(PerfilPage));

            // Aluno
            Routing.RegisterRoute("aula", typeof(AulaDetalhePage));
            Routing.RegisterRoute("provarealizar", typeof(ProvaRealizarPage));
            Routing.RegisterRoute("certificado", typeof(CertificadoDetalhePage));

            // Conteúdo (coordenação e professor)
            Routing.RegisterRoute("curso", typeof(CursoDetalhePage));
            Routing.RegisterRoute("cursoedit", typeof(CursoEditPage));
            Routing.RegisterRoute("moduloedit", typeof(ModuloEditPage));
            Routing.RegisterRoute("materiaedit", typeof(MateriaEditPage));
            Routing.RegisterRoute("aulaedit", typeof(AulaEditPage));

            // Professor
            Routing.RegisterRoute("provaedit", typeof(ProvaEditPage));
            Routing.RegisterRoute("provaresultados", typeof(ProvaResultadosPage));
            Routing.RegisterRoute("salaedit", typeof(SalaEditPage));

            // Coordenação
            Routing.RegisterRoute("alunoacesso", typeof(AlunoAcessoPage));
            Routing.RegisterRoute("professoracesso", typeof(ProfessorAcessoPage));
            Routing.RegisterRoute("contanova", typeof(ContaNovaPage));
        }
    }
}
