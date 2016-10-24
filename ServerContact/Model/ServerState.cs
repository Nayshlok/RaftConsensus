using System.Collections.Generic;

namespace RaftConsensus.Model
{
    public class ServerState
    {
        //Persistent State
        public int CurrentTerm { get; set; }
        public int? VotedForId { get; set; }
        private List<LogItem> _log = new List<LogItem>();
        public List<LogItem> Log { get; set; }

        //Volatile state
        public int CommittedIndex { get; set; }
        public int LastAppliedIndex { get; set; }

        //Leaders
        public List<ServerInfo> ServerInfo { get; set; }
        public CurrentState CurrentState { get; set; }
    }
}
