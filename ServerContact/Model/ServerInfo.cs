using System;

namespace RaftConsensus.Model
{
    public class ServerInfo
    {
        public int Id { get; set; }
        public Uri ServerAddress { get; set; }
        public int NextIndex { get; set; }
        public int MatchIndex { get; set; }
    }
}
