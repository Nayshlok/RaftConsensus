namespace RaftConsensus.Model
{
    public class AppendEntriesRequest
    {
        public int Term { get; set; }
        public ServerInfo Leader { get; set; }
        public int PrevLogIndex { get; set; }
        public int PrevLogTerm { get; set; }
        public LogItem[] entries { get; set; }
        public int LeaderCommitIndex { get; set; }
    }
}
