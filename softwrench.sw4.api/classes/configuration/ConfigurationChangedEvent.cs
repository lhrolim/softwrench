using cts.commons.simpleinjector.Events;

namespace softwrench.sw4.api.classes.configuration {

    public class ConfigurationChangedEvent : ISWEvent {
        public bool AllowMultiThreading => true;

        public ConfigurationChangedEvent(string configKey, string previousValue, string currentValue) {
            ConfigKey = configKey;
            PreviousValue = previousValue;
            CurrentValue = currentValue;
        }

        public string ConfigKey { get; }

        public string PreviousValue { get; }

        public string CurrentValue { get; }
    }
}