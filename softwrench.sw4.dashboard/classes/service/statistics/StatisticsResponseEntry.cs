namespace softwrench.sw4.dashboard.classes.service.statistics {
    public class StatisticsResponseEntry {

        private readonly string _fieldValue;
        private readonly int _fieldCount;
        private readonly string _fieldLabel;

        /// <param name="fieldValue"></param>
        /// <param name="fieldCount"></param>
        /// <param name="fieldLabel">Value to display (usually a description or formatted label). Defaults to FieldValue if null.</param>
        public StatisticsResponseEntry(string fieldValue, int fieldCount, string fieldLabel = null) {
            _fieldValue = fieldValue;
            _fieldCount = fieldCount;
            _fieldLabel = fieldLabel ?? fieldValue;
        }

        public string FieldValue { get { return _fieldValue; } }

        public int FieldCount { get { return _fieldCount; } }

        public string FieldLabel { get { return _fieldLabel; } }
    }
}
