package main

import (
	"database/sql"
	"fmt"
	"log"
	"net/http"
	"os"

	"backend-go/routes"

	"gopkg.in/yaml.v2"
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
		return nil, err
	}
	var config Config
	err = yaml.Unmarshal(file, &config)
	return &config, err
}

func main() {
	config, _ := loadConfig("config/config.yml")
	db, _ := sql.Open("postgres", fmt.Sprintf("host=%s port=%d user=%s password=%s dbname=%s sslmode=%s",
		config.Database.Host, config.Database.Port, config.Database.User,
		config.Database.Password, config.Database.DBName, config.Database.SSLMode))
	defer db.Close()

	r := routes.SetupRoutes(db, config.JWT.Secret)
	log.Println("Server in ascolto su :8080")
	http.ListenAndServe(":8080", r)
}
