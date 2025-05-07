using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json.Serialization;
using ParkSystemApp.Models;
using System.Buffers.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http.Json;


namespace ParkSystemApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            // Cambia con il tuo IP/porta
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://10.0.2.2:8080")
            };
        }

        // Classe UserInfo unificata (spostata fuori da LoginResponse)
         public class UserInfo
        {
            public int Id { get; set; }

            [JsonPropertyName("nome")]
            public string Nome { get; set; }

            [JsonPropertyName("cognome")]
            public string Cognome { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("tipo_utente")]
            public string TipoUtente { get; set; }

            [JsonPropertyName("tipo_avventura")]
            public string TipoAvventura { get; set; }
        }

        public class FriendshipListResponse
        {
            public List<FriendInfo> Accepted { get; set; }
            public List<PendingFriendRequest> Pending { get; set; }
        }

        public class FriendInfo
        {
            [JsonPropertyName("id_richiesta")]
            public int IdRichiesta { get; set; }

            [JsonPropertyName("full_name")]
            public string FullName { get; set; }

            [JsonPropertyName("tipo_avventura")]
            public string TipoAvventura { get; set; }

            [JsonPropertyName("amico_da")]
            public string AmicoDa { get; set; }

            [JsonPropertyName("data_accettazione")]
            [JsonConverter(typeof(JsonDateTimeConverter))]
            public DateTime DataAccettazione { get; set; }
        }

        public class JsonDateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (DateTime.TryParse(reader.GetString(), out DateTime date))
                    {
                        return date;
                    }
                }
                return DateTime.MinValue;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("o"));
            }
        }

        public class PendingFriendRequest
        {
            [JsonPropertyName("id_richiesta")]
            public int IdRichiesta { get; set; }

            [JsonPropertyName("mittente_nome")]
            public string MittenteNome { get; set; }

            [JsonPropertyName("mittente_cognome")]
            public string MittenteCognome { get; set; }

            [JsonPropertyName("data_richiesta")]
            [JsonConverter(typeof(JsonDateTimeConverter))]           
            public DateTime DataRichiesta { get; set; }

            [JsonIgnore]
            public string FullName => $"{MittenteNome} {MittenteCognome}";

            [JsonIgnore]
            public string DataFormattata => 
                DataRichiesta == DateTime.MinValue ? 
                "Data non disponibile" : 
                DataRichiesta.ToLocalTime().ToString("dd/MM/yyyy");
        }

        private class LoginResponse
        {
            public string token { get; set; }
            public UserInfo user { get; set; }
        }
        


        public class LoginResult
        {
            public string Token { get; set; }
            public string UserType { get; set; }
            public string ErrorMessage { get; set; }
        }

        public async Task<LoginResult> LoginAsync(string email, string password)
        {

            try

            {
                var loginData = new

                {
                    Email = email,
                    Password = password

                };


                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json"

                );


                var response = await _httpClient.PostAsync("/login", content);

                if (!response.IsSuccessStatusCode)

                {

                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new LoginResult { ErrorMessage = $"Errore: {errorMessage}" };

                }

                var json = await response.Content.ReadAsStringAsync();

                var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(json);

                if (!string.IsNullOrEmpty(loginResponse?.token))

                {

                    await SecureStorage.SetAsync("AuthToken", loginResponse.token);

                }
                // Salviamo i dati utente in Preferences (così li carichiamo senza rifare query)

                if (loginResponse.user != null)

                {

                    Preferences.Set("UserName", loginResponse.user.Nome);

                    System.Diagnostics.Debug.WriteLine("UserName salvato: " + Preferences.Get("UserName", "vuoto"));

                    Preferences.Set("UserCognome", loginResponse.user.Cognome);

                    Preferences.Set("UserEmail", loginResponse.user.Email);

                    Preferences.Set("UserTipoAvventura", loginResponse.user.TipoAvventura);


                    Console.WriteLine(loginResponse.user);

                }

                return new LoginResult { Token = loginResponse.token, UserType = loginResponse.user.TipoUtente };

            }

            catch (Exception ex)

            {
                var inner = ex.InnerException?.Message ?? "";

                return new LoginResult { ErrorMessage = $"Errore: {ex.Message} | InnerException: {inner}" };

            }

        }

        public async Task<string> RegisterAsync(string nome, string cognome, string tipoAvventura, string email, string password)
        {
            try
            {
                var registerData = new
                {
                    Nome = nome,
                    Cognome = cognome,
                    tipo_avventura = tipoAvventura,
                    Email = email,
                    Password = password
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(registerData),
                    Encoding.UTF8,
                    "application/json"
                );

                
                var response = await _httpClient.PostAsync("/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return $"Errore: {errorMessage}";
                }

                // Se tutto ok, restituisco il messaggio di successo
                var successMessage = await response.Content.ReadAsStringAsync();
                return successMessage;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "";
                return $"Errore: {ex.Message} | InnerException: {inner}";
            }
        }

        public async Task<string> ChangeEmailAsync(string newEmail)
        {
            try
            {
                // Recuperiamo il token dall'archivio sicuro
                var token = await SecureStorage.GetAsync("AuthToken");
                System.Diagnostics.Debug.WriteLine($"Token recuperato: {token}");
                if (string.IsNullOrEmpty(token))
                {
                    return "Errore: Nessun token presente. Utente non loggato?";
                }

                var payload = new
                {
                    NuovaEmail = newEmail
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsync("/account/email", content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return $"Errore: {err}";
                }

                return "OK";
            }
            catch (Exception ex)
            {
                return $"Errore: {ex.Message}";
            }
        }

        public async Task<string> ChangePasswordAsync(string oldPass, string newPass)
        {
            try
            {
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return "Errore: Nessun token presente. Utente non loggato?";
                }

                var payload = new
                {
                    VecchiaPassword = oldPass,
                    NuovaPassword = newPass
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsync("/account/password", content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return $"Errore: {err}";
                }

                return "OK";
            }
            catch (Exception ex)
            {
                return $"Errore: {ex.Message}";
            }
        }

        public async Task<string> DeleteAccountAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return "Errore: Nessun token presente. Utente non loggato?";
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync("/account");
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return $"Errore: {err}";
                }

                return "OK";
            }
            catch (Exception ex)
            {
                return $"Errore: {ex.Message}";
            }
        }


        public async Task<String> BookAttractionAsync(int id)
        {
            var token = await SecureStorage.GetAsync("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return "Errore: Nessun token presente. Utente non loggato?";
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Creazione del payload JSON
            var payload = new
            {
                AttractionID = id
            };


            // Serializzazione del payload in JSON
            var content = new StringContent(
                                            System.Text.Json.JsonSerializer.Serialize(payload),
                                            Encoding.UTF8,
                                            "application/json");

            // Invio della richiesta POST al server
            try
            {
                var response = await _httpClient.PostAsync("/book/create", content);
                if (response.IsSuccessStatusCode)
                {
                    // Se il server restituisce successo, possiamo anche leggere e restituire il corpo della risposta se necessario
                    return await response.Content.ReadAsStringAsync();  // Supponiamo che il backend restituisca una conferma o dettagli della prenotazione
                }
                else
                {
                    // Legge il messaggio di errore dal server e lo restituisce
                    return $"Errore nella prenotazione: {await response.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                return $"Errore di connessione al server: {ex.Message}";
            }
        }

        public async Task<string> DeleteBookingAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return "Errore: Nessun token presente. Utente non loggato?";
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync("/book/delete");
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return $"Errore: {err}";
                }

                return "Prenotazione cancellata con successo";
            }
            catch (Exception ex)
            {
                return $"Errore: {ex.Message}";
            }
        }


        public async Task<List<Attrazione>> GetAttrazioniAsync(string tipoUtente)
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("/attrazioni");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // ignora maiuscole/minuscole nei nomi dei campi
                    };
                    List<Attrazione> attrazioni = JsonSerializer.Deserialize<List<Attrazione>>(jsonResponse, options);

                    if (tipoUtente == "visitatore")
                    {
                        attrazioni = attrazioni.Where(a => a.State == "attiva").ToList();
                        return attrazioni;
                    }
                    return attrazioni;
                }

                else

                {
                    // Gestisci il caso in cui la risposta non sia successo
                    throw new Exception("Non è stato possibile recuperare le attrazioni");
                }
            }

            catch (Exception ex)

            {

                // Gestisci eventuali eccezioni

                throw new Exception($"Errore durante il recupero delle attrazioni: {ex.Message}");

            }

        }

        public async Task<String> SendRating(int attractionId, int rating)
            {
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return "Errore: Nessun token presente. Utente non loggato?";
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Creazione del payload da inviare
                var payload = new
                {
                    AttractionId = attractionId,
                    Rating = rating
                };

                // Serializzazione del payload in JSON
                var content = new StringContent(
                                                System.Text.Json.JsonSerializer.Serialize(payload),
                                                Encoding.UTF8,
                                                "application/json");

                // Invio della richiesta POST al backend
                try
                {
                    var response = await _httpClient.PostAsync("/ratings", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        return $"Errore: {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    return $"Eccezione: {ex.Message}";
                }

            }

        // Metodi per la gestione delle amicizie
            public async Task<UserInfo> SearchUserAsync(string userCode)
            {
                try
                {
                    var token = await SecureStorage.GetAsync("AuthToken");
                    if (string.IsNullOrEmpty(token))
                    {
                        throw new Exception("Utente non autenticato");
                    }

                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.GetAsync($"/friendship/search?code={Uri.EscapeDataString(userCode)}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        throw new Exception(error);
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return await response.Content.ReadFromJsonAsync<UserInfo>(options);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Errore in SearchUserAsync: {ex}");
                    throw;
                }
            }

        public async Task<string> SendFriendRequestAsync(string friendCode)
            {
                try
                {
                    var token = await SecureStorage.GetAsync("AuthToken");
                    if (string.IsNullOrEmpty(token))
                    {
                        return "Errore: Utente non autenticato";
                    }

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    // Modifica il payload per accettare una stringa
                    var payload = new { 
                        codice_amico = friendCode // Ora accetta stringhe alfanumeriche
                    };

                    var content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await _httpClient.PostAsync("/friendship/request", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return $"Errore: {errorMessage}";
                    }
                    
                    return "Richiesta inviata con successo";
                }
                catch (Exception ex)
                {
                    return $"Errore: {ex.Message}";
                }
            }


        public async Task<string> AcceptFriendRequestAsync(int requestId)
        {
            try
            {
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return "Errore: Utente non autenticato";
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var payload = new { id_richiesta = requestId };
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PutAsync("/friendship/accept", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return $"Errore: {errorMessage}";
                }
                return "Richiesta accettata con successo";
            }
            catch (Exception ex)
            {
                return $"Errore: {ex.Message}";
            }
        }

        public async Task<string> RejectFriendRequestAsync(int requestId)
        {
            try
            {
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return "Errore: Utente non autenticato";
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var payload = new { id_richiesta = requestId };
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PutAsync("/friendship/reject", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return $"Errore: {errorMessage}";
                }
                return "Richiesta rifiutata con successo";
            }
            catch (Exception ex)
            {
                return $"Errore: {ex.Message}";
            }
        }

        public async Task<FriendshipListResponse> GetFriendshipsAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Utente non autenticato");
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync("/friendship/list");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonDateTimeConverter() }
                    };
                    return await response.Content.ReadFromJsonAsync<FriendshipListResponse>(options);
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Errore API: {error}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore in GetFriendshipsAsync: {ex}");
                throw;
            }
        }



        public async Task<String> AggiornaAttrazioneAsync(Attrazione attrazione)
        {
            var token = await SecureStorage.GetAsync("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return "Errore: Utente non autenticato";
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {   id = attrazione.ID,
                nome = attrazione.Nome,
                descrizione = attrazione.Descrizione,
                tipologia = attrazione.Tipologia,
                tematica = attrazione.Tematica,
                minimumAge = attrazione.MinimumAge,
                state = attrazione.State,
                hourCapacity = attrazione.HourCapacity
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync("/attrazioni/modify", content);

            if (response.IsSuccessStatusCode)
            {
                return "OK";
            }

            var error = await response.Content.ReadAsStringAsync();
            return $"Errore dal server: {error}";
        }


        public async Task<string> EliminaAttrazioneAsync(int idAttrazione)
        {
            var token = await SecureStorage.GetAsync("AuthToken");
            if (string.IsNullOrEmpty(token))
                return "Errore: Utente non autenticato";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var payload = new { id = idAttrazione };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("attrazioni/delete", content);

            if (response.IsSuccessStatusCode)
                return "OK";

            var error = await response.Content.ReadAsStringAsync();
            return $"Errore: {error}";
        }




        public async Task<String> InserisciAttrazioneAsync(Attrazione attrazione)
        {
            var token = await SecureStorage.GetAsync("AuthToken");
            if (string.IsNullOrEmpty(token))
                return "Errore: Utente non autenticato";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);


            var content = new StringContent(
                JsonSerializer.Serialize(attrazione),
                Encoding.UTF8,
                "application/json"
            );

            // Effettua una richiesta POST per inserire l'attrazione
            var response = await _httpClient.PostAsync("attrazioni/insert", content);

            if (response.IsSuccessStatusCode)
                return "OK";  // Operazione riuscita

            var error = await response.Content.ReadAsStringAsync();
            return $"Errore: {error}";  // Restituisce il messaggio di errore dalla risposta
        }

        public async Task<List<FriendBookingInfo>> GetFriendsBookingsAsync(int attractionId)
        {
            try
            {
                // VERIFICA 1: Token esiste?
                var token = await SecureStorage.GetAsync("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Token JWT mancante");
                }

                // VERIFICA 2: URL corretto?
                var url = $"/friendship/bookings?attraction_id={attractionId}";
                Debug.WriteLine($"[API] Chiamando: {url}");

                // VERIFICA 3: Headers corretti?
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                // VERIFICA 4: Timeout sufficiente?
                _httpClient.Timeout = TimeSpan.FromSeconds(15);

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"[API] Risposta: {response.StatusCode}\n{content}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API error: {response.StatusCode}");
                }

                // VERIFICA 5: Deserializzazione corretta?
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonDateTimeConverter() }
                };
                
                return JsonSerializer.Deserialize<List<FriendBookingInfo>>(content, options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[API CRITICAL] {ex}");
                throw;
            }
        }

        public class FriendBookingInfo
        {
            public int IdUtente { get; set; }
            public string Nome { get; set; }
            public string Cognome { get; set; }
            public string PrenotatoIl { get; set; }
            
            public string FullName => $"{Nome} {Cognome}";
        }



        public async Task<List<GlobalStats>> GetGlobalStatsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("/stats/global");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    List<GlobalStats> statsList = JsonSerializer.Deserialize<List<GlobalStats>>(jsonResponse, options);

                    if (statsList != null)
                        return statsList;
                    else
                        throw new Exception("Il JSON ricevuto è nullo");
                }
                else
                {
                    throw new Exception("Non è stato possibile recuperare le statistiche globali");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore durante il recupero delle statistiche globali: {ex.Message}");
            }
        }


        public async Task<DailyStat> GetDailyStatsByAttrazioneIdAsync(int idAttrazione)
        {
            try
            {
                string url = $"/stats/daily?id={idAttrazione}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    DailyStat stat = JsonSerializer.Deserialize<DailyStat>(jsonResponse, options);
                    return stat;
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // Nessuna statistica trovata
                }
                else
                {
                    throw new Exception("Errore nel recupero delle statistiche giornaliere.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore durante la richiesta delle statistiche giornaliere: {ex.Message}");
            }
        }






    }


}
