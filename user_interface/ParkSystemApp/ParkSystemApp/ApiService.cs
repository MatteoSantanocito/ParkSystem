using System;
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
        

        public async Task<string> LoginAsync(string email, string password)
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

                // Esempio: endpoint /login definito nel backend Go
                var response = await _httpClient.PostAsync("/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return $"Errore: {errorMessage}";
                }


                var json = await response.Content.ReadAsStringAsync();
                var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(json);


                // Se il token è presente, lo salviamo in SecureStorage
                if (!string.IsNullOrEmpty(loginResponse?.token))
                {
                    await SecureStorage.SetAsync("AuthToken", loginResponse.token);
                    System.Diagnostics.Debug.WriteLine($"TOKEN PER POSTMAN: {loginResponse.token}");

                }
                
                // Salviamo i dati utente in Preferences (così li carichiamo senza rifare query)
                if (loginResponse.user != null)
                {
                    Preferences.Set("UserName", loginResponse.user.Nome);
                    System.Diagnostics.Debug.WriteLine("UserName salvato: " + Preferences.Get("UserName", "vuoto"));
                    Preferences.Set("UserCognome", loginResponse.user.Cognome);
                    Preferences.Set("UserEmail", loginResponse.user.Email);
                    Preferences.Set("UserTipoAvventura", loginResponse.user.TipoAvventura);
                }
                
                return loginResponse.token;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "";
                return $"Errore: {ex.Message} | InnerException: {inner}";
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


            public async Task<List<Attrazione>> GetAttrazioniAsync()
            {
                try
                {
                    var response = await _httpClient.GetAsync("/attrazioni");
                    if (response.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        return await response.Content.ReadFromJsonAsync<List<Attrazione>>(options);
                    }
                    else
                    {
                        throw new Exception("Non è stato possibile recuperare le attrazioni");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Errore in GetAttrazioniAsync: {ex}");
                    throw;
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


    }
}
