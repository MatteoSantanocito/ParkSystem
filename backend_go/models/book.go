package models

import (
	"database/sql"
	"time"
)

// Book rappresenta una prenotazione nel sistema
type Book struct {
	ID            int       `json:"id"`
	UserID        int       `json:"user_id"`
	AttractionID  int       `json:"attraction_id"`
	BookingDate   time.Time `json:"booking_date"`
	ExpectedTime  time.Time `json:"expected_time"`
	BookingStatus string    `json:"booking_status"`
}

// CreateBooking inserisce una nuova prenotazione nel database
func CreateBooking(db *sql.DB, userID, attractionID int, expectedTime time.Time) error {
	query := `
        INSERT INTO prenotazioni (
            id_utente,
            id_attrazione,
            orario_previsto
        ) VALUES ($1, $2, $3)
    `
	_, err := db.Exec(query, userID, attractionID, expectedTime)
	return err
}

// GetBookingByID recupera una prenotazione tramite il suo ID
func GetBookingByID(db *sql.DB, id int) (*Book, error) {
	var book Book
	query := `
        SELECT id_prenotazione, id_utente, id_attrazione, data_prenotazione, orario_previsto, stato_prenotazione
        FROM prenotazioni
        WHERE id_prenotazione = $1
    `
	err := db.QueryRow(query, id).Scan(&book.ID, &book.UserID, &book.AttractionID, &book.BookingDate, &book.ExpectedTime, &book.BookingStatus)
	if err != nil {
		return nil, err
	}
	return &book, nil
}

// UpdateBookingStatus aggiorna lo stato di una prenotazione
func UpdateBookingStatus(db *sql.DB, bookingID int, newStatus string) error {
	query := `
        UPDATE prenotazioni
        SET stato_prenotazione = $1
        WHERE id_prenotazione = $2
    `
	_, err := db.Exec(query, newStatus, bookingID)
	return err
}

// DeleteBooking elimina una prenotazione dal database
func DeleteBooking(db *sql.DB, bookingID int) error {
	query := `DELETE FROM prenotazioni WHERE id_prenotazione = $1`
	_, err := db.Exec(query, bookingID)
	return err
}

// DeleteExpiredBookings elimina le prenotazioni scadute dal database
func DeleteExpiredBookings(db *sql.DB) error {
	// Imposta il tempo limite per considerare una prenotazione scaduta a 1 minuto fa
	cutoffTime := time.Now().Add(-1 * time.Minute)
	query := `DELETE FROM prenotazioni WHERE orario_previsto < $1`
	_, err := db.Exec(query, cutoffTime)
	if err != nil {
		return err
	}
	return nil
}
