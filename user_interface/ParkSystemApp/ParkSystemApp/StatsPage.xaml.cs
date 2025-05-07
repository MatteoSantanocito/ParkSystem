namespace ParkSystemApp;
using ParkSystemApp.Services;
using ParkSystemApp.Models;

public partial class StatsPage : ContentPage
{

    private ApiService _apiService; // Aggiungi una dipendenza al tuo ApiService
    private List<GlobalStats> _statsList;

    public StatsPage()
	{
		InitializeComponent();
        _apiService = new ApiService(); // Inizializza ApiService
        CaricaDati();
    }


    private async void CaricaDati()
    {
        try
        {
            _statsList = await _apiService.GetGlobalStatsAsync();

            // Debug: stampa tutto l'oggetto serializzato in JSON
            var jsonDebug = System.Text.Json.JsonSerializer.Serialize(_statsList, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            System.Diagnostics.Debug.WriteLine("Contenuto di _stats:");
            System.Diagnostics.Debug.WriteLine(jsonDebug);


            BindingContext = _statsList;
        }

        catch (Exception ex)
        {
            await DisplayAlert("Errore", ex.Message, "OK");
        }
    }


    private async void OnHomePageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPageAdmin", false);
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProfiloAdmin", false);
    }
}