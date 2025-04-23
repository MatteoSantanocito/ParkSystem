namespace ParkSystemApp;

using ParkSystemApp.Services;
using ParkSystemApp.Models;
using System.Diagnostics;

public partial class AttrazionePage : ContentPage, IQueryAttributable
{
    private readonly ApiService _apiService;
    private bool isBooked = false;
    private int currentRating = 0;


    public AttrazionePage()
	{
		InitializeComponent();
        _apiService = new ApiService();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (isBooked)
        {
            BookingLabel.Text = "Hai prenotato. Vuoi annullare?";
            BookingLabel.IsVisible = true;
            BookButton.IsVisible = false;
            CancelButton.IsVisible = true;
        }
        else
        {
            BookingLabel.Text = "Vuoi prenotare un posto?";
            BookingLabel.IsVisible = true;
            BookButton.IsVisible = true;
            CancelButton.IsVisible = false;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("attrazione", out var obj) && obj is Attrazione attr)
        {
            BindingContext = attr;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFriendsBookings();
    }

    private async Task LoadFriendsBookings()
    {
        try
        {
            FriendsBookingsFrame.IsVisible = true;
            
            if (BindingContext is Attrazione attr)
            {
                var friends = await _apiService.GetFriendsBookingsAsync(attr.ID);
                
                // Modifica questa linea:
                FriendsBookingsCollection.ItemsSource = friends ?? new List<ApiService.FriendBookingInfo>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errore nel caricamento amici: {ex}");
            FriendsBookingsCollection.ItemsSource = new List<ApiService.FriendBookingInfo>();
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
            
            var result = await _apiService.BookAttractionAsync(attr.ID);
            await DisplayAlert("", " Prenotazione Effettuata!", "OK");
            
            // Ricarica la lista degli amici prenotati
            LoadFriendsBookings();
        }
        else
        {
            await DisplayAlert("Errore", "Impossibile recuperare i dettagli dell'attrazione.", "OK");
        }
    }


    private async void OnCancelClicked(object sender, EventArgs e)
        {
            isBooked = false;
            UpdateUI();
            
            await _apiService.DeleteBookingAsync();
            
            // Ricarica la lista degli amici prenotati
            LoadFriendsBookings();
        }

    private async void OnFriendshipClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///FriendshipPage");
    }


    private async void OnStarClicked(object sender, EventArgs e)
    {
        if (BindingContext is Attrazione attr)
        {
            var button = sender as Button;
            if (button != null)
            {
                int rating = Convert.ToInt32(button.CommandParameter);
                currentRating = rating;
                UpdateStars(rating);

                try
                {
                    // Invia la valutazione
                    ApiService apiService = new ApiService();
                    await apiService.SendRating(attr.ID, rating);
                    
                    // Nascondi le stelle e mostra il messaggio
                    StarsContainer.IsVisible = false;
                    RatingTitle.IsVisible = false;
                    ThankYouLabel.IsVisible = true;
                    
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Errore", $"Recensione non inviata: {ex.Message}", "OK");
                }
            }
        }
    }

    private void UpdateStars(int rating)
    {
        // Resetta tutte le stelle a grigio
        Star1.TextColor = Colors.LightGray;
        Star2.TextColor = Colors.LightGray;
        Star3.TextColor = Colors.LightGray;
        Star4.TextColor = Colors.LightGray;
        Star5.TextColor = Colors.LightGray;

        // Colora le stelle fino al rating selezionato
        if (rating >= 1) Star1.TextColor = Colors.Gold;
        if (rating >= 2) Star2.TextColor = Colors.Gold;
        if (rating >= 3) Star3.TextColor = Colors.Gold;
        if (rating >= 4) Star4.TextColor = Colors.Gold;
        if (rating >= 5) Star5.TextColor = Colors.Gold;
    }
}