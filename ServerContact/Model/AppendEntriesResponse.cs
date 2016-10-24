namespace RaftConsensus.Model
{
    public class AppendEntriesResponse
    {
        public int Term { get; set; }
        public bool Success { get; set; }
    }
}
