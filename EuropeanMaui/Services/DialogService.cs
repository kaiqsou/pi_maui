using EuropeanMaui.Services;

namespace EuropeanMaui.Services
{
    /// <summary>
    /// Exibe alertas/diálogos ancorados na página raiz da janela.
    /// </summary>
    public class DialogService
    {
        private static Page? RootPage =>
            Application.Current?.Windows.FirstOrDefault()?.Page;

        public Task DisplayAlertAsync(string title, string message, string cancel)
            => RootPage?.DisplayAlertAsync(title, message, cancel) ?? Task.CompletedTask;

        public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
            => RootPage?.DisplayAlertAsync(title, message, accept, cancel) ?? Task.FromResult(false);
    }
}