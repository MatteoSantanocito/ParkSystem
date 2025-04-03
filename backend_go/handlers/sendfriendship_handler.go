package handlers

import (
	"database/sql"
	"encoding/json"
	"log"
	"net/http"
)

type FriendRequest struct {
	CodiceAmico string `json:"codice_amico"` // Ora accetta stringhe alfanumeriche
}

func SendFriendRequest(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// 1. Autenticazione
		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, `{"error": "Utente non autenticato"}`, http.StatusUnauthorized)
			return
		}

		// 2. Parsing della richiesta
		var req FriendRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, `{"error": "Richiesta malformata"}`, http.StatusBadRequest)
			return
		}

		// 3. Trova l'ID destinatario dal codice amico
		var destID int
		err := db.QueryRow(`
			SELECT id_utente 
			FROM utenti 
			WHERE codice_amico = $1`, req.CodiceAmico).Scan(&destID)

		if err != nil {
			if err == sql.ErrNoRows {
				http.Error(w, `{"error": "Codice amico non trovato"}`, http.StatusNotFound)
			} else {
				log.Printf("Database error: %v", err)
				http.Error(w, `{"error": "Errore del server"}`, http.StatusInternalServerError)
			}
			return
		}

		// 4. Verifica che non stia aggiungendo se stesso
		if userID == destID {
			http.Error(w, `{"error": "Non puoi aggiungere te stesso come amico"}`, http.StatusBadRequest)
			return
		}

		// 5. Inserisci la richiesta di amicizia
		_, err = db.Exec(`
			INSERT INTO amicizie 
			(id_utente_mittente, id_utente_destinatario, stato_richiesta, data_richiesta)
			VALUES ($1, $2, 'in_attesa', NOW())`,
			userID, destID)

		if err != nil {
			log.Printf("Insert friendship error: %v", err)
			http.Error(w, `{"error": "Errore nell'invio della richiesta"}`, http.StatusInternalServerError)
			return
		}

		// 6. Risposta di successo
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(map[string]string{
			"message": "Richiesta inviata con successo",
		})
	}
}
