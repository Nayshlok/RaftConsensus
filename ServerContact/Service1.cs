using RaftConsensus.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ServerContact
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public ServerState state { get; set; }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public VoteResponse GetVote(VoteRequest request)
        {
            var validTerm = state.CurrentTerm >= request.Term;
            var ableToVote = state.VotedForId == null || state.VotedForId == request.CandidateId;
            var logIsValid = ((state.Log.LastOrDefault()?.Term ?? 0) == request.LastLogTerm)
                && state.CommittedIndex == request.LastLogIndex;
            if(validTerm && ableToVote && logIsValid)
            {
                state.VotedForId = request.CandidateId;
                return new VoteResponse
                {
                    Term = state.CurrentTerm,
                    VoteGranted = true
                };
            }
            return new VoteResponse
            {
                Term = state.CurrentTerm,
                VoteGranted = false
            };
        }

        public AppendEntriesResponse AppendEntry(AppendEntriesRequest request)
        {
            var termIsValid = state.CurrentTerm >= request.Term;
            var entryMatches = false;
            var logIsLongEnough = state.Log.Count >= request.PrevLogIndex;
            if (logIsLongEnough)
            {
                entryMatches = state.Log[request.PrevLogIndex].Term == request.PrevLogTerm;
            }
            if(request.Term > state.CurrentTerm)
            {
                state.CurrentTerm = request.Term;
                state.CurrentState = CurrentState.Follower;
            }
            if(termIsValid && entryMatches)
            {
                return new AppendEntriesResponse
                {
                    Term = state.CurrentTerm,
                    Success = true
                };
            }
            if (termIsValid && !entryMatches)
            {
                if (logIsLongEnough)
                {
                    state.Log.RemoveRange(request.PrevLogIndex, state.Log.Count - request.PrevLogIndex);
                }
                if (request.entries != null)
                {
                    state.Log.AddRange(request.entries);
                }
                if (request.LeaderCommitIndex > state.CommittedIndex)
                {
                    state.CommittedIndex = Math.Min(request.LeaderCommitIndex, state.Log.Count - 1);
                }
            }
            return new AppendEntriesResponse
            {
                Term = state.CurrentTerm,
                Success = false
            };
        }

        public CurrentState getCurrentState()
        {
            return state.CurrentState;
        }

        public void SetState(CurrentState newState)
        {
            state.CurrentState = newState;
        }
    }
}
