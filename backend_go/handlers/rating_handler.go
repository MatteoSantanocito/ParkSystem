package handlers

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"log"

	"net/http"
)

// SaveRating salva una nuova valutazione nel database
func SaveRating(db *sql.DB, userId, attractionId, rating int) error {
	query := `
		INSERT INTO recensioni (id_utente, id_attrazione, punteggio)
		VALUES ($1, $2, $3)
	`
	_, err := db.Exec(query, userId, attractionId, rating)
	if err != nil {
		return fmt.Errorf("failed to save rating: %w", err)
	}
	return nil
}

func PostRatingHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Verifica se l'utente è autenticato tramite JWT

		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
			return
		}
		// Decodifica del payload della richiesta
		var ratingData struct {
			AttractionId int `json:"attractionId"`
			Rating       int `json:"rating"`
		}
		if err := json.NewDecoder(r.Body).Decode(&ratingData); err != nil {
			log.Println("Error:", err)
			http.Error(w, "Invalid input", http.StatusBadRequest)
			return
		}

		fmt.Println("Attraction ID:", ratingData.AttractionId)
		fmt.Println("Rating:", ratingData.Rating)

		// Salvataggio della valutazione nel database

		if err := SaveRating(db, userID, ratingData.AttractionId, ratingData.Rating); err != nil {
			http.Error(w, "Internal server error", http.StatusInternalServerError)
			return
		}

		// Risposta di successo
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode("Votazione salvata con successo")
	}
}
