package handlers

import (
	"database/sql"
	"encoding/json"
	"net/http"

	"backend-go/utils"
)

// ChangeEmailRequest rappresenta il payload per il cambio email.
type ChangeEmailRequest struct {
	NuovaEmail string `json:"NuovaEmail"`
}

// ChangePasswordRequest rappresenta il payload per il cambio password.
type ChangePasswordRequest struct {
	VecchiaPassword string `json:"VecchiaPassword"`
	NuovaPassword   string `json:"NuovaPassword"`
}

// UpdateEmailHandler gestisce la modifica dell'email.
// Endpoint: PUT /account/email
func UpdateEmailHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Estrae l'ID utente dal context (impostato dal middleware JWT)
		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
			return
		}

		var req ChangeEmailRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		// Esegue l'update dell'email
		_, err := db.Exec(`UPDATE utenti SET email = $1 WHERE id_utente = $2`, req.NuovaEmail, userID)
		if err != nil {
			http.Error(w, "Errore aggiornamento email", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusOK)
	}
}

// UpdatePasswordHandler gestisce il cambio della password.
// Endpoint: PUT /account/password
func UpdatePasswordHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Estrae l'ID utente dal context
		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
			return
		}

		var req ChangePasswordRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		// Recupera l'attuale hash della password dal DB
		var currentHash string
		err := db.QueryRow(`SELECT password_hash FROM utenti WHERE id_utente = $1`, userID).Scan(&currentHash)
		if err != nil {
			http.Error(w, "Utente inesistente", http.StatusNotFound)
			return
		}

		// Verifica che la vecchia password sia corretta
		if !utils.CheckPasswordHash(req.VecchiaPassword, currentHash) {
			http.Error(w, "Vecchia password errata", http.StatusUnauthorized)
			return
		}

		// Hash della nuova password
		newHash, err := utils.HashPassword(req.NuovaPassword)
		if err != nil {
			http.Error(w, "Errore nell'hashing della nuova password", http.StatusInternalServerError)
			return
		}

		// Aggiorna la password nel DB
		_, err = db.Exec(`UPDATE utenti SET password_hash = $1 WHERE id_utente = $2`, newHash, userID)
		if err != nil {
			http.Error(w, "Errore aggiornamento password", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusOK)
	}
}

// DeleteAccountHandler elimina definitivamente l'account dell'utente loggato.
// Endpoint: DELETE /account
func DeleteAccountHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Estrae l'ID utente dal context
		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
			return
		}

		_, err := db.Exec(`DELETE FROM utenti WHERE id_utente = $1`, userID)
		if err != nil {
			http.Error(w, "Errore eliminazione utente", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusOK)
	}
}
