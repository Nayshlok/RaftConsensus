using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftConsensus.Model
{
    public enum CurrentState
    {
        Follower, Candidate, Leader
    }
}
