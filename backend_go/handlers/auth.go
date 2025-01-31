package handlers

import (
	"database/sql"
	"encoding/json"
	"log"
	"net/http"

	"backend-go/models"
	"backend-go/utils"
)

// Credentials rappresenta i campi in arrivo dal JSON di registrazione/login.
// (Aggiungiamo anche Telefono, se vogliamo consentire l'input)
type Credentials struct {
	Email         string `json:"email"`
	Password      string `json:"password"`
	Nome          string `json:"nome"`
	Cognome       string `json:"cognome"`
	Telefono      string `json:"telefono"`       // opzionale in input
	TipoAvventura string `json:"tipo_avventura"` // corrisponde a colonna "tipo_avventura"
	// TipoUtente omesso qui se lo vuoi forzare a "visitatore"
}

// RegisterHandler gestisce la registrazione utente
func RegisterHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var creds Credentials
		if err := json.NewDecoder(r.Body).Decode(&creds); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		// Hash della password
		hashedPassword, err := utils.HashPassword(creds.Password)
		if err != nil {
			http.Error(w, "Errore hashing password", http.StatusInternalServerError)
			return
		}

		// Generiamo il codice amico (formato "xxxx-xxxx-xxxx")
		codiceAmico := utils.GenerateRandomFriendCode()

		// Forziamo "visitatore" come tipo_utente di default (o puoi farlo in DB)
		tipoUtente := "visitatore"

		// Chiamiamo CreateUser dal modello
		err = models.CreateUser(
			db,
			creds.Nome,    // nome
			creds.Cognome, // cognome
			creds.Email,
			creds.Telefono,
			hashedPassword, // passwordHash
			tipoUtente,
			codiceAmico,
			creds.TipoAvventura, // tipo_avventura
		)
		if err != nil {
			log.Printf("Errore CreateUser: %v\n", err)
			http.Error(w, "Errore registrazione", http.StatusInternalServerError)
			return
		}

		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(map[string]string{"message": "Utente registrato correttamente"})
	}
}

// LoginHandler gestisce il login
func LoginHandler(db *sql.DB, jwtSecret string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var creds Credentials
		if err := json.NewDecoder(r.Body).Decode(&creds); err != nil {
			http.Error(w, "Dati non validi", http.StatusBadRequest)
			return
		}

		// Recuperiamo utente dal DB
		user, err := models.GetUserByEmail(db, creds.Email)
		if err != nil {
			log.Printf("[DEBUG] user=%v, pass=%v, err=%v\n", creds.Email, creds.Password, err)
			http.Error(w, "Credenziali errate", http.StatusUnauthorized)
			return
		}

		// Controlliamo la password
		if !utils.CheckPasswordHash(creds.Password, user.PasswordHash) {
			log.Printf("[DEBUG] user=%v, pass=%v => password hash mismatch\n", creds.Email, creds.Password)
			http.Error(w, "Credenziali errate", http.StatusUnauthorized)
			return
		}

		// Generiamo token JWT
		token, err := utils.GenerateJWT(user.ID, jwtSecret)
		if err != nil {
			http.Error(w, "Errore generazione token", http.StatusInternalServerError)
			return
		}

		// Restituiamo token e user (in JSON)
		json.NewEncoder(w).Encode(map[string]interface{}{
			"token": token,
			"user": map[string]interface{}{
				"id":                 user.ID,
				"nome":               user.Nome,
				"cognome":            user.Cognome,
				"email":              user.Email,
				"telefono":           user.Telefono,
				"tipo_utente":        user.TipoUtente,
				"codice_amico":       user.CodiceAmico,
				"tipo_avventura":     user.TipoAvventura,
				"data_registrazione": user.DataRegistrazione,
			},
		})
	}
}
