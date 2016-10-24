namespace RaftConsensus.Model
{
    public class VoteResponse
    {
        public int Term { get; set; }
        public bool VoteGranted { get; set; }
    }
}
