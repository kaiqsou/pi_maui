using European.Views;

namespace European
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("catalogo", typeof(CatalogoPage));
            Routing.RegisterRoute("cadastro", typeof(CadastroPage));
            Routing.RegisterRoute("pagamento", typeof(PagamentoPage));
            Routing.RegisterRoute("perfil", typeof(PerfilPage));
            Routing.RegisterRoute("curso", typeof(CursoDetalhePage));
            Routing.RegisterRoute("aula", typeof(AulaDetalhePage));
            Routing.RegisterRoute("certificado", typeof(CertificadoDetalhePage));
            Routing.RegisterRoute("gerenciar", typeof(GerenciarPage));

            Routing.RegisterRoute("cursoedit", typeof(CursoEditPage));
            Routing.RegisterRoute("moduloedit", typeof(ModuloEditPage));
            Routing.RegisterRoute("materiaedit", typeof(MateriaEditPage));
            Routing.RegisterRoute("aulaedit", typeof(AulaEditPage));
            Routing.RegisterRoute("provaedit", typeof(ProvaEditPage));
        }
    }
}
