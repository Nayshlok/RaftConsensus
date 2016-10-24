using System;
using System.Net;

namespace RaftConsensus.Model
{
    [Serializable]
    public class ServerInfo
    {
        public int Id { get; set; }
        public IPEndPoint ServerAddress { get; set; }
        public int NextIndex { get; set; }
        public int MatchIndex { get; set; }
    }
}
