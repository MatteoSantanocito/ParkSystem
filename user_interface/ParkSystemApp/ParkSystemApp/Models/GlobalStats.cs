using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ParkSystemApp.StatsPage;

namespace ParkSystemApp.Models
{
    public class GlobalStats
    {
        [JsonPropertyName("media_prenotazioni_giornaliere_mensile")]
        public double MediaPrenotazioniGiornaliereMensile { get; set; }

        [JsonPropertyName("top_3_attrazioni")]
        public Dictionary<string, double> Top3Attrazioni { get; set; }

        [JsonPropertyName("giorni_aperti_mese_corrente")]
        public int GiorniApertiMeseCorrente { get; set; }

        [JsonPropertyName("distribuzione_oraria_percentuale")]
        public Dictionary<string, double> DistribuzioneOrariaPercentuale { get; set; }

        [JsonPropertyName("giorno_con_piu_prenotazioni")]
        public GiornoPrenotazioni GiornoConPiuPrenotazioni { get; set; }

        [JsonPropertyName("giorno_con_meno_prenotazioni")]
        public GiornoPrenotazioni GiornoConMenoPrenotazioni { get; set; }

        [JsonPropertyName("media_prenotazioni_per_utente")]
        public double MediaPrenotazioniPerUtente { get; set; }

        [JsonPropertyName("tipologia_piu_prenotata")]
        public string TipologiaPiuPrenotata { get; set; }

        [JsonPropertyName("tipologia_meno_prenotata")]
        public string TipologiaMenoPrenotata { get; set; }
    }

    public class GiornoPrenotazioni
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("numero_prenotazioni")]
        public int NumeroPrenotazioni { get; set; }
    }
}
