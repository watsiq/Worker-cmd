using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

internal class RMQ
{
    public ConnectionFactory connectionFactory;
    public IConnection connection;
    public IModel channel;
    private bool isReceiving = false;
    public void InitRMQConnection(string host = "cloudrmqserver.pptik.id", int port = 5672, string user = "tmdgdai",
     string pass = "tmdgdai", string vhost = "/tmdgdai")
    {
        connectionFactory = new ConnectionFactory();
        connectionFactory.HostName = host;
        //connectionFactory.Port = port;
        connectionFactory.UserName = user;
        connectionFactory.Password = pass;
        connectionFactory.VirtualHost = vhost;
    }
    public void CreateRMQConnection()
    {
        connection = connectionFactory.CreateConnection();
        Console.WriteLine("Koneksi " + (connection.IsOpen ? "Berhasil!" : "Gagal!"));
    }
    public void WaitingMessage(string queue_name)
    {
        using (channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: queue_name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Pesan diterima: {0}", message);
                int number = Int32.Parse(message);
                CountDown(number);
            };
            channel.BasicConsume(queue: queue_name,
            autoAck: true,
            consumer: consumer);
            Console.WriteLine(" Tekan [enter] untuk memutus koneksi.");
            Console.ReadLine();
            Disconnect();
        }
    }
    public void CountDown(int start)
    {
        int i;
        for (i = start; i > 0; i--)
        {
            Console.Write(i + " ");
            Thread.Sleep(1000);
        }
        Console.WriteLine("Done Computing");
    }
    public void Disconnect()
    {
        isReceiving = false;
        channel.Close();
        channel = null;
        Console.WriteLine("Channel ditutup!");
        if (connection.IsOpen)
        {
            connection.Close();
        }
        Console.WriteLine("Koneksi diputus!");
        connection.Dispose();
        connection = null;
    }
}