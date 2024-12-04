using HttpPiping.Setting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HttpPiping.Core
{
    public class FlowManager : IFlowManager
    {
        private readonly ILogger? _logger;
        private readonly Dictionary<int, PipeSetting> _redirectSettings = new Dictionary<int, PipeSetting>();
        private List<RuleToSetting> _ruleToSettings = new List<RuleToSetting>();

        private Dictionary<Guid, IDestinationWorker> _destinations = new Dictionary<Guid, IDestinationWorker>();

        public FlowManager(ILogger? logger, HttpPipingSetting setting)
        {
            _logger = logger;
            foreach (var redirectSetting in setting.Pipes)
            {
                if (_redirectSettings.TryGetValue(redirectSetting.Id, out _))
                {
                    _logger?.LogError("Duplicate redirectSetting Id: {redirectSettingId}", redirectSetting.Id);
                    continue;
                }

                _redirectSettings.Add(redirectSetting.Id, redirectSetting);
                _logger?.LogInformation("RedirectSetting Added: {redirectSetting}", redirectSetting);
            }

            var ruleToSettings = new Dictionary<string, RuleToSetting>();
            foreach (var rule in setting.FlowRules)
            {
                if (ruleToSettings.TryGetValue(rule.HostPattern, out var _))
                {
                    _logger?.LogError("Duplicate redirect {rule} -> {groupDestinationId}", rule, rule.PipeId);
                    continue;
                }

                if (!_redirectSettings.TryGetValue(rule.PipeId, out var redirectSetting))
                {
                    _logger?.LogError("RedirectSetting not found -> {groupDestinationId}", rule.PipeId);
                    continue;
                }

                var ruleToSetting = new RuleToSetting(rule, redirectSetting);
                ruleToSettings.Add(rule.HostPattern, ruleToSetting);
                _logger?.LogInformation("Redirect Added: {rule} -> {redirectSetting}", rule, redirectSetting);
            }
            _ruleToSettings = ruleToSettings.Values.ToList();

        }

        public void Create(Guid key, string host, ushort port, bool keepAlive, IDestinationToSource destinationToSource)
        {
            var redirectSetting = GetRedirectSetting(in host, in port);

            if (redirectSetting == null)
            {
                _logger?.LogInformation("Direct {host}:{port}", host, port);
                _destinations.Add(key, new SocketDestination(_logger, key, host, port, keepAlive, destinationToSource));
            }
            else
            {
                _logger?.LogInformation("Redirect {host}:{port} -> {redirectSetting}", host, port, redirectSetting);
                _destinations.Add(key, new WebSocketDestination(_logger, redirectSetting.GetUriAddress(), key, host, port, keepAlive, destinationToSource));
            }
        }

        private PipeSetting? GetRedirectSetting(in string host, in ushort port)
        {

            foreach (var item in _ruleToSettings)
            {
                if (item.IsMatch(host))
                    return item.Setting;
            }

            return null;
        }

        public void SendQuery(Guid key, string query)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.SendQuery(query);
        }

        public void SendData(Guid key, byte[] data)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.SendData(data);
        }

        public void StartReceiveData(Guid key)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.StartReceiveData();
        }

        public void Destroy(Guid key)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.Destroy();
            _destinations.Remove(key);
        }
    }
}
