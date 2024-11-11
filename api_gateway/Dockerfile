# Utilizziamo l'immagine di Go per compilare ed eseguire l'applicazione
FROM golang:1.16-alpine

# Impostiamo la directory di lavoro
WORKDIR /app

# Copiamo il file go.mod e go.sum e installiamo le dipendenze
COPY go.mod go.sum ./
RUN go mod download

# Copiamo il resto del codice
COPY . .

# Compiliamo l'applicazione
RUN go build -o api_gateway

# Espone la porta 8080
EXPOSE 8080

# Comando per avviare l'applicazione
CMD ["./api_gateway"]