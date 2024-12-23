namespace ParkSystemApp;

public partial class Registrazione : ContentPage
{
	public Registrazione()
	{
		InitializeComponent();
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync(nameof(Login));
    }
}