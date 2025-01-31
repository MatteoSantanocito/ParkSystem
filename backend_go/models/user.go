package models

import (
	"database/sql"
)

type User struct {
	ID                int
	Nome              string
	Cognome           string
	Email             string
	Telefono          string
	PasswordHash      string
	TipoUtente        string
	CodiceAmico       string
	TipoAvventura     string
	DataRegistrazione string
}

// Creazione utente
func CreateUser(db *sql.DB, nome, cognome, email, telefono, passwordHash, tipoUtente, codiceAmico, tipoAvventura string) error {
	query := `
        INSERT INTO utenti (
            nome,
            cognome,
            email,
            telefono,
            password_hash,
            tipo_utente,
            codice_amico,
            tipo_avventura
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
    `
	_, err := db.Exec(query, nome, cognome, email, telefono, passwordHash, tipoUtente, codiceAmico, tipoAvventura)
	return err
}

// Recupero utente per email
func GetUserByEmail(db *sql.DB, email string) (*User, error) {
	var user User
	query := `
        SELECT
            id_utente,
            nome,
            cognome,
            email,
            telefono,
            password_hash,
            tipo_utente,
            codice_amico,
            tipo_avventura,
            data_registrazione
        FROM utenti
        WHERE email = $1
    `
	err := db.QueryRow(query, email).Scan(
		&user.ID,
		&user.Nome,
		&user.Cognome,
		&user.Email,
		&user.Telefono,
		&user.PasswordHash,
		&user.TipoUtente,
		&user.CodiceAmico,
		&user.TipoAvventura,
		&user.DataRegistrazione,
	)
	if err != nil {
		return nil, err
	}
	return &user, nil
}
