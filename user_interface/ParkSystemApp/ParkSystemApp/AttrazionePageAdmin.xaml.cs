namespace ParkSystemApp;

using ParkSystemApp.Services;
using ParkSystemApp.Models;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class AttrazionePageAdmin : ContentPage, IQueryAttributable, INotifyPropertyChanged
{
    private readonly ApiService _apiService;
    private bool isEditing = false;
    private readonly Action<int> _onEliminaCallback;
    private Attrazione _attrazione;
    public Attrazione Attrazione
    {
        get => _attrazione;
        set
        {
            _attrazione = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Attrazione.ImagePath)); // <-- forza aggiornamento
        }
    }


    public AttrazionePageAdmin(Attrazione attrazione, Action<int> onEliminaCallback)
	{
        InitializeComponent();
        _apiService = new ApiService();
        BindingContext = this; // <--- PER BINDING A PROPRIETÀ PUBBLICHE
        Attrazione = attrazione;
        _onEliminaCallback = onEliminaCallback;

        _ = CaricaStatisticheGiornaliere(attrazione.ID);
    }


    public event PropertyChangedEventHandler PropertyChanged;

    private DailyStat _dailyStat;
    public DailyStat DailyStat
    {
        get => _dailyStat;
        set
        {
            _dailyStat = value;
            OnPropertyChanged();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));




    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("attrazione", out var obj) && obj is Attrazione attr)
        {
            BindingContext = attr;
        }
    }


    private async Task CaricaStatisticheGiornaliere(int idAttrazione)
    {
        try
        {
            DailyStat = await _apiService.GetDailyStatsByAttrazioneIdAsync(idAttrazione);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Errore", ex.Message, "OK");
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