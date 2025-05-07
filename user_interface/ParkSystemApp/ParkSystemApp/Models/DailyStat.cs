using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ParkSystemApp.Models
{
    public class DailyStat
    {
        [JsonPropertyName("id_attrazione")]
        public int IdAttrazione { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("percentuale_riempimento_oraria")]
        public double PercentualeRiempimentoOraria { get; set; }

        [JsonPropertyName("percentuale_media_riempimento_giornaliera")]
        public double PercentualeMediaRiempimentoGiornaliera { get; set; }
    }
}
