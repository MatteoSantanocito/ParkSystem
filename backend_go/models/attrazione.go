package models

// Importa il pacchetto time per gestire i tipi TIME di PostgreSQL

// Attrazione rappresenta il modello di dati per una attrazione nel parco.
type Attrazione struct {
	ID           int    `json:"id"`           // corrisponde a id_attrazione
	Nome         string `json:"nome"`         // corrisponde a nome
	Descrizione  string `json:"descrizione"`  // corrisponde a descrizione
	Tipologia    string `json:"tipologia"`    // corrisponde a tipologia
	Tematica     string `json:"tematica"`     // corrisponde a tematica
	MinimumAge   int    `json:"minimumAge"`   // corrisponde a eta_minima
	State        string `json:"state"`        // corrisponde a stato
	HourCapacity int    `json:"hourCapacity"` // corrisponde a capacita_oraria
}
