using System.Collections.ObjectModel;
using Npgsql;
using ParkSystemApp.Models;

namespace ParkSystemApp
{
    public partial class MainPage : ContentPage
    {



        public ObservableCollection<Attrazione> Attrazioni { get; set; }


        public Command<Attrazione> OpenDetailCommand { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            Attrazioni = new ObservableCollection<Attrazione>();
            BindingContext = this;
            CaricaDati();
            OpenDetailCommand = new Command<Attrazione>(ExecuteOpenDetailCommand);

        }

        private async void ExecuteOpenDetailCommand(Attrazione attrazione)
        {
            var navigationParameters = new Dictionary<string, object>
        {
            { "attrazione", attrazione }
        };
            await Shell.Current.GoToAsync(nameof(AttrazionePage), navigationParameters);
        }


        private async void CaricaDati()
        {
            /// Put in host localhost if you use IOS
            var connectionString = "Host=10.0.2.2;Port=5433;Database=parksystem_db;Username=parksys;Password=system";


            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM attrazioni";
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();


                while(await reader.ReadAsync())
                {
                    var attrazione = new Attrazione
                    {
                        Nome = reader.GetString(1),
                        Descrizione = reader.GetString(2),
                        Tipologia = reader.GetString(3),
                        Tematica = reader.GetString(4),
                        MinimumAge = reader.GetInt32(5),
                        State = reader.GetString(6),
                        HourCapacity = reader.GetInt32(7),

                    };

                    Attrazioni.Add(attrazione);
                }
            }

            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Non è stato possibile recuperare i dati: {ex.Message}", "OK");
            }

        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            // Naviga alla pagina di registrazione
            await Shell.Current.GoToAsync(nameof(Profilo), false);
        }



    }



}

