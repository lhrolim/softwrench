﻿using System;

namespace softwrench.sW4.Shared2.Metadata.Entity.Association {
    public class EntityAssociationAttribute {

        public bool Primary { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Literal { get; set; }
        public bool QuoteLiteral { get; set; }

        public string Query { get; set; }

        public EntityAssociationAttribute() { }

        public EntityAssociationAttribute(string to, string @from, string query, bool primary = false) {

            From = @from;
            To = to;
            Primary = primary;
            Query = query;
        }

        public EntityAssociationAttribute(bool quoteLiteral, string to, string from, string literal) {
            if (literal == null) throw new ArgumentNullException("literal");
            if (from == null && to == null) {
                throw new InvalidOperationException("either to or from attributes must be provided");
            }
            To = to;
            From = from;
            Literal = literal;
            QuoteLiteral = quoteLiteral;
        }

        public string GetQueryReplacingMarkers(String entityName) {
            return Query.Replace("!@", entityName + ".");
        }


        public override string ToString() {
            return string.Format("From: {0}, To: {1}, Literal: {2}, QuoteLiteral: {3}, Primary: {4}", From, To, Literal, QuoteLiteral, Primary);
        }
    }
}