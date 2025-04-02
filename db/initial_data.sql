-- Dati iniziali per avviare il sistema: un utente admin e alcune attrazioni di base

INSERT INTO utenti (nome, cognome, email, password_hash, tipo_utente, codice_amico, tipo_avventura)
VALUES
('Admin', 'Park', 'admin@gmail.com', '$2a$10$JwJkvkuWDBLQ5PQU.7wI6u4Y4Dc9OilG7kU', 'admin', 'CODICEADMIN01', 'Avventura');

INSERT INTO utenti (nome, cognome, email, password_hash, tipo_utente, codice_amico, tipo_avventura)
VALUES
('Matteo', 'Santanocito', 'matteosantanocito@outlook.it', 'password', 'visitatore', 'CODICEMARIO01', 'Famiglia');

INSERT INTO attrazioni (nome, descrizione, tipologia, tematica, eta_minima, stato, capacita_oraria)
VALUES 
('Montagna Russa', 'Attrazione ad alta velocità', 'Thrill Ride', 'Fantasy', 12, 'attiva', 500),
('Carosello', 'Giostra classica per tutte le età', 'Giostra', 'Famiglia', 0, 'chiusa', 300),
('Atlantide', 'Giostra ad alta velocità', 'Thrill Ride', 'Fantasy', 14, 'attiva', 400),
('Ruota Panoramica', 'Giostra panoramica', 'Giostra', 'Famiglia', 0, 'attiva', 200),
('Casa Stregata', 'Attrazione horror', 'Dark Ride', 'Horror', 16, 'attiva', 100),
('Treno Fantasma', 'Attrazione horror', 'Dark Ride', 'Horror', 16, 'attiva', 100),
('Pirati', 'Attrazione acquatica', 'Water Ride', 'Adventure', 10, 'chiusa', 300),
('Canyon', 'Attrazione acquatica', 'Water Ride', 'Adventure', 10, 'attiva', 300),
('Cinema 4D', 'Attrazione cinematografica', 'Show', 'Fantasy', 0, 'attiva', 100),
('Teatro', 'Attrazione teatrale', 'Show', 'Fantasy', 0, 'attiva', 100),
('Casetta', 'Attrazione per bambini', 'Playground', 'Famiglia', 3, 'attiva', 100);

INSERT INTO prenotazioni (id_utente, id_attrazione, orario_previsto)
VALUES ((SELECT id_utente FROM utenti WHERE email='matteosantanocito@outlook.it'),
        (SELECT id_attrazione FROM attrazioni WHERE nome='Montagna Russa'),
        NOW() + INTERVAL '1 hour');