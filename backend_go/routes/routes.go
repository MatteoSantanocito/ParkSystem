package routes

import (
	"database/sql"
	"net/http"

	"backend-go/handlers"
	"backend-go/middleware"

	"github.com/gorilla/mux"
)

func SetupRoutes(db *sql.DB, jwtSecret string) *mux.Router {
	r := mux.NewRouter()

	r.HandleFunc("/register", handlers.RegisterHandler(db)).Methods("POST")
	r.HandleFunc("/login", handlers.LoginHandler(db, jwtSecret)).Methods("POST")
	r.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		w.Write([]byte("Benvenuto nel backend Go su / !"))
	}).Methods("GET")

	// Route per le prenotazioni con applicazione del middleware JWT
	r.Handle("/book/create", middleware.JWTMiddleware(handlers.CreateBookingHandler(db))).Methods("POST")
	r.Handle("/book/get", middleware.JWTMiddleware(handlers.GetBookingHandler(db))).Methods("GET")
	r.Handle("/book/update", middleware.JWTMiddleware(handlers.UpdateBookingHandler(db))).Methods("PUT")
	r.Handle("/book/delete", middleware.JWTMiddleware(handlers.DeleteBookingHandler(db))).Methods("DELETE")
	// Endpoint protetti: applica il middleware JWT per garantire che solo utenti autenticati possano accedervi
	r.Handle("/account/email", middleware.JWTMiddleware(handlers.UpdateEmailHandler(db))).Methods("PUT")
	r.Handle("/account/password", middleware.JWTMiddleware(handlers.UpdatePasswordHandler(db))).Methods("PUT")
	r.Handle("/account", middleware.JWTMiddleware(handlers.DeleteAccountHandler(db))).Methods("DELETE")

	// Endpoint pubblici (non protetti) per ottenere informazioni sulle attrazioni
	r.HandleFunc("/attrazioni", handlers.GetAttrazioniHandler(db)).Methods("GET")

	r.Handle("/ratings", middleware.JWTMiddleware(handlers.PostRatingHandler(db))).Methods("POST")

	// route WebSocket
	r.Handle("/ws", middleware.JWTMiddleware(http.HandlerFunc(handlers.HandleWebSocket)))

	r.Handle("/send-notification", middleware.JWTMiddleware(http.HandlerFunc(handlers.SendNotificationHandler))).Methods("POST")

	// endpoint per le richieste di amicizia protetti da JWT
	r.Handle("/friendship/request", middleware.JWTMiddleware(http.HandlerFunc(handlers.SendFriendRequest(db)))).Methods("POST")
	r.Handle("/friendship/accept", middleware.JWTMiddleware(http.HandlerFunc(handlers.AcceptFriendRequest(db)))).Methods("PUT")
	r.Handle("/friendship/reject", middleware.JWTMiddleware(http.HandlerFunc(handlers.RejectFriendRequest(db)))).Methods("PUT")
	r.Handle("/friendship/list", middleware.JWTMiddleware(http.HandlerFunc(handlers.ListFriendships(db)))).Methods("GET")
	r.Handle("/friendship/search", middleware.JWTMiddleware(http.HandlerFunc(handlers.SearchUserHandler(db)))).Methods("GET")

	///gli endpoint per registrazione e login sono pubblici: non richiedono che
	///l'utente sia già autenticato, perché sono il punto di ingresso per ottenere
	///il token. Invece, le operazioni che modificano dati sensibili (cambio email,
	///cambio password, eliminazione account) devono essere protette per assicurarsi
	///che solo l'utente autenticato possa eseguirle. Il middleware JWT serve proprio a questo scopo:
	/// - Verifica la presenza e la validità del token JWT nell'header della richiesta.
	/// - Estrae l'ID utente dal token e lo inserisce nel contesto della richiesta.
	/// - Garantisce che, se il token non è valido o assente, la richiesta venga
	/// rifiutata con un errore di autorizzazione. Quindi, mentre per registrazione e login
	/// non serve autenticazione, per gli endpoint di account è fondamentale assicurarsi che
	///l'utente che effettua la richiesta sia effettivamente chi dice di essere.

	return r
}
