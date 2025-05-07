import time
import psycopg2
import pandas as pd
import os

# Connessione al database PostgreSQL
def connect_db():
    return psycopg2.connect(
        host=os.getenv('DB_HOST', 'localhost'),
        port=os.getenv('DB_PORT', '5432'),
        user=os.getenv('DB_USER', 'parksys'),
        password=os.getenv('DB_PASSWORD', ''),
        database=os.getenv('DB_NAME', 'parksystem_db')
    )

# Ottiene dati sulle attrazioni
def fetch_attractions(conn):
    query = "SELECT id_attrazione, nome, capacita_oraria, tipologia FROM attrazioni WHERE stato='attiva'"
    return pd.read_sql(query, conn)

# Ottiene prenotazioni del mese corrente
def fetch_bookings(conn):
    query = """
        SELECT *
        FROM prenotazioni
        WHERE date_trunc('month', data_prenotazione) = date_trunc('month', CURRENT_DATE)
        AND stato_prenotazione = 'attiva'
    """
    return pd.read_sql(query, conn)

# Statistiche giornaliere singole attrazioni
from datetime import datetime

def daily_stats(attractions, bookings):
    now = datetime.now()
    current_hour = now.hour

    today = now.replace(hour=0, minute=0, second=0, microsecond=0)
    bookings_today = bookings[bookings['data_prenotazione'].dt.normalize() == today]

    stats = []
    for _, attr in attractions.iterrows():
        id_attr = attr['id_attrazione']
        nome = attr['nome']
        capacita = attr['capacita_oraria']

        # Prenotazioni per l'ora corrente
        current_hour_bookings = bookings_today[
            (bookings_today['id_attrazione'] == id_attr) &
            (bookings_today['orario_previsto'].dt.hour == current_hour)
        ]
        percentuale_oraria = (len(current_hour_bookings) / capacita) * 100 if capacita > 0 else 0

        # Media giornaliera fino ad ora
        hourly_counts = bookings_today[
            bookings_today['id_attrazione'] == id_attr
        ].groupby(bookings_today['orario_previsto'].dt.hour).size()

        hourly_fill = hourly_counts / capacita * 100
        media_giornaliera = hourly_fill.mean() if not hourly_fill.empty else 0

        stats.append({
            'id_attrazione': id_attr,
            'nome': nome,
            'percentuale_riempimento_oraria': round(percentuale_oraria, 2),
            'percentuale_media_riempimento_giornaliera': round(media_giornaliera, 2)
        })

    return pd.DataFrame(stats)


# Statistiche mensili generali
def monthly_global_stats(attractions, bookings):
    stats = {}

    now = pd.Timestamp.now()
    current_month_days = now.days_in_month

    # Totale prenotazioni
    total_bookings = len(bookings)

    # 1. Media prenotazioni giornaliere
    media_giornaliera = total_bookings / current_month_days if current_month_days > 0 else 0
    stats['media_prenotazioni_giornaliere_mensile'] = round(media_giornaliera, 2)

    # 2. Classifica top 3 attrazioni
    top_counts = (
        bookings
        .merge(attractions[['id_attrazione', 'nome']], on='id_attrazione')
        .groupby('nome')
        .size()
        .sort_values(ascending=False)
    )
    top_3 = top_counts.head(3)
    top_3_perc = (top_3 / total_bookings * 100).round(2).to_dict()
    stats['top_3_attrazioni'] = top_3_perc

    # 3. Giorni aperti (con almeno una prenotazione)
    giorni_aperti = bookings['data_prenotazione'].dt.normalize().nunique()
    stats['giorni_aperti_mese_corrente'] = giorni_aperti

    # 4. Distribuzione oraria (heatmap)
    bookings['ora'] = bookings['orario_previsto'].dt.hour
    hour_distribution = (bookings['ora'].value_counts(normalize=True).sort_index() * 100).round(2).to_dict()
    stats['distribuzione_oraria_percentuale'] = hour_distribution

    # 5. Giorno con più prenotazioni
    daily_counts = bookings['data_prenotazione'].dt.date.value_counts().sort_values(ascending=False)
    if not daily_counts.empty:
        stats['giorno_con_piu_prenotazioni'] = {
            'data': str(daily_counts.index[0]),
            'numero_prenotazioni': int(daily_counts.iloc[0])
        }

    # 6. Giorno con meno prenotazioni (> 0)
    nonzero_days = daily_counts[daily_counts > 0]
    if not nonzero_days.empty:
        stats['giorno_con_meno_prenotazioni'] = {
            'data': str(nonzero_days.index[-1]),
            'numero_prenotazioni': int(nonzero_days.iloc[-1])
        }

    # 7. Numero medio di prenotazioni per utente
    prenotazioni_per_utente = bookings['id_utente'].value_counts()
    if not prenotazioni_per_utente.empty:
        media_per_utente = prenotazioni_per_utente.mean()
        stats['media_prenotazioni_per_utente'] = round(media_per_utente, 2)

    # 8. Tipo di attrazione più e meno prenotata
    bookings_with_type = bookings.merge(attractions[['id_attrazione', 'tipologia']], on='id_attrazione')
    type_counts = bookings_with_type['tipologia'].value_counts()
    if not type_counts.empty:
        stats['tipologia_piu_prenotata'] = type_counts.idxmax()
        stats['tipologia_meno_prenotata'] = type_counts.idxmin()

    return pd.DataFrame([stats])



# Main loop dello script
def main_loop(interval_minutes=5):
    first_run = True

    while True:
        print("Inizio nuovo ciclo analisi dati...")
        try:
            conn = connect_db()

            attractions = fetch_attractions(conn)
            bookings = fetch_bookings(conn)

            if first_run:
                print("Prima esecuzione: dati del mese corrente recuperati.")
                first_run = False

            # Calcolo statistiche
            daily = daily_stats(attractions, bookings)
            monthly = monthly_global_stats(attractions, bookings)

            # Salva in CSV (sovrascrivendo)
            daily.to_json('/data/daily_single_stats.json', orient='records', indent=2)
            monthly.to_json('/data/monthly_global_stats.json', orient='records', indent=2)



            print("Statistiche aggiornate con successo:")
            print(daily.head())
            print(monthly.head())

            conn.close()

        except Exception as e:
            print(f"Errore: {e}")

        print(f"Ciclo completato. Attendo {interval_minutes} minuti.")
        time.sleep(interval_minutes * 60)

if __name__ == "__main__":
    main_loop()
