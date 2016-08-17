using cts.commons.simpleinjector.Events;

namespace softWrench.sW4.Configuration.Services.Api {
    public class ConfigurationChangedEvent : ISWEvent {
        public bool AllowMultiThreading { get { return true; } }

        private readonly string _configKey;
        private readonly string _previousValue;
        private readonly string _currentValue;

        public ConfigurationChangedEvent(string configKey, string previousValue, string currentValue) {
            _configKey = configKey;
            _previousValue = previousValue;
            _currentValue = currentValue;
        }

        public string ConfigKey { get { return _configKey; } }
        public string PreviousValue { get { return _previousValue; } }
        public string CurrentValue { get { return _currentValue; } }
    }
}