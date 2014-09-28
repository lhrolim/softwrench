using System;
using System.Collections.Generic;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Metadata.Extensions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Data
{
    public class DataOperation
    {
        private readonly Guid _localId;
        private readonly string _application;
        private readonly IReadOnlyDictionary<string, string> _data;
        private readonly string _handler;

        public DataOperation(string application, IReadOnlyDictionary<string, string> data, string handler, Guid localId)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (data == null) throw new ArgumentNullException("application");
            if (handler == null) throw new ArgumentNullException("handler");

            _localId = localId;
            _application = application;
            _data = data;
            _handler = handler;
        }

        public DataOperation(ApplicationSchemaDefinition application, IReadOnlyDictionary<string, string> data, Type handler)
            : this(application.ApplicationName, data, handler.FullName, Guid.NewGuid())
        {
        }

        public Guid LocalId
        {
            get { return _localId; }
        }

        public string Application
        {
            get { return _application; }
        }

        public IReadOnlyDictionary<string, string> Data
        {
            get { return _data; }
        }

        public string Handler
        {
            get { return _handler; }
        }
    }
}