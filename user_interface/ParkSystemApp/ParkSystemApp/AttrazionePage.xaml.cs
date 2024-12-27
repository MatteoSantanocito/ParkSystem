namespace ParkSystemApp;

using ParkSystemApp.Models;
public partial class AttrazionePage : ContentPage, IQueryAttributable
{


    public AttrazionePage()
	{
		InitializeComponent();
	}



    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("attrazione", out var obj) && obj is Attrazione attr)
        {
            BindingContext = attr;
        }
    }


}