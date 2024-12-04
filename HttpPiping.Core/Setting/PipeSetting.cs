using System.Runtime.Serialization;

namespace HttpPiping.Setting
{
    [DataContract]
    public class PipeSetting
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public PipeType Type { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public bool IsSecure { get; set; }

        public Uri GetUriAddress()
        {
            if (Type == PipeType.Socket)
                throw new NotImplementedException();

            return IsSecure ?
               new Uri($"wss://{Address}") :
               new Uri($"ws://{Address}");
        }

        public override string ToString() => $"[{Id}] {Type}, {Address}{(IsSecure ? "(Secured)" : "")}";
    }
}
