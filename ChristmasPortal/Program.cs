using ChristmasPortal.mqtt;
using MQTTnet;
using MQTTnet.Client;
using LegoDimensions;
using LegoDimensions.Portal;
using Color = ChristmasPortal.mqtt.Color;

namespace ChristmasPortal
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            
            var broker = Environment.GetEnvironmentVariable("MQTT_BROKER");

            if (string.IsNullOrEmpty(broker))
            {
                return;
            }

            var url = new Uri(broker);
            
            using var mqttSender = await Sender.Create(
                url.Host,
                url.Port,
                Environment.GetEnvironmentVariable("MQTT_USER")!,
                Environment.GetEnvironmentVariable("MQTT_PASSWORD")!,
                cts.Token
            );

            using var portal = LegoPortal.GetFirstPortal();
            
            portal.Flash(Pad.All, new FlashPad{Color = LegoDimensions.Color.Green, Enabled = true, TickCount = 20, TickOn = 5, TickOff = 5});
            
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            portal.LegoTagEvent += async (object? sender, LegoTagEventArgs e) =>
            {
                if (!e.Present) return;
                
                switch (e.Pad)
                {
                    case Pad.Center:
                        await mqttSender.Send(Sender.SetTopic, new SetMessage
                        {
                            State = "OFF"
                        }, cts.Token);
                        break;
                    case Pad.Left:
                        await mqttSender.Send(Sender.SetTopic, new SetMessage
                        {
                            State = "ON",
                            Color = new Color { R = 200, G = 200, B = 200 },
                            Brightness = 150
                        }, cts.Token);
                        break;
                    case Pad.Right:
                        await mqttSender.Send(Sender.SetTopic, new SetMessage
                        {
                            State = "ON",
                            Effect = "blue-white-static"
                        }, cts.Token);
                        break;
                }
            };
            
            await WaitForCancel(cts.Token);
            await mqttSender.Close();
        }

        private static async Task WaitForCancel(CancellationToken token)
        {
            while (true) {
                try
                {
                    await Task.Delay(-1, token);
                } catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
