using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

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
                    JsonSerializer.Serialize(loginData),
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
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(json);

                // Se il token è presente, lo salviamo in SecureStorage
                if (!string.IsNullOrEmpty(loginResponse?.Token))
                {
                    await SecureStorage.SetAsync("AuthToken", loginResponse.Token);
                }

                return loginResponse.Token;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "";
                return $"Errore: {ex.Message} | InnerException: {inner}";
            }
        }

        private class UserResponse
        {
            public int ID { get; set; }
            public string Nome { get; set; }
            public string Cognome { get; set; }
            public string TipodiAvventura { get; set; }
            public string Email { get; set; }
            public string CodiceAmico { get; set; }
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
                    JsonSerializer.Serialize(registerData),
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

        private class LoginResponse
        {
            public string Token { get; set; }
            public UserResponse User { get; set; }
        }
    }
}
