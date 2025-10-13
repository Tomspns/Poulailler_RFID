using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RFIDwpf.RFID
{
    public static class MqttClientService
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        public static async Task InitializeAsync()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithClientId("poulailler_local_client")
                .WithTcpServer("172.31.254.159", 1883)
                .Build();

            _client.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("🔌 MQTT déconnecté, reconnexion...");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try { await _client.ConnectAsync(_options); }
                catch { Console.WriteLine("❌ Reconnexion échouée"); }
            });

            await _client.ConnectAsync(_options);
            Console.WriteLine("✅ Connecté au broker MQTT");
        }

        public static async Task PublishEtatPouleAsync(string id, string nom, string etat)
        {
            if (_client == null || !_client.IsConnected)
                await InitializeAsync();

            var message = new
            {
                id,
                nom,
                etat,
                horodatage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            string payload = JsonConvert.SerializeObject(message);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic("poulailler/poules")
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithAtLeastOnceQoS()
                .Build();

            await _client.PublishAsync(mqttMessage);
            Console.WriteLine($"📤 État envoyé pour {nom} : {etat}");
        }
    }
}
