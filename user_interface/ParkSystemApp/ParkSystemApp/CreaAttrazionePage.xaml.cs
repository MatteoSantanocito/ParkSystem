namespace ParkSystemApp;

using ParkSystemApp.Services;
using ParkSystemApp.Models;
using System.Diagnostics;


public partial class CreaAttrazionePage : ContentPage
{

    private readonly ApiService _apiService;
    private Attrazione nuovaAttrazione;

    public CreaAttrazionePage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        nuovaAttrazione = new Attrazione(); // Creazione dell'oggetto
        BindingContext = nuovaAttrazione;   // Assegnazione al BindingContext
    }
    private async void OnBackToMainPageClicked(Object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(MainPageAdmin)}", false);
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(Profilo), false);
    }

    private async void OnSalvaClicked(object sender, EventArgs e)
    {
        try
        {
            await _apiService.InserisciAttrazioneAsync(nuovaAttrazione);

            await DisplayAlert("Successo", "Attrazione inserita con successo!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Errore", $"Errore durante il salvataggio: {ex.Message}", "OK");
        }
    }
}
