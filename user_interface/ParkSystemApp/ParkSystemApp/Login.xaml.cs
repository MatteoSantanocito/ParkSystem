namespace ParkSystemApp;

using Npgsql;
public partial class Login : ContentPage
{
    



    public Login()
    {
        InitializeComponent();

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetAttrazioneAsync();
    }

    public async Task GetAttrazioneAsync()
    {
        var connectionString = "Host=10.0.2.2;Port=5433;Database=parksystem_db;Username=parksys;Password=system";
        /// Put in host localhost if you use IOS
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        var query = "SELECT Descrizione FROM attrazioni WHERE nome = @nome";
        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@nome", "Carosello");

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            DescrizioneLabel.Text = reader.GetString(0);
            Console.WriteLine(DescrizioneLabel.Text);
        }
        else
        {
            Console.WriteLine("Nessuna descrizione trovata per l'attrazione specificata.");
        }
    }





    private void Entry_Focused(object sender, FocusEventArgs e)
    {

        var entry = sender as Entry;

        if (entry != null && entry.Text == entry.Placeholder)
        {
            entry.Text = "";
            entry.TextColor = Color.FromArgb("#000000");
        }
    }



    private async void Button_Clicked(object sender, EventArgs e)
    {
        await GetAttrazioneAsync();

        Shell.Current.GoToAsync(nameof(Registrazione));

    }
}