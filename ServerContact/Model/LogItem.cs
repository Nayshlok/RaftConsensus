
namespace RaftConsensus.Model
{
    public class LogItem
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public int Term { get; set; }
    }
}
