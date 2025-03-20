using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plugin.LocalNotification; // Assicurati di aver installato il package
using Microsoft.Maui.ApplicationModel; // Per MainThread

namespace ParkSystemApp
{
    public class WebSocketClient
    {
        private ClientWebSocket _client = new ClientWebSocket();

        public async Task ConnectAsync(Uri uri)
        {
            var token = await SecureStorage.GetAsync("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token non trovato.");
                return;
            }
            try
            {
                // Imposta l'header Authorization con il token JWT
                _client.Options.SetRequestHeader("Authorization", "Bearer " + token);
                await _client.ConnectAsync(uri, CancellationToken.None);
                Console.WriteLine("Connesso al server WebSocket.");
                await ReceiveMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante la connessione: " + ex.Message);
            }
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[1024];

            while (_client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    Console.WriteLine("Connessione chiusa dal server.");
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Messaggio ricevuto: " + message);

                    // Mostra una notifica push locale sul dispositivo
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var notification = new NotificationRequest
                        {
                            BadgeNumber = 1,
                            Title = "Nuova Notifica",
                            Description = message,
                            ReturningData = "Dati opzionali", // se vuoi passare dati all'apertura della notifica
                            NotificationId = new Random().Next(1000, 9999) // ID univoco
                        };
                        LocalNotificationCenter.Current.Show(notification);
                    });
                }
            }
        }

        public async Task CloseAsync()
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Chiusura client", CancellationToken.None);
            }
        }
    }
}
