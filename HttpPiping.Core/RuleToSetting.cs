using HttpPiping.Setting;
using System.Text.RegularExpressions;

namespace HttpPiping.Core
{
    public class RuleToSetting
    {
        public readonly FlowRule Rule;
        public readonly PipeSetting Setting;
        public readonly Regex Regex;

        public RuleToSetting(FlowRule rule, PipeSetting setting)
        {
            Rule = rule;
            Setting = setting;
            string hostPattern = Regex.Escape(rule.HostPattern).Replace("\\*", ".*?");
            Regex = new Regex(hostPattern);
        }

        internal bool IsMatch(string host)
        {
            return Regex.IsMatch(host);
        }
    }
}
