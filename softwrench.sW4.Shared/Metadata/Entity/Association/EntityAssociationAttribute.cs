using System;

namespace softwrench.sW4.Shared.Metadata.Entity.Association {
    public class EntityAssociationAttribute {
        private readonly bool _primary;
        private readonly string _to;
        private readonly string _from;
        private readonly string _literal;
        private readonly bool _quoteLiteral;

        public EntityAssociationAttribute( string to,  string @from, bool primary = false) {
            if (to == null) throw new ArgumentNullException("to");
            if (@from == null) throw new ArgumentNullException("from");

            _from = @from;
            _to = to;
            _primary = primary;
        }

        public EntityAssociationAttribute(bool quoteLiteral,  string to,  string literal) {
            if (to == null) throw new ArgumentNullException("to");
            if (literal == null) throw new ArgumentNullException("literal");

            _to = to;
            _literal = literal;
            _quoteLiteral = quoteLiteral;
        }

        
        public string To {
            get { return _to; }
        }

        public string From {
            get { return _from; }
        }

        
        public string Literal {
            get { return _literal; }
        }

        public bool QuoteLiteral {
            get { return _quoteLiteral; }
        }

        public bool Primary {
            get { return _primary; }
        }

        public override string ToString() {
            return string.Format("From: {0}, To: {1}, Literal: {2}, QuoteLiteral: {3}, Primary: {4}", _from, _to, _literal, _quoteLiteral, _primary);
        }
    }
}