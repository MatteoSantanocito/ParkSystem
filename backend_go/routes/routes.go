package routes

import (
	"database/sql"
	"net/http"

	"backend-go/handlers"

	"github.com/gorilla/mux"
)

func SetupRoutes(db *sql.DB, jwtSecret string) *mux.Router {
	r := mux.NewRouter()

	r.HandleFunc("/register", handlers.RegisterHandler(db)).Methods("POST")
	r.HandleFunc("/login", handlers.LoginHandler(db, jwtSecret)).Methods("POST")
	r.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		w.Write([]byte("Benvenuto nel backend Go su / !"))
	}).Methods("GET")

	return r
}
