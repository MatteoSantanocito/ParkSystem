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

func InsertAttrazioneHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var attr models.Attrazione
		if err := json.NewDecoder(r.Body).Decode(&attr); err != nil {
			http.Error(w, "Errore nella lettura dell'attrazione: "+err.Error(), http.StatusBadRequest)
			return
		}

		// Insert into the database
		query := `INSERT INTO attrazioni (nome, descrizione, tipologia, tematica, eta_minima, stato, capacita_oraria) VALUES ($1, $2, $3, $4, $5, $6, $7)`
		_, err := db.Exec(query, attr.Nome, attr.Descrizione, attr.Tipologia, attr.Tematica, attr.MinimumAge, attr.State, attr.HourCapacity)
		if err != nil {
			log.Printf("Errore durante l'inserimento dell'attrazione: %v", err)
			http.Error(w, "Errore durante l'inserimento nel database", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusCreated)
		w.Write([]byte("Attrazione inserita con successo"))
	}
}

func UpdateAttrazioneHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var attrazione models.Attrazione

		// Decodifica del body
		if err := json.NewDecoder(r.Body).Decode(&attrazione); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		// Query per aggiornare l’attrazione
		_, err := db.Exec(`
		UPDATE attrazioni 
		SET 
			nome = $1, 
			descrizione = $2, 
			tipologia = $3, 
			tematica = $4, 
			eta_minima = $5, 
			stato = $6, 
			capacita_oraria = $7
		WHERE id_attrazione = $8`, attrazione.Nome, attrazione.Descrizione, attrazione.Tipologia,
			attrazione.Tematica, attrazione.MinimumAge, attrazione.State,
			attrazione.HourCapacity, attrazione.ID)

		if err != nil {
			http.Error(w, "Errore aggiornamento attrazione", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusOK)
		w.Write([]byte(`{"message": "Attrazione aggiornata con successo"}`))
	}
}

func DeleteAttrazioneHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var req models.DeleteAttrazioneRequest

		// Decodifica del body JSON
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		// Esegue la cancellazione
		_, err := db.Exec(`DELETE FROM attrazioni WHERE id_attrazione = $1`, req.ID)
		if err != nil {
			http.Error(w, "Errore durante l'eliminazione", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusOK)
		w.Write([]byte(`{"message": "Attrazione eliminata con successo"}`))
	}
}
