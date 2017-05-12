using System;
using softwrench.sw4.Shared2.Metadata.Entity;

namespace softwrench.sW4.Shared2.Metadata.Entity.Association {
    public class EntityAssociationAttribute : IQueryHolder {

        public bool Primary {
            get; set;
        }
        public string To {
            get; set;
        }
        public string From {
            get; set;
        }
        public string Literal {
            get; set;
        }
        public bool QuoteLiteral {
            get; set;
        }

        public string Query {
            get; set;
        }

        public bool AllowsNull {
            get; set;
        }

        public bool HasFromAndTo() {
            return From != null && To != null;
        }

        /// <summary>
        /// If true this query should be included on the sync process. Not that this only makes sense for query attributes that does not, 
        /// usually, rely on the parent entity itself, since we're dealing with fetching lit of entities.
        /// 
        /// For example a query that picks the userid and orgid of the current user
        /// </summary>
        public bool IncludeOnSync { get; set; }

        public EntityAssociationAttribute() {
        }


        public EntityAssociationAttribute(string to, string @from, string query, bool primary = false, bool allowsNull = false, bool includeOnSync = false) {

            From = @from;
            To = to;
            Primary = primary;
            Query = query;
            AllowsNull = allowsNull;
            IncludeOnSync = includeOnSync;
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

        public string GetQueryReplacingMarkers(string entityName, string fromValue = null) {
            var queryReplacingMarkers = Query.Replace("!@", entityName + ".");
            if (fromValue != null) {
                queryReplacingMarkers = queryReplacingMarkers.Replace("@from", "'" + fromValue + "'");
            }
            return queryReplacingMarkers;
        }


        public override string ToString() {
            return string.Format("From: {0}, To: {1}, Literal: {2}, QuoteLiteral: {3}, Primary: {4}", From, To, Literal, QuoteLiteral, Primary);
        }

        public EntityAssociationAttribute Clone() {
            if (Literal == null) {
                return new EntityAssociationAttribute(To, From, Query, Primary, AllowsNull, IncludeOnSync);
            }
            return new EntityAssociationAttribute(QuoteLiteral, To, From, Literal);
        }
    }
}