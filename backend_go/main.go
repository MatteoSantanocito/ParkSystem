package main

import (
	"database/sql"
	"fmt"
	"log"
	"net/http"
	"os"

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

	// Avvia il server
	log.Println("Server in ascolto su :8080")
	err = http.ListenAndServe(":8080", r)
	if err != nil {
		log.Fatalf("Errore nell'avvio del server: %v", err)
	}
}
