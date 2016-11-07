using RaftConsensus.ClientModel;
using RaftConsensus.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RaftClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            var serverInfo = new ServerInfo
            {
                Id = 0,
                ServerAddress = new IPEndPoint(IPAddress.Loopback, 13000)
            };
            var info = serverInfo;
            TcpClient client = new TcpClient();
            client.Connect(info.ServerAddress);
            BinaryFormatter formatter = new BinaryFormatter();
            var clientRequest = new ClientRequest
            {
                VariableName = "x",
                VariableValue = 2
            };
            Console.Read();
            formatter.Serialize(client.GetStream(), clientRequest);
            var obj = formatter.Deserialize(client.GetStream());
            var response = obj as ClientResponse;
            if (response != null)
            {
                if (!response.Success)
                {
                    Console.WriteLine($"Connection failed. Sending to {response.Leader.ServerAddress}, id {response.Leader.Id}");
                    client.Close();
                    client.Connect(response.Leader.ServerAddress);
                    formatter.Serialize(client.GetStream(), clientRequest);
                }
            }
            Console.WriteLine("Press any key to close");
            Console.ReadLine();
        }
    }
}
