# Utilizziamo l'immagine di Python slim per un'immagine più leggera
FROM python:3.9-slim

# Impostiamo la directory di lavoro
WORKDIR /app

# Copiamo il file requirements.txt e installiamo le dipendenze
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copiamo il resto del codice nell'immagine
COPY . .

# Espone la porta 8000
EXPOSE 8000

# Comando per avviare l'applicazione
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]