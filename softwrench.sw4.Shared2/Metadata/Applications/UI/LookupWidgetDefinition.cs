using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Applications.UI
{
    public class LookupWidgetDefinition : IWidgetDefinition
    {
        private readonly string _sourceApplication;
        private readonly string _sourceField;
        private readonly IEnumerable<string> _sourceDisplay;
        private readonly string _targetField;
        private readonly string _targetQualifier;
        private readonly IEnumerable<Filter> _filters;

        public LookupWidgetDefinition(string sourceApplication, string sourceField, IEnumerable<string> sourceDisplay, string targetField,  string targetQualifier, IEnumerable<Filter> filters)
        {
            if (sourceApplication == null) throw new ArgumentNullException("sourceApplication");
            if (sourceField == null) throw new ArgumentNullException("sourceField");
            if (sourceDisplay == null) throw new ArgumentNullException("sourceDisplay");
            if (targetField == null) throw new ArgumentNullException("targetField");
            if (filters == null) throw new ArgumentNullException("filters");

            _sourceApplication = sourceApplication;
            _sourceField = sourceField;
            _sourceDisplay = sourceDisplay;
            _targetField = targetField;
            _targetQualifier = targetQualifier;
            _filters = filters;
        }

        public string Type
        {
            get { return GetType().Name; }
        }

        public string SourceApplication
        {
            get { return _sourceApplication; }
        }

        public string SourceField
        {
            get { return _sourceField; }
        }

        public IEnumerable<string> SourceDisplay
        {
            get { return _sourceDisplay; }
        }

        public string TargetField
        {
            get { return _targetField; }
        }

        public string TargetQualifier
        {
            get { return _targetQualifier; }
        }

        public IEnumerable<Filter> Filters
        {
            get { return _filters; }
        }

        public class Filter
        {
            private readonly string _sourceField;
            private readonly string _targetField;
            private readonly string _literal;

            public Filter(string sourceField, string targetField, string literal)
            {
                if (sourceField == null) throw new ArgumentNullException("sourceField");

                _sourceField = sourceField;
                _targetField = targetField;
                _literal = literal;
            }

            public string SourceField
            {
                get { return _sourceField; }
            }

            public string TargetField
            {
                get { return _targetField; }
            }

            public string Literal
            {
                get { return _literal; }
            }
        }
    }
}