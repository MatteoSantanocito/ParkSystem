package models

import (
	"database/sql"
	"time"
)

type User struct {
	ID        int       `json:"id"`
	Email     string    `json:"email"`
	Password  string    `json:"password"`
	CreatedAt time.Time `json:"created_at"`
}

func CreateUser(db *sql.DB, Email, hashedPassword string) error {
	_, err := db.Exec("INSERT INTO utenti (email, password) VALUES ($1, $2)", Email, hashedPassword)
	return err
}

// DA RISOLVERE
func GetUserByUsernamePassword(db *sql.DB, Email string, Password string) (*User, error) {
	var user User
	err := db.QueryRow("SELECT email, password_hash FROM utenti WHERE email = $1 AND password_hash=$2 ", Email, Password).
		Scan(&user.Email, &user.Password)
	if err != nil {
		return nil, err
	}
	return &user, nil
}
