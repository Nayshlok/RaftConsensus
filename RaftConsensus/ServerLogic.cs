using System;
using System.Threading;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using RaftConsensus.Model;
using System.Runtime.Serialization.Formatters.Binary;

namespace RaftConsensus
{
    public class ServerLogic
    {
        private Thread ListeningThread;
        private ServerState state;
        private int port;
        private int votesReceived;

        public ServerLogic(int port = 13000, ServerState state = null)
        {
            state = state ?? new ServerState();
            ListeningThread = new Thread(ServerStart);
            ListeningThread.Start();
        }

        public void ServerStart()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            try
            {
                Console.WriteLine("Starting server");
                listener.Start();

                while (!Console.KeyAvailable || Console.ReadKey().KeyChar != 'q')
                {
                    var socket = listener.AcceptTcpClient();
                    BinaryFormatter formatter = new BinaryFormatter();
                    var obj = formatter.Deserialize(socket.GetStream());
                    var vote = obj as VoteRequest;
                    object response = null;
                    if(vote != null)
                    {
                        response = GetVote(vote);
                    }
                    var append = obj as AppendEntriesRequest;
                    if(append != null)
                    {
                        response = AppendEntry(append);
                    }
                    formatter.Serialize(socket.GetStream(), response);
                }
            }
            finally
            {
                Console.WriteLine("Ending server");
                listener.Stop();
            }
        }

        public void Run()
        {
            switch (state.CurrentState)
            {
                case CurrentState.Follower:

                    break;
                case CurrentState.Candidate:
                    state.CurrentTerm++;
                    votesReceived = 0;
                    votesReceived++;
                    foreach(var info in state.ServerInfo)
                    {

                    }
                    break;
                case CurrentState.Leader:
                    break;
            }
        }

        public void RunVotingProcess(object serverInfo)
        {
            if(serverInfo is ServerInfo)
            {
                var info = (ServerInfo)serverInfo;
                TcpClient client = new TcpClient();
                client.Connect(info.ServerAddress);
                BinaryFormatter formatter = new BinaryFormatter();
                var voteRequest = new VoteRequest
                {
                    CandidateId = state.Id,
                    LastLogIndex = state.CommittedIndex,
                    LastLogTerm = state.Log[state.CommittedIndex].Term,
                    Term = state.CurrentTerm
                };
                formatter.Serialize(client.GetStream(), voteRequest);
                var response = formatter.Deserialize(client.GetStream());
                if(response is VoteResponse)
                {
                    var voteResponse = (VoteResponse)response;
                    if(voteResponse.Term > state.CurrentTerm)
                    {
                        state.CurrentTerm = voteResponse.Term;
                        state.CurrentState = CurrentState.Follower;
                        votesReceived = 0;
                    }
                    else if (voteResponse.VoteGranted && state.CurrentState == CurrentState.Candidate)
                    {
                        votesReceived++;
                        if(votesReceived > state.ServerInfo.Count / 2 && state.CurrentState == CurrentState.Candidate)
                        {
                            state.CurrentState = CurrentState.Leader;
                            Run();
                        }
                    }
                }
            }
        }

        public VoteResponse GetVote(VoteRequest request)
        {
            var validTerm = state.CurrentTerm >= request.Term;
            var ableToVote = state.VotedForId == null || state.VotedForId == request.CandidateId;
            var logIsValid = ((state.Log.LastOrDefault()?.Term ?? 0) == request.LastLogTerm)
                && state.CommittedIndex == request.LastLogIndex;
            if (validTerm && ableToVote && logIsValid)
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
            if (request.Term > state.CurrentTerm)
            {
                state.CurrentTerm = request.Term;
                state.CurrentState = CurrentState.Follower;
            }
            if (termIsValid && entryMatches)
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
    }
}
