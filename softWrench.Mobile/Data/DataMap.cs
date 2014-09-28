using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using softWrench.Mobile.Metadata.Applications;

using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Data {
    public class DataMap : DataMapDefinition {
        public static string ConvertType(DateTime value) {
            return TypeConversion.FromDateTime(value);
        }

        private readonly string _application;
        private readonly IDictionary<string, string> _fields;
        private readonly IDictionary<string, string> _customFields;
        private readonly LocalState _localState;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataMap"/>
        ///     class using the specified parameters.
        /// </summary>
        /// <param name="application">The name of the application owning the data map.</param>
        /// <param name="fields">The initial data map fields.</param>
        /// <param name="customFields">The initial data map custom fields.</param>
        /// <param name="localState">The data map local state.</param>
        public DataMap(string application, IDictionary<string, string> fields, IDictionary<string, string> customFields, LocalState localState) {
            if (application == null) throw new ArgumentNullException("application");
            if (fields == null) throw new ArgumentNullException("fields");
            if (customFields == null) throw new ArgumentNullException("customFields");
            if (localState == null) throw new ArgumentNullException("localState");

            _application = application;
            _fields = fields;
            _customFields = customFields;
            _localState = localState;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataMap"/>
        ///     class using the specified application, field list
        ///     and custom field list, with a default local state.
        /// </summary>
        /// <param name="application">The name of the application owning the data map.</param>
        /// <param name="fields">The initial data map fields.</param>
        /// <param name="customFields">The initial data map custom fields.</param>
        public DataMap(string application, IDictionary<string, string> fields, IDictionary<string, string> customFields)
            : this(application, fields, customFields, new LocalState { LocalId = Guid.NewGuid(), IsLocal = true, IsBouncing = false }) {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataMap"/>
        ///     class using the specified application and field list,
        ///     with a default local state.
        /// </summary>
        /// <param name="application">The name of the application owning the data map.</param>
        /// <param name="fields">The initial data map fields.</param>
        public DataMap(string application, IDictionary<string, string> fields)
            : this(application, fields, new Dictionary<string, string>(), new LocalState { LocalId = Guid.NewGuid(), IsLocal = true, IsBouncing = false }) {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataMap"/>
        ///     class using the specified local state and with all
        ///     fields empty.
        /// </summary>
        /// <param name="applicationSchemaDefinition">The application metadata owning the data map.</param>
        /// <param name="localState">The data map local state.</param>
        public DataMap(ApplicationSchemaDefinition applicationSchemaDefinition, LocalState localState) {
            if (applicationSchemaDefinition == null) throw new ArgumentNullException("applicationSchemaDefinition");
            if (localState == null) throw new ArgumentNullException("localState");

            _application = applicationSchemaDefinition.Name;
            _localState = localState;

            _fields = applicationSchemaDefinition
                .Fields
                .ToDictionary(f => f.Attribute, f => "");

            _customFields = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataMap"/>
        ///     class using the default values.
        /// </summary>
        /// <param name="applicationSchemaDefinition">The application metadata owning the data map.</param>
        public DataMap(ApplicationSchemaDefinition applicationSchemaDefinition)
            : this(applicationSchemaDefinition, new LocalState { LocalId = Guid.NewGuid(), IsLocal = true, IsBouncing = false }) {
        }

        public string Application {
            get { return _application; }
        }

        public IDictionary<string, string> Fields {
            get { return _fields; }
        }

        /// <summary>
        ///     Gets or sets custom fields, which are intended to be
        ///     used solely by the client platform and thus should not
        ///     be synchronized with the server.
        /// </summary>
        public IDictionary<string, string> CustomFields {
            get { return _customFields; }
        }

        public LocalState LocalState {
            get { return _localState; }
        }

        public string Value(string name) {
            return _fields[name];
        }

        public string Value(ApplicationFieldDefinition field) {
            return Value(field.Attribute);
        }

        public T Value<T>(string name) {
            return TypeConversion.FromString<T>(Value(name));
        }

        public void Value(string name, string value) {
            _fields[name] = value;
        }

        public void Value(string name, DateTime value) {
            Value(name, TypeConversion.FromDateTime(value));
        }

        public void Value(string name, int value) {
            Value(name, TypeConversion.FromInt32(value));
        }

        public void Value(string name, decimal value) {
            Value(name, TypeConversion.FromDecimal(value));
        }

        public void Value(ApplicationFieldDefinition field, string value) {
            Value(field.Attribute, value);
        }

        public DataMap Previous { get; set; }

        public DataMap Next { get; set; }

        private static class TypeConversion {
            public static T FromString<T>(string value) {
                if (typeof(T) == typeof(DateTime)) {
                    return (T)(object)DateTime.Parse(value);
                }
                if (typeof(T) == typeof(int)) {
                    return (T)(object)int.Parse(value);
                }
                if (typeof(T) == typeof(decimal)) {
                    return (T)(object)decimal.Parse(value);
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }

            public static string FromDateTime(DateTime value) {
                return value.ToString("yyyy-MM-dd HH:mm:ss");
            }

            public static string FromInt32(int value) {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            public static string FromDecimal(decimal value) {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
