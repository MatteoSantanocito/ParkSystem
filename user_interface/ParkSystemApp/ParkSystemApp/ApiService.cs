using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            // Cambia l'URL con l'indirizzo del tuo backend (API Gateway o simile)
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://10.0.2.2:8080") // Esempio per emulatore Android
            };
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            try
            {
                var loginData = new
                {
                    Username = username,
                    Password = password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json"
                );

                // Esempio: endpoint per login definito nel backend
                var response = await _httpClient.PostAsync("/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return $"Errore: {errorMessage}";
                }

                var json = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(json);

                // Se il token è presente, lo salviamo in SecureStorage
                if (!string.IsNullOrEmpty(loginResponse.Token))
                {
                    await SecureStorage.SetAsync("AuthToken", loginResponse.Token);
                }

                return loginResponse.Token;
            }
            catch (Exception ex)
            {
                // Qui catturiamo l'eccezione e logghiamo anche l’InnerException per una diagnosi più completa
                var inner = ex.InnerException?.Message ?? "";
                return $"Errore: {ex.Message} | InnerException: {inner}";
            }
        }

        private class LoginResponse
        {
            public string Token { get; set; }
        }
    }
}
