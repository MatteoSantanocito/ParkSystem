using ParkSystemApp.Models;
using ParkSystemApp.Services;
using Microsoft.Maui.Controls;
using System.Linq;

namespace ParkSystemApp
{
    public partial class FriendshipPage : ContentPage
    {
        private ApiService _apiService;

        public FriendshipPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadFriendships();
        }

        private async void LoadFriendships()
        {
            try
            {
                var response = await _apiService.GetFriendshipsAsync();
                
                // Trasforma gli amici accettati per la visualizzazione
                var acceptedFriends = response.Accepted?.Select(f => new 
                {
                    f.IdRichiesta,
                    FullName = $"{f.FullName}",
                    TipoAvventura = $"Tipo di avventura: {f.TipoAvventura}",
                    AmicoDa = $"Amico da: {(DateTime.Now - f.DataAccettazione).Days} giorni"
                }).ToList();
                
                // Trasforma le richieste pendenti per la visualizzazione
                var pendingRequests = response.Pending?.Select(p => new 
                {
                    p.IdRichiesta,
                    FullName = $"{p.FullName}",
                    DataRichiesta = $"Richiesta il: {p.DataRichiesta:dd/MM/yyyy}"
                }).ToList();

                AcceptedFriendsCollectionView.ItemsSource = acceptedFriends;
                PendingRequestsCollectionView.ItemsSource = pendingRequests;
                NoPendingLabel.IsVisible = pendingRequests == null || !pendingRequests.Any();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Impossibile caricare le amicizie: {ex.Message}", "OK");
            }
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FriendCodeEntry.Text))
            {
                try
                {
                    SearchResultFrame.IsVisible = false;
                    var user = await _apiService.SearchUserAsync(FriendCodeEntry.Text.Trim());
                    if (user != null)
                    {
                        SearchResultLabel.Text = $"{user.Nome} {user.Cognome}";
                        SearchResultSubLabel.Text = $"Tipo avventura: {user.TipoAvventura}"; 
                        SearchResultFrame.IsVisible = true;
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Errore", ex.Message, "OK");
                    SearchResultFrame.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("Attenzione", "Inserisci un codice utente valido", "OK");
            }
        }

        private async void OnAddFriendClicked(object sender, EventArgs e)
        {

            // Rimuovi il TryParse e accetta direttamente la stringa
            if (!string.IsNullOrWhiteSpace(FriendCodeEntry.Text))
            {
                try
                {
                    // Usa direttamente il codice come stringa
                    var result = await _apiService.SendFriendRequestAsync(FriendCodeEntry.Text.Trim());
                    await DisplayAlert("Successo", result, "OK");
                    SearchResultFrame.IsVisible = false;
                    LoadFriendships();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Errore", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Errore", "Inserisci un codice amico valido", "OK");
            }
        }


        private async void OnAcceptRequestClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int requestId)
            {
                try
                {
                    var result = await _apiService.AcceptFriendRequestAsync(requestId);
                    await DisplayAlert("Successo", result, "OK");
                    LoadFriendships();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Errore", ex.Message, "OK");
                }
            }
        }

        private async void OnRejectRequestClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int requestId)
            {
                try
                {
                    var result = await _apiService.RejectFriendRequestAsync(requestId);
                    await DisplayAlert("Info", result, "OK");
                    LoadFriendships();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Errore", ex.Message, "OK");
                }
            }
        }

        private async void OnBackToMainPageClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}", false);
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(Profilo), false);
        }

    }
}
