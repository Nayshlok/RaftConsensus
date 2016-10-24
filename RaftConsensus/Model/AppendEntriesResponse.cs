using System;

namespace RaftConsensus.Model
{
    [Serializable]
    public class AppendEntriesResponse
    {
        public int Term { get; set; }
        public bool Success { get; set; }
    }
}
