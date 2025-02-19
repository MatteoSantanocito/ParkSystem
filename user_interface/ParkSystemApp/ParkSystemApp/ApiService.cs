using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json.Serialization;
using ParkSystemApp.Models;
using Newtonsoft.Json;


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

        private class LoginResponse
        {
            public string token { get; set; }
            public UserInfo user { get; set; }
        }

        
        private class UserInfo
        {
            
            [JsonPropertyName("nome")]
            public string Nome { get; set; }

            [JsonPropertyName("cognome")]
            public string Cognome { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }
            [JsonPropertyName("tipo_avventura")]
            public string TipoAvventura { get; set; }
           
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
                HttpResponseMessage response = await _httpClient.GetAsync("/attrazioni");
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    List<Attrazione> attrazioni = JsonConvert.DeserializeObject<List<Attrazione>>(jsonResponse);
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

    }
}
