package routes

import (
	"database/sql"

	"backend-go/handlers"

	"github.com/gorilla/mux"
)

func SetupRoutes(db *sql.DB, jwtSecret string) *mux.Router {
	r := mux.NewRouter()

	r.HandleFunc("/register", handlers.RegisterHandler(db)).Methods("POST")
	r.HandleFunc("/login", handlers.LoginHandler(db, jwtSecret)).Methods("POST")

	return r
}
