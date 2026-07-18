using System.Diagnostics;

namespace ShellSwipeBackRepro;

public partial class DetailPage : ContentPage
{
	public DetailPage()
	{
		InitializeComponent();
		Debug.WriteLine($"[REPRO] DetailPage..ctor, stack={Shell.Current.Navigation.NavigationStack.Count}");
	}

	~DetailPage()
	{
		Debug.WriteLine("[REPRO] DetailPage.~DetailPage (finalized / collected)");
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		var count = Shell.Current.Navigation.NavigationStack.Count;
		StackLabel.Text = $"NavigationStack.Count = {count}";
		Debug.WriteLine($"[REPRO] DetailPage.OnAppearing, stack={count}");
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Debug.WriteLine($"[REPRO] DetailPage.OnDisappearing, stack={Shell.Current.Navigation.NavigationStack.Count}");
	}
}
