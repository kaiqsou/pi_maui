using Microsoft.Extensions.DependencyInjection;

namespace LuminaAVA;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		window.Height = 700;
		window.Width = 400;
        return window;
	}
}