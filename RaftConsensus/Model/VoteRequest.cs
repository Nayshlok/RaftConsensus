﻿using System;

namespace RaftConsensus.Model
{
    [Serializable]
    public class VoteRequest
    {
        public int Term { get; set; }
        public int CandidateId { get; set; }
        public int LastLogIndex { get; set; }
        public int LastLogTerm { get; set; }

    }
}
