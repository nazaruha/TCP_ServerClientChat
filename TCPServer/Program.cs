using System.Net;
using System.Net.Sockets;
using System.Text;
using Context;
using Context.Entities;
using TCPUserMessage;

namespace TCPServer
{
    public class Program
    {
        private static string ipAddress = "127.0.0.1";
        private static int port = 1002;
        private static readonly object _lock = new object();
        private static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        private static MyDataContext context;
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            TcpListener server = new TcpListener(ip, port);
            server.Start();
            context = new MyDataContext();
            Console.WriteLine("Server started");

            int count = 1;
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                lock (_lock) list_clients.Add(count, client);
                Console.WriteLine($"\nClient #{count} connected!");
                Thread thread = new Thread(handle_client);
                thread.Start(count);
                count++;
            }
        }

        public static void handle_client(object c)
        {
            int id = (int)c;
            TcpClient client;
            lock(_lock) client = list_clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Client end-point: {0}", client.Client.RemoteEndPoint);
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                if (byte_count == 0) break;
                string data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                broadcast(data);
                Console.WriteLine(data);
                TCPUserMessage.UserMessage TCPuser = TCPUserMessage.UserMessage.Deserialize(buffer);
                if (TCPuser.MessageType == TypeMessage.Login || TCPuser.MessageType == TypeMessage.Logout) continue;
                Context.Entities.UserMessage user = new Context.Entities.UserMessage() { Name = TCPuser.Name, Message = TCPuser.Text, Date = DateTime.Now};
                context.Users.Add(user);
                context.SaveChanges();
            }
            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void broadcast(string data)
        { 
            byte[] buffer = Encoding.UTF8.GetBytes(data);

            lock (_lock)
            {
                foreach (TcpClient client in list_clients.Values)
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}