using System;
using System.Threading;
using System.Linq;
using System.Net.Sockets;
using RaftConsensus.Model;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using RaftConsensus.ClientModel;

namespace RaftConsensus
{
    public class ServerLogic
    {
        private static Random _rand = new Random();
        private Thread _listeningThread;
        private ServerState _state;
        private int _votesReceived;
        private Timer _heartBeat;
        private Timer _election;
        private TcpListener listener;
        private List<CandidateLogItem> _candidateEntries = new List<CandidateLogItem>();
        private int ElectionTimeout { get { return _rand.Next(5000, 10000); } }

        public ServerLogic(ServerState state = null)
        {
            _election = new Timer(ChangeToCandidate);
            _heartBeat = new Timer(StartHeartbeat);
            _state = state ?? new ServerState();
            _listeningThread = new Thread(ServerStart);
            _listeningThread.Name = "ServerThread" + state.Id;
        }

        public void Start()
        {
            listener = new TcpListener(_state.ThisServerInfo.ServerAddress);
            _listeningThread.Start();
            Run();
        }

        public void ServerStart()
        {
            try
            {
                Console.WriteLine("Starting server " + _state.Id);
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
                        _election.Change(ElectionTimeout, Timeout.Infinite);
                        response = AppendEntry(append);
                    }
                    var clientRequest = obj as ClientRequest;
                    if(clientRequest != null)
                    {
                        response = AddEntry(clientRequest);
                    }
                    if (response != null)
                    {
                        formatter.Serialize(socket.GetStream(), response);
                    }
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
            switch (_state.CurrentState)
            {
                case CurrentState.Follower:
                    var timeout = ElectionTimeout;
                    _election.Change(timeout, Timeout.Infinite);
                    break;
                case CurrentState.Candidate:
                    var followerTimer = new Timer(ChangeToFollower);
                    followerTimer.Change(4000, Timeout.Infinite);
                    _state.CurrentTerm++;
                    _votesReceived = 0;
                    _votesReceived++;
                    foreach(var info in _state.ServerInfo)
                    {
                        var votingThread = new Thread(RunVotingProcess);
                        votingThread.Start(info);
                    }
                    break;
                case CurrentState.Leader:
                    Console.WriteLine($"Server {_state.Id} is now sending heartbeat.");
                    _heartBeat.Change(0, 500);
                    break;
            }
        }

        private void ChangeToCandidate(object state)
        {
            Console.WriteLine($"Server {_state.Id} is becoming a candidate");
            _state.CurrentState = CurrentState.Candidate;
            _state.VotedForId = null;
            Run();
        }

        private void ChangeToFollower(object state)
        {
            Console.WriteLine($"Server {_state.Id} just became a follower");
            _state.CurrentState = CurrentState.Follower;
            _state.VotedForId = null;
            Run();
        }

        public void StartHeartbeat(object state)
        {
            foreach (var info in _state.ServerInfo)
            {
                var initialHeartBeatThread = new Thread(SendHeartBeat);
                initialHeartBeatThread.Start(info);
            }
        }

        public void SendHeartBeat(object serverInfo)
        {
            _election.Change(ElectionTimeout, Timeout.Infinite);
            if (serverInfo is ServerInfo)
            {
                var info = (ServerInfo)serverInfo;
                var appendRequest = new AppendEntriesRequest
                {
                    Leader = _state.ThisServerInfo,
                    LeaderCommitIndex = _state.CommittedIndex,
                    PrevLogIndex = _state.LastAppliedIndex,
                    PrevLogTerm = _state.Log.Count <= _state.CommittedIndex
                        ? 0
                        : _state.Log[_state.CommittedIndex].Term,
                    Term = _state.CurrentTerm
                };
                SendAppend(info, appendRequest);
            }
        }

        public AppendEntriesResponse SendAppend(ServerInfo serverInfo, AppendEntriesRequest appendRequest)
        {
            TcpClient client = new TcpClient();
            client.Connect(serverInfo.ServerAddress);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(client.GetStream(), appendRequest);
            var response = formatter.Deserialize(client.GetStream());
            if (response is AppendEntriesResponse)
            {
                var appendResponse = (AppendEntriesResponse)response;
                if (appendResponse.Term > _state.CurrentTerm)
                {
                    _state.CurrentState = CurrentState.Follower;
                    Run();
                }
                return appendResponse;
            }
            return null;
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
                    CandidateId = _state.Id,
                    LastLogIndex = _state.CommittedIndex,
                    LastLogTerm = _state.Log.Count <= _state.CommittedIndex 
                        ? 0 
                        : _state.Log[_state.CommittedIndex].Term,
                    Term = _state.CurrentTerm
                };
                formatter.Serialize(client.GetStream(), voteRequest);
                var response = formatter.Deserialize(client.GetStream());
                if(response is VoteResponse)
                {
                    var voteResponse = (VoteResponse)response;
                    if(voteResponse.Term > _state.CurrentTerm && _state.CurrentState == CurrentState.Candidate)
                    {
                        _state.CurrentTerm = voteResponse.Term;
                        _state.CurrentState = CurrentState.Follower;
                        _votesReceived = 0;
                        Run();
                        return;
                    }
                    else if (voteResponse.VoteGranted && _state.CurrentState == CurrentState.Candidate)
                    {
                        _votesReceived++;
                        if(_votesReceived > _state.ServerInfo.Count / 2 && _state.CurrentState == CurrentState.Candidate)
                        {
                            _state.CurrentState = CurrentState.Leader;
                            Run();
                        }
                    }
                }
            }
        }

        public VoteResponse GetVote(VoteRequest request)
        {
            var validTerm = _state.CurrentTerm >= request.Term;
            var ableToVote = _state.VotedForId == null || _state.VotedForId == request.CandidateId;
            var logIsValid = ((_state.Log.LastOrDefault()?.Term ?? 0) == request.LastLogTerm)
                && _state.CommittedIndex == request.LastLogIndex;
            if (validTerm && ableToVote && logIsValid)
            {
                _state.VotedForId = request.CandidateId;
                return new VoteResponse
                {
                    Term = _state.CurrentTerm,
                    VoteGranted = true
                };
            }
            return new VoteResponse
            {
                Term = _state.CurrentTerm,
                VoteGranted = false
            };
        }

        public AppendEntriesResponse AppendEntry(AppendEntriesRequest request)
        {
            Console.WriteLine($"Server {_state.Id} received a heartbeat");
            _election.Change(ElectionTimeout, Timeout.Infinite);
            var termIsValid = _state.CurrentTerm >= request.Term;
            var entryMatches = false;
            var logIsLongEnough = _state.Log.Count > request.PrevLogIndex;
            if (logIsLongEnough)
            {
                entryMatches = _state.Log[request.PrevLogIndex].Term == request.PrevLogTerm;
            }
            if (request.Term > _state.CurrentTerm)
            {
                _state.CurrentTerm = request.Term;
                _state.CurrentState = CurrentState.Follower;
            }
            if (termIsValid && entryMatches)
            {
                _state.VotedForId = null;
                _state.VotedForId = null;
                _state.LeaderId = request.Leader.Id;
                if (logIsLongEnough)
                {
                    _state.Log.RemoveRange(request.PrevLogIndex, _state.Log.Count - request.PrevLogIndex);
                }
                if (request.entries != null)
                {
                    Console.WriteLine($"Server {_state.Id} "
                             + $"appending {string.Join<string>(", ", request.entries.Select(x => $"{x.Name}, {x.Value}"))}");
                    _state.Log.AddRange(request.entries);
                }
                if (request.LeaderCommitIndex > _state.CommittedIndex)
                {
                    _state.CommittedIndex = Math.Min(request.LeaderCommitIndex, _state.Log.Count - 1);
                }
                return new AppendEntriesResponse
                {
                    Term = _state.CurrentTerm,
                    Success = true
                };
            }
            return new AppendEntriesResponse
            {
                Term = _state.CurrentTerm,
                Success = false
            };
        }

        public ClientResponse AddEntry(ClientRequest request)
        {
            if(_state.CurrentState != CurrentState.Leader)
            {
                return new ClientResponse
                {
                    Success = false,
                    Leader = _state.ServerInfo.FirstOrDefault(x => x.Id == _state.LeaderId)
                };
            }
            Console.WriteLine($"Server {_state.Id} received request ({request.VariableName}, {request.VariableValue})");
            var logItem = new LogItem
            {
                Name = request.VariableName,
                Value = request.VariableValue,
                Term = _state.CurrentTerm
            };
            _state.LastAppliedIndex++;
            _state.Log.Add(logItem);
            var clientResponse = new ClientResponse();
            var serverCopyCount = 0;
            foreach(var info in _state.ServerInfo)
            {
                var response = SendAppend(info, new AppendEntriesRequest
                {
                    entries = new[] { logItem},
                    Leader = _state.ThisServerInfo,
                    LeaderCommitIndex = _state.CommittedIndex,
                    PrevLogIndex = _state.CommittedIndex,
                    PrevLogTerm = _state.Log[_state.CommittedIndex].Term,
                    Term = _state.CurrentTerm
                });
                if (response.Success)
                {
                    serverCopyCount++;
                }
            }
            if (serverCopyCount > 3)
            {
                _state.CommittedIndex++;
                clientResponse.Success = true;
            }
            foreach(var info in _state.ServerInfo)
            {
                var heartBeatThread = new Thread(SendHeartBeat);
                heartBeatThread.Start(info);
            }
            return clientResponse;
        }
    }
}
