using System.Diagnostics;

namespace ShellSwipeBackRepro;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnPushClicked(object? sender, EventArgs e)
	{
		Debug.WriteLine($"[REPRO] MainPage.OnPushClicked -> PushAsync(DetailPage), stack={Shell.Current.Navigation.NavigationStack.Count}");
		await Shell.Current.Navigation.PushAsync(new DetailPage());
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Debug.WriteLine($"[REPRO] MainPage.OnAppearing, stack={Shell.Current.Navigation.NavigationStack.Count}");
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Debug.WriteLine($"[REPRO] MainPage.OnDisappearing, stack={Shell.Current.Navigation.NavigationStack.Count}");
	}
}
