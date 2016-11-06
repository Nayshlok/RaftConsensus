using System.Collections.Generic;

namespace RaftConsensus.Model
{
    public class ServerState
    {
        //Persistent State
        public int Id { get; set; }
        public int CurrentTerm { get; set; }
        public int? VotedForId { get; set; }
        public List<LogItem> Log { get; set; } = new List<LogItem>();
        public ServerInfo ThisServerInfo { get; set;}
        public int LeaderId { get; set; }

        //Volatile state
        public int CommittedIndex { get; set; }
        public int LastAppliedIndex { get; set; }

        //Leaders
        public List<ServerInfo> ServerInfo { get; set; } = new List<ServerInfo>();
        public CurrentState CurrentState { get; set; } = CurrentState.Follower;
    }
}
