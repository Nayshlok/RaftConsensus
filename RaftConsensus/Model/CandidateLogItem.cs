using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftConsensus.Model
{
    [Serializable]
    public class CandidateLogItem
    {
        public LogItem Item { get; set; }
        public int ServersCopied { get; set; }
    }
}
