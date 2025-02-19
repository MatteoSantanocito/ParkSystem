namespace ParkSystemApp;

using ParkSystemApp.Services;
using ParkSystemApp.Models;
public partial class AttrazionePage : ContentPage, IQueryAttributable
{
    private readonly ApiService _apiService;
    private bool isBooked = false;


    public AttrazionePage()
	{
		InitializeComponent();
        _apiService = new ApiService();
        UpdateUI();
    }

    private void UpdateUI()
    {
        BookingLabel.IsVisible = !isBooked;
        BookButton.IsVisible = !isBooked;
        CancelButton.IsVisible = isBooked;
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
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}", false);
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        // Naviga alla pagina di registrazione
        await Shell.Current.GoToAsync(nameof(Profilo), false);
    }

    private async void OnBookClicked(Object sender, EventArgs e)
    {
        if (BindingContext is Attrazione attr)
        {
            isBooked = true;
            UpdateUI();
            // Avvia il timer qui
            var result = await _apiService.BookAttractionAsync(attr.ID);
            // Gestisci il risultato come necessario
            // Ad esempio, mostrare un messaggio di conferma o di errore
            await DisplayAlert("Prenotazione", result, "OK");
        }
        else
        {
            await DisplayAlert("Errore", "Impossibile recuperare i dettagli dell'attrazione.", "OK");
        }
    }


    private void OnCancelClicked(object sender, EventArgs e)
    {
        isBooked = false;
        UpdateUI();
        // Ferma il timer qui

        _apiService.DeleteBookingAsync();
        
    }


}