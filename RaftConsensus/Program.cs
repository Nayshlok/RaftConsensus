using RaftConsensus.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace RaftConsensus
{
    class Program
    {
        public static IPEndPoint IpEndpoint { get; private set; }

        public static void Main(string[] args)
        {
            ServerLogic server = new ServerLogic();
            Thread.Sleep(1000);
            var vote = new VoteRequest
            {
                CandidateId = 1,
                LastLogIndex = 2,
                LastLogTerm = 2,
                Term = 3
            };
            BinaryFormatter formatter = new BinaryFormatter();
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 13000);
            TcpClient client = new TcpClient();
            client.Connect(endpoint);
            formatter.Serialize(client.GetStream(), vote);
            var obj = formatter.Deserialize(client.GetStream());
            var voteResponse = obj as VoteResponse;
            //ClientLogic client = new ClientLogic();
            //client.Run();
        }
    }
}
