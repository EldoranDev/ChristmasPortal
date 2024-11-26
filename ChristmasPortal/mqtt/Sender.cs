using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Internal;

namespace ChristmasPortal.mqtt;

public class Sender(IMqttClient mqttClient) : IDisposable
{
    public const string SetTopic = "xmaspi/set";
    
    public static async Task<Sender> Create(string broker, int port, string user, string password, CancellationToken cancellationToken)
    {
        var mqttFactory = new MqttFactory();

        var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithCredentials(user, password)
            .WithTcpServer(broker, port)
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);
        
        return new Sender(mqttClient);
    }

    public async Task Close()
    {
        await mqttClient.DisconnectAsync();
    }
    
    public async Task Send(string topic, object message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);
                            
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(SetMessage.Topic)
            .WithPayload(payload)
            .Build();
        
        await mqttClient.PublishAsync(mqttMessage, cancellationToken);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        mqttClient.Dispose();
    }
}