-- Dati iniziali per avviare il sistema: un utente admin e alcune attrazioni di base

INSERT INTO utenti (nome, cognome, email, password_hash, tipo_utente, codice_amico, tipo_avventura)
VALUES
('Admin', 'Park', 'admin@gmail.com', 'password', 'admin', 'CODICEADMIN01', 'Avventura');

INSERT INTO utenti (nome, cognome, email, password_hash, tipo_utente, codice_amico, tipo_avventura)
VALUES
('Matteo', 'Santanocito', 'matteosantanocito@outlook.it', 'password', 'visitatore', 'CODICEMARIO01', 'Famiglia');

INSERT INTO attrazioni (nome, descrizione, tipologia, tematica, eta_minima, stato, capacita_oraria)
VALUES 
('Montagna Russa', 'Attrazione ad alta velocità', 'Thrill Ride', 'Fantasy', 12, 'attiva', 500),
('Carosello', 'Giostra classica per tutte le età', 'Giostra', 'Famiglia', 0, 'chiusa', 300);

INSERT INTO prenotazioni (id_utente, id_attrazione, orario_previsto)
VALUES ((SELECT id_utente FROM utenti WHERE email='matteosantanocito@outlook.it'),
        (SELECT id_attrazione FROM attrazioni WHERE nome='Montagna Russa'),
        NOW() + INTERVAL '1 hour');