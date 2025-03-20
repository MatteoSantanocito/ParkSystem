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
	// Ottieni la capacità oraria dall'attrazione
	var maxPerHour int
	capacityQuery := `SELECT capacita_oraria FROM attrazioni WHERE id_attrazione = $1`
	if err := db.QueryRow(capacityQuery, attractionID).Scan(&maxPerHour); err != nil {
		return time.Time{}, err
	}

	const rideDuration = 2 * time.Minute

	// Usa l'orario corrente (se vuoi locale, usa time.Now().Local() o In(loc))
	currentTime := time.Now()

	// Inizia dall'inizio dell'ora corrente
	hourStart := time.Date(
		currentTime.Year(),
		currentTime.Month(),
		currentTime.Day(),
		currentTime.Hour(),
		0,
		0,
		0,
		currentTime.Location(),
	)

	for {
		hourEnd := hourStart.Add(time.Hour)

		// Conta le prenotazioni già presenti nell'intervallo [hourStart, hourEnd)
		var count int
		query := `SELECT COUNT(*) FROM prenotazioni WHERE id_attrazione = $1 AND orario_previsto BETWEEN $2 AND $3`
		if err := db.QueryRow(query, attractionID, hourStart, hourEnd).Scan(&count); err != nil {
			return time.Time{}, err
		}

		// Calcola expectedTime in base al numero di prenotazioni presenti
		expectedTime := hourStart.Add(time.Duration(count) * rideDuration)

		// Se expectedTime è nel passato rispetto a currentTime,
		// calcola il prossimo slot disponibile nell'ora corrente.
		if !expectedTime.After(currentTime) {
			// Calcola quanti slot dovrebbero essere passati per essere dopo currentTime.
			elapsed := currentTime.Sub(hourStart)
			nextSlot := int(elapsed/rideDuration) + 1
			expectedTime = hourStart.Add(time.Duration(nextSlot) * rideDuration)
		}

		// Se c'è ancora capacità in questa fascia oraria e expectedTime è entro l'ora
		if count < maxPerHour && expectedTime.Before(hourEnd) && expectedTime.After(currentTime) {
			return expectedTime, nil
		}

		// Altrimenti, passa all'ora successiva e resetta gli slot passati
		hourStart = hourEnd
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

		localTimeStr := expectedTime.Local().Format(time.RFC3339)

		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(map[string]interface{}{
			"userID":       userID,
			"attractionID": req.AttractionID,
			"expectedTime": localTimeStr,
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
