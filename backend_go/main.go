package main

import (
	"database/sql"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	"backend-go/handlers"
	"backend-go/models"
	"backend-go/routes"

	"gopkg.in/yaml.v2"

	_ "github.com/lib/pq"
)

type Config struct {
	Database struct {
		Host     string `yaml:"host"`
		Port     int    `yaml:"port"`
		User     string `yaml:"user"`
		Password string `yaml:"password"`
		DBName   string `yaml:"dbname"`
		SSLMode  string `yaml:"sslmode"`
	} `yaml:"database"`
	JWT struct {
		Secret string `yaml:"secret"`
	} `yaml:"jwt"`
}

func checkAndSendBookingAlerts(db *sql.DB) {
	// Calcola il tempo attuale e il tempo 10 minuti nel futuro
	now := time.Now()
	tenMinutesLater := now.Add(10 * time.Minute)

	// Query che recupera il nome dell'attrazione e l'ID utente della prenotazione
	query := `
        SELECT a.nome, p.id_utente 
		FROM prenotazioni p
		JOIN attrazioni a ON p.id_attrazione = a.id_attrazione
		WHERE p.orario_previsto BETWEEN $1 AND $2

    `
	rows, err := db.Query(query, now, tenMinutesLater)
	if err != nil {
		log.Printf("Errore durante il controllo delle prenotazioni: %v", err)
		return
	}
	defer rows.Close()

	// Itera sui risultati e invia notifiche mirate per ogni prenotazione imminente
	for rows.Next() {
		var nomeAttrazione string
		var bookingUserID int
		if err := rows.Scan(&nomeAttrazione, &bookingUserID); err != nil {
			log.Printf("Errore durante la lettura delle prenotazioni: %v", err)
			continue
		}

		// Logga i dati estratti dalla riga
		log.Printf("Prenotazione trovata: attrazione = %s, id_utente = %d", nomeAttrazione, bookingUserID)

		notificationMsg := "Mancano 10 minuti per l'attrazione " + nomeAttrazione
		handlers.SendNotificationToUser(notificationMsg, bookingUserID)
	}

	if err := rows.Err(); err != nil {
		log.Printf("Errore durante il recupero delle prenotazioni: %v", err)
	}
}

func scheduleBookingAlerts(db *sql.DB) {
	ticker := time.NewTicker(1 * time.Minute) // Controlla ogni minuto
	go func() {
		for range ticker.C {
			checkAndSendBookingAlerts(db)
		}
	}()
}
func loadConfig(path string) (*Config, error) {
	file, err := os.ReadFile(path)
	if err != nil {
		return nil, fmt.Errorf("errore nel caricamento del file di configurazione: %w", err)
	}
	var config Config
	err = yaml.Unmarshal(file, &config)
	if err != nil {
		return nil, fmt.Errorf("errore nel parsing della configurazione: %w", err)
	}
	return &config, nil
}

// funzione per schedulare la pulizia delle prenotazioni scadute una volta al mese
func scheduleBookingCleanup(db *sql.DB) {
	// Definisce il ticker per ogni 30 giorni (30 * 24 ore)
	ticker := time.NewTicker(30 * 24 * time.Hour)
	go func() {
		for range ticker.C {
			err := models.DeleteExpiredBookings(db)
			if err != nil {
				log.Printf("Errore durante l'eliminazione delle prenotazioni scadute: %v", err)
			} else {
				log.Println("Prenotazioni scadute eliminate con successo")
			}
		}
	}()
}

func main() {
	// Carica la configurazione
	config, err := loadConfig("config/config.yml")
	if err != nil {
		log.Fatalf("Errore nel caricamento della configurazione: %v", err)
	}

	// Crea la stringa di connessione al database
	dsn := fmt.Sprintf("host=%s port=%d user=%s password=%s dbname=%s sslmode=%s",
		config.Database.Host, config.Database.Port, config.Database.User,
		config.Database.Password, config.Database.DBName, config.Database.SSLMode)

	// Connessione al database
	db, err := sql.Open("postgres", dsn)
	if err != nil {
		log.Fatalf("Errore nella connessione al database: %v", err)
	}
	defer db.Close()

	// Verifica la connessione al database
	err = db.Ping()
	if err != nil {
		log.Fatalf("Database non raggiungibile: %v", err)
	}

	// Configura le route
	r := routes.SetupRoutes(db, config.JWT.Secret)

	// Schedula la pulizia delle prenotazioni
	scheduleBookingCleanup(db)

	scheduleBookingAlerts(db)
	// Avvia il server
	log.Println("Server in ascolto su :8080")
	err = http.ListenAndServe(":8080", r)
	if err != nil {
		log.Fatalf("Errore nell'avvio del server: %v", err)
	}

}
