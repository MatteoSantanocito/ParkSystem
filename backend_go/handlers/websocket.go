package handlers

import (
	"fmt"
	"log"
	"net/http"
	"sync"

	"github.com/gorilla/websocket"
)

var (
	clients   = make(map[*websocket.Conn]string) // Associa ad ogni connessione l'ID utente (come stringa)
	clientsMu sync.Mutex
)

var upgrader = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool {
		return true // Per test in locale, in produzione aggiungi controlli appropriati.
	},
}

// HandleWebSocket gestisce l'upgrade e registra il client.
func HandleWebSocket(w http.ResponseWriter, r *http.Request) {
	// Estrai l'ID utente dal contesto, come fai nelle altre rotte
	userID, ok := r.Context().Value("userID").(int)
	if !ok {
		http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
		return
	}

	// Effettua l'upgrade della connessione a WebSocket
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("Errore nell'upgrade WebSocket: %v", err)
		return
	}

	// Registra il client, associandolo all'ID utente (convertito in stringa, ad esempio)
	clientsMu.Lock()
	clients[conn] = fmt.Sprintf("%d", userID)
	clientsMu.Unlock()

	log.Printf("Nuovo client WebSocket connesso. userID: %d", userID)

	// Loop per mantenere la connessione aperta e leggere eventuali messaggi dal client.
	for {
		_, _, err := conn.ReadMessage()
		if err != nil {
			clientsMu.Lock()
			delete(clients, conn)
			clientsMu.Unlock()
			log.Printf("Connessione WebSocket chiusa per userID %d: %v", userID, err)
			break
		}
	}
}

func SendNotificationToUser(message string, targetUserID int) {
	targetIDStr := fmt.Sprintf("%d", targetUserID)
	clientsMu.Lock()
	defer clientsMu.Unlock()

	for conn, uid := range clients {
		if uid == targetIDStr {
			if err := conn.WriteMessage(websocket.TextMessage, []byte(message)); err != nil {
				log.Printf("Errore durante l'invio della notifica a userID %s: %v", uid, err)
				conn.Close()
				delete(clients, conn)
			}
		}
	}
}

func SendNotificationHandler(w http.ResponseWriter, r *http.Request) {
	userID, ok := r.Context().Value("userID").(int)
	if !ok {
		http.Error(w, "Autenticazione non valida", http.StatusUnauthorized)
		return
	}
	notification := "Notifica di prova dal backend per l'utente " + fmt.Sprintf("%d", userID)
	SendNotificationToUser(notification, userID)
	w.WriteHeader(http.StatusOK)
	w.Write([]byte("Notifica inviata"))
}
