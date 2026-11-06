namespace European.Services
{
    /// <summary>Rotas do Shell e atalhos de navegação usados pelos ViewModels.</summary>
    public static class Navegacao
    {
        // Áreas: raiz da pilha, declaradas no AppShell.xaml (navegar com "//" troca a pilha inteira).
        public const string Login = "//login";
        public const string Coordenacao = "//coordenacao";
        public const string Professor = "//professor";
        public const string Aluno = "//main";
        public const string SomenteCertificados = "//certificados";
        public const string Aguardando = "//aguardando";
        public const string Pagamento = "//pagamento";
        public const string NovaSenha = "//novasenha";

        // Telas empilhadas sobre a área atual, registradas em AppShell.xaml.cs.
        public const string Cadastro = "cadastro";
        public const string CursoInfo = "cursoinfo";
        public const string PrimeiroAcesso = "primeiroacesso";
        public const string Perfil = "perfil";
        public const string Aula = "aula";
        public const string ProvaRealizar = "provarealizar";
        public const string Certificado = "certificado";
        public const string Curso = "curso";
        public const string CursoEdit = "cursoedit";
        public const string ModuloEdit = "moduloedit";
        public const string MateriaEdit = "materiaedit";
        public const string AulaEdit = "aulaedit";
        public const string ProvaEdit = "provaedit";
        public const string ProvaResultados = "provaresultados";
        public const string Correcao = "correcao";
        public const string SalaEdit = "salaedit";
        public const string AlunoAcesso = "alunoacesso";
        public const string Solicitacao = "solicitacao";
        public const string ProfessorAcesso = "professoracesso";
        public const string ContaNova = "contanova";
        public const string Notificacoes = "notificacoes";

        /// <summary>Abre a rota passando parâmetros (chave, valor); valores nulos são ignorados.</summary>
        public static Task IrAsync(string rota, params (string Chave, object? Valor)[] parametros)
        {
            var dados = parametros
                .Where(p => p.Valor != null)
                .ToDictionary(p => p.Chave, p => p.Valor!);

            return dados.Count == 0
                ? Shell.Current.GoToAsync(rota)
                : Shell.Current.GoToAsync(rota, dados);
        }

        /// <summary>Volta para a tela anterior, opcionalmente devolvendo parâmetros a ela.</summary>
        public static Task VoltarAsync(params (string Chave, object? Valor)[] parametros)
            => IrAsync("..", parametros);

        /// <summary>Lê um parâmetro recebido na navegação (null se ausente ou de outro tipo).</summary>
        public static T? Obter<T>(this IDictionary<string, object> parametros, string chave)
            => parametros.TryGetValue(chave, out var valor) && valor is T tipado ? tipado : default;
    }
}
