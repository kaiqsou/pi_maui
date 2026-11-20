using European.Services;
using European.ViewModels;

namespace European.Views
{
    /// <summary>Página base: sempre que aparece, avisa o ViewModel (que recarrega os dados, se precisar).</summary>
    public class BasePage : ContentPage
    {
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is not BaseViewModel viewModel)
                return;

            try
            {
                await viewModel.AoAparecerAsync();
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(GetType().Name, ex);
            }
        }
    }
}
