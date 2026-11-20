using CommunityToolkit.Mvvm.ComponentModel;
using European.Models;
using European.Services;
using MongoDB.Driver;

namespace European.ViewModels
{
    /// <summary>
    /// Base de todos os ViewModels: estado de carregamento, avisos e os dois ganchos usados pelas telas:
    /// <see cref="ReceberParametrosAsync"/> (dados passados na navegação) e <see cref="AoAparecerAsync"/>.
    /// </summary>
    public partial class BaseViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;

        public bool IsNotBusy => !IsBusy;

        /// <summary>Chamado pela BasePage sempre que a tela aparece (inclusive ao voltar de outra tela).</summary>
        public virtual Task AoAparecerAsync() => Task.CompletedTask;

        /// <summary>Recebe os parâmetros da navegação (Navegacao.IrAsync). O Shell entrega ao BindingContext da página.</summary>
        protected virtual Task ReceberParametrosAsync(IDictionary<string, object> parametros) => Task.CompletedTask;

        async void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
        {
            try
            {
                await ReceberParametrosAsync(query);
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(GetType().Name, ex);
            }
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
                    await AvisarAsync("Erro", mensagem);
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Grava e, se deu certo, volta para a tela anterior.</summary>
        protected async Task SalvarEVoltarAsync(Func<Task> gravar)
        {
            if (await ExecutarAsync(gravar))
                await VoltarAsync();
        }

        /// <summary>Pede confirmação, exclui e volta para a tela anterior.</summary>
        protected async Task ExcluirEVoltarAsync(string pergunta, Func<Task> excluir)
        {
            if (await ConfirmarAsync(pergunta) && await ExecutarAsync(excluir))
                await VoltarAsync();
        }

        /// <summary>Coordenação: pede e grava uma senha provisória para o usuário ("aluno" ou "professor").</summary>
        protected async Task RedefinirSenhaProvisoriaAsync(Usuario usuario, string papel)
        {
            var senha = await PerguntarAsync("Nova senha", $"Defina uma senha provisória para {usuario.Nome}.", "Salvar");
            if (string.IsNullOrEmpty(senha))
                return;

            if (!SenhaService.EhValida(senha, out var erro))
            {
                await AvisarAsync("Senha", erro);
                return;
            }

            if (await ExecutarAsync(() => ContaService.RedefinirSenhaAsync(usuario, senha)))
                await AvisarAsync("Senha", $"Senha alterada. Informe a nova senha ao {papel}.");
        }

        protected static Task AvisarAsync(string titulo, string mensagem)
            => Shell.Current.DisplayAlertAsync(titulo, mensagem, "OK");

        protected static Task<bool> ConfirmarAsync(string mensagem, string aceitar = "Excluir")
            => Shell.Current.DisplayAlertAsync("Confirmar", mensagem, aceitar, "Cancelar");

        /// <summary>Pede um texto ao usuário; retorna null se ele cancelar.</summary>
        protected static Task<string?> PerguntarAsync(string titulo, string mensagem, string aceitar = "OK",
            string? placeholder = null, string valorInicial = "", Keyboard? teclado = null)
            => Shell.Current.DisplayPromptAsync(titulo, mensagem, aceitar, "Cancelar", placeholder,
                initialValue: valorInicial, keyboard: teclado ?? Keyboard.Default)!;

        /// <summary>Mostra um menu de opções (a destrutiva, se houver, aparece em destaque); retorna a escolhida.</summary>
        protected static Task<string?> EscolherOpcaoAsync(string titulo, string? destrutiva, params string[] opcoes)
            => Shell.Current.DisplayActionSheetAsync(titulo, "Cancelar", destrutiva, opcoes)!;

        protected static Task VoltarAsync() => Navegacao.VoltarAsync();
    }
}
