using System.Runtime.Serialization;

namespace HttpPiping.Setting
{
    [DataContract]
    public class FlowRule
    {
        [DataMember]
        public int PipeId { get; set; }
        [DataMember]
        public string HostPattern { get; set; }

        public override string ToString() => $"{HostPattern} to {PipeId}";
    }
}
