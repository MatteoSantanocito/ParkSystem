package handlers

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"net/http"
	"strconv"
	"time"

	"backend-go/models"
)

type BookingRequest struct {
	AttractionID int `json:"AttractionID"`
}

// Calcola il prossimo orario disponibile per la prenotazione
func CalculateNextAvailableTime(db *sql.DB, attractionID int) (time.Time, error) {
	// Ottieni la capacità oraria dall'attrazione specificata
	var maxPerHour int
	capacityQuery := `SELECT capacita_oraria FROM attrazioni WHERE id_attrazione = $1`
	err := db.QueryRow(capacityQuery, attractionID).Scan(&maxPerHour)
	if err != nil {
		return time.Time{}, err // Ritorna errore se la query fallisce o l'attrazione non esiste
	}

	const rideDuration = 2 * time.Minute

	currentTime := time.Now()
	hourStart := time.Date(currentTime.Year(), currentTime.Month(), currentTime.Day(), currentTime.Hour(), 0, 0, 0, currentTime.Location())

	// Calcola il numero di slot passati dall'inizio dell'ora fino all'orario corrente
	slotsPassed := int(currentTime.Sub(hourStart) / rideDuration)

	for {
		hourEnd := hourStart.Add(time.Hour)
		var count int
		query := `SELECT COUNT(*) FROM prenotazioni WHERE id_attrazione = $1 AND orario_previsto BETWEEN $2 AND $3`
		err := db.QueryRow(query, attractionID, hourStart, hourEnd).Scan(&count)
		if err != nil {
			return time.Time{}, err
		}

		if count < maxPerHour {
			expectedTime := hourStart.Add(time.Duration(count) * rideDuration)
			// Assicurati che l'expectedTime sia anche dopo l'currentTime
			if expectedTime.After(currentTime) && count >= slotsPassed {
				return expectedTime, nil
			}
		}

		// Se non ci sono slot disponibili nell'ora corrente, o tutti gli slot sono nel passato,
		// sposta hourStart all'hourEnd e aggiusta slotsPassed per la nuova ora
		hourStart = hourEnd
		slotsPassed = 0 // Reset dei slot passati poiché ora sei in una nuova ora
	}
}

// CreateBookingHandler gestisce la creazione di una nuova prenotazione
func CreateBookingHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {

		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
			return
		}

		var req BookingRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, "Invalid input", http.StatusBadRequest)
			return
		}

		// Stampiamo l'attractionID e userID in console
		fmt.Println("Attraction ID:", req.AttractionID, "User ID:", userID)

		expectedTime, err := CalculateNextAvailableTime(db, req.AttractionID)
		if err != nil {
			http.Error(w, "Failed to calculate expected time", http.StatusInternalServerError)
			return
		}

		if err := models.CreateBooking(db, userID, req.AttractionID, expectedTime); err != nil {
			http.Error(w, "Failed to create booking", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(map[string]interface{}{
			"userID":       userID,
			"attractionID": req.AttractionID,
			"expectedTime": expectedTime,
		})
	}
}

// GetBookingHandler gestisce la richiesta di dettagli di una prenotazione
func GetBookingHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Assumi che l'ID della prenotazione venga passato come parametro URL
		bookingID, _ := strconv.Atoi(r.URL.Query().Get("id"))

		book, err := models.GetBookingByID(db, bookingID)
		if err != nil {
			http.Error(w, "Booking not found", http.StatusNotFound)
			return
		}

		json.NewEncoder(w).Encode(book)
	}
}

// UpdateBookingHandler gestisce l'aggiornamento dello stato di una prenotazione
func UpdateBookingHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		bookingID, _ := strconv.Atoi(r.URL.Query().Get("id"))
		var status struct {
			NewStatus string `json:"new_status"`
		}
		if err := json.NewDecoder(r.Body).Decode(&status); err != nil {
			http.Error(w, "Invalid request body", http.StatusBadRequest)
			return
		}

		if err := models.UpdateBookingStatus(db, bookingID, status.NewStatus); err != nil {
			http.Error(w, "Failed to update booking", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{"message": "Booking updated successfully"})
	}
}

// DeleteBookingHandler elimina l'ultima prenotazione inserita da un utente.
func DeleteBookingHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Estrae l'ID utente dal context (assicurandosi che sia autenticato)
		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
			return
		}

		// Esegue l'eliminazione dell'ultima prenotazione inserita dall'utente
		// Assumendo che esista una colonna 'data_creazione' per identificare l'ultima prenotazione
		_, err := db.Exec(`
            DELETE FROM prenotazioni
            WHERE id_prenotazione = (
                SELECT id_prenotazione
                FROM prenotazioni
                WHERE id_utente = $1
                ORDER BY data_prenotazione DESC
                LIMIT 1
            )
        `, userID)
		if err != nil {
			http.Error(w, "Errore eliminazione prenotazione", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusOK)
		w.Write([]byte("prenotazione cancellata con successo"))
	}
}
