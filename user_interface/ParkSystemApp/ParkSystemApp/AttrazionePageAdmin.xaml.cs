namespace ParkSystemApp;

using ParkSystemApp.Services;
using ParkSystemApp.Models;
using System.Diagnostics;

public partial class AttrazionePageAdmin : ContentPage, IQueryAttributable
{
    private readonly ApiService _apiService;


    public AttrazionePageAdmin()
	{
        InitializeComponent();
        _apiService = new ApiService();
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("attrazione", out var obj) && obj is Attrazione attr)
        {
            BindingContext = attr;
        }
    }



    private async void OnBackToMainPageClicked(Object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(MainPageAdmin)}", false);
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        // Naviga alla pagina di registrazione
        await Shell.Current.GoToAsync(nameof(Profilo), false);
    }

    
}