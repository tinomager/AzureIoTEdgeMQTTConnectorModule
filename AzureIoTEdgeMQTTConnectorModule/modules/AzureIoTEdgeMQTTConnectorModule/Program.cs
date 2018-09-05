namespace AzureIoTEdgeMQTTConnectorModule
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using Newtonsoft.Json;

    class Program
    {
        public static ModuleClient moduleClient = null;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            // create client instance
            try{
                MqttClient client = new MqttClient("localhost");
                // register to message received
                client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                string clientId = Guid.NewGuid().ToString();
                client.Connect(clientId);
                // subscribe to the topic "/home/temperature" with QoS 2
                client.Subscribe(new string[] { "/messages" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            catch(Exception ex)   
            {
                Console.WriteLine("Cannot connect to MQTT broker");
                Console.WriteLine("Exception: " + ex.Message + ex);
            }
            

            //init module client
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };
            moduleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await moduleClient.OpenAsync();         

        }

        static async void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
        // handle message received
            
            var stringMessage = Encoding.UTF8.GetString(e.Message);
            Console.WriteLine($"Message received from MQTT broker: {stringMessage}");

            var messageObject = JsonConvert.DeserializeObject<MqttMessage>(stringMessage);

            var pipeMessage = new Message(e.Message);
            await moduleClient.SendEventAsync("output1", pipeMessage);
        }
    }
}
