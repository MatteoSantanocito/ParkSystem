namespace ParkSystemApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();


            Routing.RegisterRoute(nameof(Registrazione), typeof(Registrazione));
            Routing.RegisterRoute(nameof(Login), typeof(Login));
        }
    }
}
