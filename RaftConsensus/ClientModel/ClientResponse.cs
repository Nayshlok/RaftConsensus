using RaftConsensus.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftConsensus.ClientModel
{
    [Serializable]
    public class ClientResponse
    {
        public bool Success { get; set; }
        public ServerInfo Leader { get; set; }
    }
}
