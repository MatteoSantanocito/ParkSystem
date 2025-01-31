package utils

import (
	"math/rand"
	"time"
)

// set di lettere (minuscole e/o maiuscole)
var letters = []rune("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")

func init() {
	// Inizializza il seme per la generazione random
	rand.Seed(time.Now().UnixNano())
}

// randomString genera una stringa di lunghezza n
// con caratteri presi da letters
func randomString(n int) string {
	b := make([]rune, n)
	for i := range b {
		b[i] = letters[rand.Intn(len(letters))]
	}
	return string(b)
}

// GenerateRandomFriendCode genera un codice
// del formato "xxxx-xxxx-xxxx"
func GenerateRandomFriendCode() string {
	part1 := randomString(4)
	part2 := randomString(4)
	part3 := randomString(4)
	return part1 + "-" + part2 + "-" + part3
}
