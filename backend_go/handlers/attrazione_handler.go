// File: handlers/attrazione_handler.go
package handlers

import (
	"database/sql"
	"encoding/json"
	"log"
	"net/http"

	"backend-go/models"
)

// GetAttrazioniHandler ritorna un handler per le richieste delle attrazioni.
func GetAttrazioniHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		attrazioni, err := fetchAttrazioni(db)
		if err != nil {
			http.Error(w, "Failed to fetch attractions", http.StatusInternalServerError)
			log.Println("Error fetching attractions:", err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		if err := json.NewEncoder(w).Encode(attrazioni); err != nil {
			http.Error(w, "Error encoding response", http.StatusInternalServerError)
			log.Println("Error encoding attractions:", err)
		}
	}
}

// fetchAttrazioni esegue la query al database e restituisce una lista di attrazioni.
func fetchAttrazioni(db *sql.DB) ([]models.Attrazione, error) {
	// Utilizzo di SELECT * per semplificare la query
	query := `SELECT id_attrazione, nome, descrizione, tipologia, tematica, eta_minima, stato, capacita_oraria FROM attrazioni`
	rows, err := db.Query(query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var attrazioni []models.Attrazione
	for rows.Next() {
		var attr models.Attrazione
		// Assicurati che l'ordine e il numero dei campi nel metodo Scan corrispondano alla struttura Attrazione
		if err := rows.Scan(&attr.ID, &attr.Nome, &attr.Descrizione, &attr.Tipologia, &attr.Tematica, &attr.MinimumAge, &attr.State, &attr.HourCapacity); err != nil {
			return nil, err
		}
		attrazioni = append(attrazioni, attr)
	}
	return attrazioni, nil
}
