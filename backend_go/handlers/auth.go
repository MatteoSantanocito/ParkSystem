package handlers

import (
	"database/sql"
	"encoding/json"
	"log"
	"net/http"

	"backend-go/models"
	"backend-go/utils"
)

type Credentials struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

func RegisterHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var creds Credentials
		if err := json.NewDecoder(r.Body).Decode(&creds); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		hashedPassword, err := utils.HashPassword(creds.Password)
		if err != nil {
			http.Error(w, "Errore hashing", http.StatusInternalServerError)
			return
		}

		if err := models.CreateUser(db, creds.Username, hashedPassword); err != nil {
			http.Error(w, "Errore registrazione", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(map[string]string{"message": "Utente registrato"})
	}
}

func LoginHandler(db *sql.DB, jwtSecret string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var creds Credentials
		if err := json.NewDecoder(r.Body).Decode(&creds); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		user, err := models.GetUserByUsernamePassword(db, creds.Username, creds.Password)
		if err != nil || !utils.CheckPasswordHash(creds.Password, user.Password) {
			log.Printf("[DEBUG] Errore in GetUserByUsernamePassword: user=%v, pass=%v, err=%v\n", creds.Username, creds.Password, err)
			http.Error(w, "Credenziali errate", http.StatusUnauthorized)
			return
		}

		token, err := utils.GenerateJWT(user.ID, jwtSecret)
		if err != nil {
			http.Error(w, "Errore generazione token", http.StatusInternalServerError)
			return
		}

		// Risposta in formato JSON con il token
		json.NewEncoder(w).Encode(map[string]string{"token": token})
	}
}
