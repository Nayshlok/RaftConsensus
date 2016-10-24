using System;

namespace RaftConsensus.Model
{
    [Serializable]
    public class VoteResponse
    {
        public int Term { get; set; }
        public bool VoteGranted { get; set; }
    }
}
