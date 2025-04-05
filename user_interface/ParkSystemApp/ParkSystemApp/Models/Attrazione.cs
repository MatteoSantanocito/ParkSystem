using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ParkSystemApp.Models
{
    public class Attrazione : INotifyPropertyChanged
    {
        public int ID { get; set; }
        private string nome;
        public string Nome
        {
            get => nome;
            set { nome = value; OnPropertyChanged(); }
        }

        private string descrizione;
        public string Descrizione
        {
            get => descrizione;
            set { descrizione = value; OnPropertyChanged(); }
        }

        private string tipologia;
        public string Tipologia
        {
            get => tipologia;
            set { tipologia = value; OnPropertyChanged(); }
        }

        private string tematica;
        public string Tematica
        {
            get => tematica;
            set { tematica = value; OnPropertyChanged(); }
        }

        private int minimumAge;
        public int MinimumAge
        {
            get => minimumAge;
            set { minimumAge = value; OnPropertyChanged(); }
        }

        private string state;
        public string State
        {
            get => state;
            set { state = value; OnPropertyChanged(); }
        }

        private int hourCapacity;
        public int HourCapacity
        {
            get => hourCapacity;
            set { hourCapacity = value; OnPropertyChanged(); }
        }
        public string ImagePath => $"{Nome}.png".Replace(" ", "_");



        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
