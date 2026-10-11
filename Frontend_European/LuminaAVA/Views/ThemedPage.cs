using Microsoft.Maui.Controls;

namespace LuminaAVA.Views;

public abstract class ThemedPage : ContentPage
{
    protected ThemedPage()
    {
        LuminaTheme.Changed += OnThemeChanged;
    }

    protected abstract View BuildRoot();

    void OnThemeChanged()
    {
        if (Dispatcher.IsDispatchRequired)
            Dispatcher.Dispatch(() => Content = BuildRoot());
        else
            Content = BuildRoot();
    }
}
