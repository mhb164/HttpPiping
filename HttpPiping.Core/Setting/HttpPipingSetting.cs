using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HttpPiping.Setting
{
    public class HttpPipingSetting
    {
        [DataMember]
        public List<PipeSetting> Pipes { get; set; } = new List<PipeSetting>();
        [DataMember]
        public List<FlowRule> FlowRules { get; set; } = new List<FlowRule>();
    }
}
