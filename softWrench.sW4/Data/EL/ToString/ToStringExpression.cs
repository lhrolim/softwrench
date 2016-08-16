using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data.EL
{
    public class ToStringExpression
    {
        private readonly string _constExpression;
        private readonly IList<String> _fieldNames;

        public ToStringExpression(string constExpression, IList<string> fieldNames)
        {
            this._constExpression = constExpression;
            this._fieldNames = fieldNames;
        }

        public string apply(DataMap map)
        {
            var parameters = new object[_fieldNames.Count];
            for (int i = 0; i < _fieldNames.Count(); i++)
            {
                dynamic obj = map[_fieldNames[i]];
                parameters[i] = obj;
            }
            return String.Format(_constExpression, parameters);
        }

        public string ConstExpression
        {
            get { return _constExpression; }
        }

        public IList<string> FieldNames
        {
            get { return _fieldNames; }
        }
    }
}
