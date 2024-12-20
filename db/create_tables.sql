CREATE TABLE IF NOT EXISTS utenti (
    id_utente           SERIAL PRIMARY KEY,
    nome                VARCHAR(100) NOT NULL,
    cognome             VARCHAR(100) NOT NULL,
    email               VARCHAR(255) UNIQUE NOT NULL,
    telefono            VARCHAR(20),
    password_hash       TEXT NOT NULL,
    tipo_utente         VARCHAR(20) CHECK (tipo_utente IN ('admin', 'visitatore')) NOT NULL DEFAULT 'visitatore',
    codice_amico        VARCHAR(50) UNIQUE NOT NULL,
    tipo_avventura      VARCHAR(100),
    data_registrazione  TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS attrazioni (
    id_attrazione       SERIAL PRIMARY KEY,
    nome                VARCHAR(100) NOT NULL,
    descrizione         TEXT,
    tipologia           VARCHAR(100),
    tematica            VARCHAR(100),
    eta_minima          INT CHECK (eta_minima >= 0),
    stato               VARCHAR(20) CHECK (stato IN ('attiva', 'manutenzione', 'chiusa')) NOT NULL DEFAULT 'attiva',
    capacita_oraria     INT CHECK (capacita_oraria > 0),
    orario_apertura     TIME,
    orario_chiusura     TIME
);

CREATE TABLE IF NOT EXISTS prenotazioni (
    id_prenotazione     SERIAL PRIMARY KEY,
    id_utente           INT NOT NULL REFERENCES utenti(id_utente) ON DELETE CASCADE,
    id_attrazione       INT NOT NULL REFERENCES attrazioni(id_attrazione) ON DELETE CASCADE,
    data_prenotazione   TIMESTAMP NOT NULL DEFAULT NOW(),
    orario_previsto     TIMESTAMP,
    stato_prenotazione  VARCHAR(20) CHECK (stato_prenotazione IN ('attiva', 'annullata')) DEFAULT 'attiva'
);

CREATE TABLE IF NOT EXISTS recensioni (
    id_recensione       SERIAL PRIMARY KEY,
    id_utente           INT NOT NULL REFERENCES utenti(id_utente) ON DELETE CASCADE,
    id_attrazione       INT NOT NULL REFERENCES attrazioni(id_attrazione) ON DELETE CASCADE,
    punteggio           INT CHECK (punteggio >= 1 AND punteggio <= 5) NOT NULL,
    commento            TEXT,
    data_recensione     TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE (id_utente, id_attrazione)
);

CREATE TABLE IF NOT EXISTS amicizie (
    id_richiesta        SERIAL PRIMARY KEY,
    id_utente_mittente  INT NOT NULL REFERENCES utenti(id_utente) ON DELETE CASCADE,
    id_utente_destinatario INT NOT NULL REFERENCES utenti(id_utente) ON DELETE CASCADE,
    stato_richiesta     VARCHAR(20) CHECK (stato_richiesta IN ('in_attesa', 'accettata', 'rifiutata')) DEFAULT 'in_attesa',
    data_richiesta      TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE (id_utente_mittente, id_utente_destinatario)
);

CREATE TABLE IF NOT EXISTS manutenzioni (
    id_manutenzione     SERIAL PRIMARY KEY,
    id_attrazione       INT NOT NULL REFERENCES attrazioni(id_attrazione) ON DELETE CASCADE,
    id_utente_admin     INT REFERENCES utenti(id_utente) ON DELETE SET NULL,
    descrizione_intervento TEXT,
    data_inizio         TIMESTAMP NOT NULL DEFAULT NOW(),
    data_fine           TIMESTAMP,
    tipo_intervento     VARCHAR(50),
    stato_manutenzione  VARCHAR(20) CHECK (stato_manutenzione IN ('in_corso', 'completata')) DEFAULT 'in_corso'
);