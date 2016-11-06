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
            Console.WriteLine("Enter the server to send the request to");
            var serverId = int.Parse(Console.ReadLine());
            var serverInfo = new ServerInfo
            {
                Id = serverId,
                ServerAddress = new IPEndPoint(IPAddress.Loopback, 13000 + serverId)
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
            formatter.Serialize(client.GetStream(), clientRequest);
            var obj = formatter.Deserialize(client.GetStream());
            var response = obj as ClientResponse;
            if (response != null)
            {

            }
            Console.WriteLine("Press any key to close");
            Console.ReadLine();
        }
    }
}
