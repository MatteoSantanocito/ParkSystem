namespace ParkSystemApp;

using ParkSystemApp.Services;
using ParkSystemApp.Models;
using System.Diagnostics;

public partial class AttrazionePageAdmin : ContentPage, IQueryAttributable
{
    private readonly ApiService _apiService;
    private bool isEditing = false;
    private readonly Action<int> _onEliminaCallback;

    public AttrazionePageAdmin(Attrazione attrazione, Action<int> onEliminaCallback)
	{
        InitializeComponent();
        _apiService = new ApiService();
        BindingContext = attrazione;
        _onEliminaCallback = onEliminaCallback;
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
        await Shell.Current.GoToAsync("//ProfiloAdmin", false);
    }


    private async void OnEliminaAttrazioneClicked(object sender, EventArgs e)
    {
        bool conferma = await DisplayAlert("Conferma eliminazione", "Sei sicuro di voler eliminare questa attrazione?", "Sì", "Annulla");
        if (!conferma) return;

        if (BindingContext is Attrazione attrazione)
        {
            var result = await _apiService.EliminaAttrazioneAsync(attrazione.ID);
            if (result == "OK")
            {
                _onEliminaCallback?.Invoke(attrazione.ID); // richiama la callback
                await DisplayAlert("Eliminato", "L'attrazione è stata eliminata.", "OK");

                // Torna alla pagina precedente
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Errore", result, "OK");
            }
        }
    }

    private async void OnModificaAttrazioneClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Modifica", "Funzione di modifica in costruzione.", "OK");
        isEditing = true;
        ToggleEditingMode();
    }

    private void OnAnnullaModificaClicked(object sender, EventArgs e)
    {
        isEditing = false;
        ToggleEditingMode();
    }

    private async void OnConfermaModificaClicked(object sender, EventArgs e)
    {
        isEditing = false;

        bool conferma = await DisplayAlert("Conferma modifica", "Sei sicuro di voler salvare le modifiche?", "Sì", "Annulla");
        if (conferma)
        {
            if (BindingContext is Attrazione attrazione)
            {
                var result = await _apiService.AggiornaAttrazioneAsync(attrazione);
                if (result == "OK")
                {
                    await DisplayAlert("Successo", "L'attrazione è stata modificata.", "OK");
                }
                else
                {
                    await DisplayAlert("Errore", result, "OK");
                }
            }
        }

        ToggleEditingMode();
    }


    private void ToggleEditingMode()
    {
        LabelModeLayout.IsVisible = !isEditing;
        EditModeLayout.IsVisible = isEditing;
        EditButtonsLayout.IsVisible = isEditing;
        ViewButtonsLayout.IsVisible = !isEditing;
    }

}