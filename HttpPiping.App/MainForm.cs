using Boilerplates;
using HttpPiping.Core;
using HttpPiping.Setting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Xml;

namespace HttpPiping.App
{
    public partial class MainForm : Form, ILogger
    {
        private readonly HttpPipingSetting? _setting;
        public MainForm(HttpPipingSetting setting)
        {
            InitializeComponent();
            Text = $"Http Piping v{Aid.AppVersion}";

            StartButton.Enabled = true;
            StopButton.Enabled = false;

            _setting = setting;

        }

        PipeServer? _server;
        LogLevel _currentLogLevel = LogLevel.Information;

        private void StartButton_Click(object sender, EventArgs e) => Start();
        private void StopButton_Click(object sender, EventArgs e) => Stop();
        private void ClearLogsButton_Click(object sender, EventArgs e) => LogBox.Clear();
        private void TraceEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _currentLogLevel = TraceEnabledCheckBox.Checked
                ? LogLevel.Trace
                : LogLevel.Information;
        }

        private void Start()
        {
            var listenerPort = (ushort)PortInput.Value;
            var destinationManager = new FlowManager(this as ILogger, _setting);

            _server = new PipeServer(this as ILogger, destinationManager, IPAddress.Any, listenerPort).Start();

            StartButton.Enabled = PortInput.Enabled = false;
            StopButton.Enabled = true;
        }

        private void Stop()
        {
            _server?.Stop();
            _server = null;

            StartButton.Enabled = PortInput.Enabled = true;
            StopButton.Enabled = false;
        }

        #region ILogger
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string message = string.Empty;
            if (formatter != null)
            {
                message += formatter(state, exception);
            }

            var timetag = DateTime.Now;
            this.InvokeIfNecessary(() => LogBox.AppendText($"{timetag:HH:mm:ss.fff}> {logLevel.ToString()} - {eventId.Id} {message}\r\n"));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _currentLogLevel <= logLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }
        #endregion
    }
}