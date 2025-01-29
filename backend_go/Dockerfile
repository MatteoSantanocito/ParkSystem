# Usa un'immagine di base con Go
FROM golang:1.21 AS builder

# Imposta la directory di lavoro
WORKDIR /app

# Copia i file go.mod e go.sum e scarica le dipendenze
COPY go.mod go.sum ./
RUN go mod download

# Copia il codice sorgente
COPY . .

# Compila l'applicazione
RUN go build -o backend-go

# Crea un'immagine più leggera per l'esecuzione
FROM alpine:latest

# Imposta la directory di lavoro
WORKDIR /root/

# Copia l'eseguibile dal builder
COPY --from=builder /app/backend-go .

# Espone la porta (cambia se necessario)
EXPOSE 8080

# Comando di avvio
CMD ["./backend-go"]
