package handlers

import (
	"encoding/json"
	"net/http"
	"os"
	"strconv"
)

// Struct per rappresentare ogni riga nel JSON
type DailyStat struct {
	IDAttrazione                           int     `json:"id_attrazione"`
	Nome                                   string  `json:"nome"`
	PercentualeRiempimentoOraria           float64 `json:"percentuale_riempimento_oraria"`
	PercentualeMediaRiempimentoGiornaliera float64 `json:"percentuale_media_riempimento_giornaliera"`
}

// Statistica globale letta da file JSON
func GetGlobalStatsHandler(w http.ResponseWriter, r *http.Request) {
	data, err := os.ReadFile("/data/monthly_global_stats.json")
	if err != nil {
		http.Error(w, "Errore lettura stats", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.Write(data)
}

func GetDailyStatsByAttrazioneID(w http.ResponseWriter, r *http.Request) {
	// Prendi parametro ?id=
	idStr := r.URL.Query().Get("id")
	if idStr == "" {
		http.Error(w, "Parametro 'id' mancante", http.StatusBadRequest)
		return
	}

	id, err := strconv.Atoi(idStr)
	if err != nil {
		http.Error(w, "Parametro 'id' non valido", http.StatusBadRequest)
		return
	}

	// Legge il JSON dal file
	data, err := os.ReadFile("/data/daily_single_stats.json")
	if err != nil {
		http.Error(w, "Errore lettura file statistiche", http.StatusInternalServerError)
		return
	}

	var stats []DailyStat
	if err := json.Unmarshal(data, &stats); err != nil {
		http.Error(w, "Errore parsing JSON", http.StatusInternalServerError)
		return
	}

	// Cerca la statistica per l'id richiesto
	for _, stat := range stats {
		if stat.IDAttrazione == id {
			w.Header().Set("Content-Type", "application/json")
			json.NewEncoder(w).Encode(stat)
			return
		}
	}

	http.Error(w, "Attrazione non trovata", http.StatusNotFound)
}
