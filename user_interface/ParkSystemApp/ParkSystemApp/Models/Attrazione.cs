using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSystemApp.Models
{
    public class Attrazione
    {
        public required string Nome { get; set; }
        public required string Descrizione { get; set; }

        public required string Tipologia { get; set; }

        public required string Tematica { get; set; }

        public required  int MinimumAge { get; set; }

        public required string State { get; set; }

        public required int HourCapacity { get; set; }
        public string ImagePath => $"{Nome}.png".Replace(" ", "_");
    }
}
