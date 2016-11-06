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
            var ipAddresses = new List<ServerInfo>();
            for(int i = 0; i < 5; i++)
            {
                ipAddresses.Add(new ServerInfo
                {
                    Id = i,
                    ServerAddress = new IPEndPoint(IPAddress.Loopback, 13000 + i)
                });
            }
            for(int i = 0; i < ipAddresses.Count; i++)
            {
                ServerState state = new ServerState
                {
                    Id = i,
                    ServerInfo = ipAddresses.Where(x => x.Id != i).ToList(),
                    CurrentState = CurrentState.Follower,
                    ThisServerInfo = ipAddresses[i]
                };
                ServerLogic server = new ServerLogic(state);
                server.Start();
            }
        }
    }
}
