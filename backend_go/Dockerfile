# Usa un'immagine di base con Go
FROM golang:1.20-alpine

# Imposta la directory di lavoro
WORKDIR /app

# Copia i file go.mod e go.sum (dipendenze)
COPY go.mod go.sum ./

# Scarica le dipendenze
RUN go mod download

# Copia tutto il resto del codice sorgente
COPY . .

# Compila il tuo programma
RUN go build -o backend_go

# Espone la porta 8080 (quella su cui gira la tua app Go)
EXPOSE 8080

# Comando di avvio del container
CMD ["./backend_go"]
