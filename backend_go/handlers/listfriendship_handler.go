package handlers

import (
	"database/sql"
	"encoding/json"
	"log"
	"net/http"
	"time"
)

// Friendship rappresenta la struttura della relazione in amicizie.
type Friendship struct {
	ID             int       `json:"id_richiesta"`
	MittenteID     int       `json:"id_utente_mittente"`
	DestinatarioID int       `json:"id_utente_destinatario"`
	Stato          string    `json:"stato_richiesta"`
	Data           time.Time `json:"data_richiesta"`
}

type PendingRequest struct {
	IDRichiesta     int       `json:"id_richiesta"`
	Data            time.Time `json:"data_richiesta"`
	MittenteNome    string    `json:"mittente_nome"`
	MittenteCognome string    `json:"mittente_cognome"`
}

type AcceptedFriend struct {
	ID               int       `json:"id_richiesta"`
	FullName         string    `json:"full_name"`
	TipoAvventura    string    `json:"tipo_avventura"`
	DataAccettazione time.Time `json:"data_accettazione"`
}

// ListFriendshipsResponse raggruppa le amicizie accettate e quelle in attesa.
type ListFriendshipsResponse struct {
	Accepted []Friendship     `json:"accepted"`
	Pending  []PendingRequest `json:"pending"`
}

// UserInfo rappresenta i dati pubblici di un utente
type UserInfo struct {
	ID            int    `json:"id_utente"`
	Nome          string `json:"nome"`
	Cognome       string `json:"cognome"`
	TipoAvventura string `json:"tipo_avventura"`
	CodiceAmico   string `json:"codice_amico"`
}

func SearchUserHandler(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Log della richiesta
		log.Println("Richiesta ricerca utente ricevuta")

		// Recupera il codice amico dalla query string
		userCode := r.URL.Query().Get("code")
		if userCode == "" {
			log.Println("Codice amico mancante nella richiesta")
			http.Error(w, `{"error": "Codice amico mancante"}`, http.StatusBadRequest)
			return
		}

		log.Printf("Ricerca utente con codice: %s\n", userCode)

		// Query modificata per cercare per codice_amico
		var user UserInfo
		query := `
			SELECT id_utente, nome, cognome, tipo_avventura, codice_amico 
			FROM utenti 
			WHERE codice_amico = $1
		`
		err := db.QueryRow(query, userCode).Scan(
			&user.ID,
			&user.Nome,
			&user.Cognome,
			&user.TipoAvventura,
			&user.CodiceAmico,
		)

		if err != nil {
			if err == sql.ErrNoRows {
				log.Printf("Nessun utente trovato con codice: %s\n", userCode)
				http.Error(w, `{"error": "Utente non trovato"}`, http.StatusNotFound)
			} else {
				log.Printf("Errore database: %v\n", err)
				http.Error(w, `{"error": "Errore del server"}`, http.StatusInternalServerError)
			}
			return
		}

		// Verifica che l'utente non stia cercando se stesso
		currentUserID := r.Context().Value("userID").(int)
		if user.ID == currentUserID {
			log.Println("Un utente ha provato a cercare se stesso")
			http.Error(w, `{"error": "Non puoi aggiungere te stesso come amico"}`, http.StatusBadRequest)
			return
		}

		// Log della risposta
		log.Printf("Utente trovato: %+v\n", user)

		w.Header().Set("Content-Type", "application/json")
		if err := json.NewEncoder(w).Encode(user); err != nil {
			log.Printf("Errore serializzazione JSON: %v\n", err)
			http.Error(w, `{"error": "Errore nella risposta"}`, http.StatusInternalServerError)
		}
	}
}

func AcceptFriendRequest(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Recupera l'ID utente dal context (per controlli di sicurezza, se necessario)
		_ = r.Context().Value("userID").(int)

		type RequestPayload struct {
			IDRichiesta int `json:"id_richiesta"`
		}
		var payload RequestPayload
		if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
			http.Error(w, "Bad request", http.StatusBadRequest)
			return
		}

		query := `
			UPDATE amicizie
			SET stato_richiesta = 'accettata', data_richiesta = NOW()
			WHERE id_richiesta = $1 AND stato_richiesta = 'in_attesa'
		`
		res, err := db.Exec(query, payload.IDRichiesta)
		if err != nil {
			http.Error(w, "Errore nell'aggiornamento della richiesta", http.StatusInternalServerError)
			return
		}
		rowsAffected, err := res.RowsAffected()
		if err != nil || rowsAffected == 0 {
			http.Error(w, "Nessuna richiesta trovata o già processata", http.StatusBadRequest)
			return
		}
		w.Write([]byte("Richiesta accettata con successo"))
	}
}

func RejectFriendRequest(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Recupera l'ID utente dal context (per controlli di sicurezza, se necessario)
		_ = r.Context().Value("userID").(int)

		type RequestPayload struct {
			IDRichiesta int `json:"id_richiesta"`
		}
		var payload RequestPayload
		if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
			http.Error(w, "Bad request", http.StatusBadRequest)
			return
		}

		query := `
			UPDATE amicizie
			SET stato_richiesta = 'rifiutata'
			WHERE id_richiesta = $1 AND stato_richiesta = 'in_attesa'
		`
		res, err := db.Exec(query, payload.IDRichiesta)
		if err != nil {
			http.Error(w, "Errore nell'aggiornamento della richiesta", http.StatusInternalServerError)
			return
		}
		rowsAffected, err := res.RowsAffected()
		if err != nil || rowsAffected == 0 {
			http.Error(w, "Nessuna richiesta trovata o già processata", http.StatusBadRequest)
			return
		}
		w.Write([]byte("Richiesta rifiutata con successo"))
	}
}

func ListFriendships(db *sql.DB) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Recupera l'ID utente dal context
		userID, ok := r.Context().Value("userID").(int)
		if !ok {
			http.Error(w, "Utente non autenticato", http.StatusUnauthorized)
			return
		}

		// Amicizie accettate: l'utente è mittente o destinatario
		queryAccepted := `
            SELECT 
              a.id_richiesta,
              CASE 
                WHEN a.id_utente_mittente = $1 THEN u2.nome || ' ' || u2.cognome
                ELSE u1.nome || ' ' || u1.cognome
              END AS full_name,
              CASE 
                WHEN a.id_utente_mittente = $1 THEN u2.tipo_avventura
                ELSE u1.tipo_avventura
              END AS tipo_avventura,
              a.data_richiesta
            FROM amicizie a
            JOIN utenti u1 ON a.id_utente_mittente = u1.id_utente
            JOIN utenti u2 ON a.id_utente_destinatario = u2.id_utente
            WHERE (a.id_utente_mittente = $1 OR a.id_utente_destinatario = $1)
              AND a.stato_richiesta = 'accettata'
        `
		rowsAccepted, err := db.Query(queryAccepted, userID)
		if err != nil {
			http.Error(w, "Errore nel recupero degli amici", http.StatusInternalServerError)
			return
		}
		defer rowsAccepted.Close()

		accepted := []AcceptedFriend{}
		for rowsAccepted.Next() {
			var af AcceptedFriend
			if err := rowsAccepted.Scan(&af.ID, &af.FullName, &af.TipoAvventura, &af.DataAccettazione); err != nil {
				http.Error(w, "Errore nella scansione dei dati", http.StatusInternalServerError)
				return
			}
			accepted = append(accepted, af)
		}

		// Richieste pendenti: l'utente è destinatario
		queryPending := `
					SELECT 
						a.id_richiesta, 
						u.nome AS mittente_nome, 
						u.cognome AS mittente_cognome, 
						a.data_richiesta
					FROM amicizie a
					JOIN utenti u ON a.id_utente_mittente = u.id_utente
					WHERE a.id_utente_destinatario = $1
					AND a.stato_richiesta = 'in_attesa'
				
							
		`
		rowsPending, err := db.Query(queryPending, userID)
		if err != nil {
			http.Error(w, "Errore nel recupero delle richieste pendenti", http.StatusInternalServerError)
			return
		}
		defer rowsPending.Close()

		pending := []PendingRequest{}
		for rowsPending.Next() {
			var pr PendingRequest
			if err := rowsPending.Scan(&pr.IDRichiesta, &pr.MittenteNome, &pr.MittenteCognome, &pr.Data); err != nil {
				log.Printf("Errore nella scansione della richiesta pendente: %v", err)
				continue
			}
			pending = append(pending, pr)
		}

		response := struct {
			Accepted []AcceptedFriend `json:"accepted"`
			Pending  []PendingRequest `json:"pending"`
		}{
			Accepted: accepted,
			Pending:  pending,
		}
		log.Printf("Risposta completa: %+v", response)
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(response)
	}
}
