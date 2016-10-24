using System;

namespace RaftConsensus.Model
{
    [Serializable]
    public enum CurrentState
    {
        Follower, Candidate, Leader
    }
}
