using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftConsensus.ClientModel
{
    [Serializable]
    public class ClientRequest
    {
        public string VariableName { get; set; }
        public int VariableValue { get; set; }
    }
}
