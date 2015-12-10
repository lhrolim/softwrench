﻿using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using NHibernate.Linq;

namespace softWrench.sW4.Util {
    public static class DictionaryExtensions {


        public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other) {
            if (other == null) {
                return dictionary;
            }
            other.ForEach(dictionary.Add);
            return dictionary;
        }


        public static ExpandoObject ToExpando(this IDictionary<string, string> dictionary) {
            var tempDict = dictionary.ToObjectDir();
            return tempDict.ToExpando();
        }

        public static Dictionary<string, object> ToObjectDir(this IDictionary<string, string> dictionary) {
            var tempDict = new Dictionary<string, object>();
            foreach (var entry in dictionary) {
                tempDict.Add(entry.Key, entry.Value);
            }
            return tempDict;
        }



        /// <summary>
        /// Extension method that turns a dictionary of string and object to an ExpandoObject
        /// </summary>
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary) {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;

            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in dictionary) {
                // if the value can also be turned into an ExpandoObject, then do it!
                if (kvp.Value is IDictionary<string, object>) {
                    var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                } else if (kvp.Value is ICollection) {
                    // iterate through the collection and convert any strin-object dictionaries
                    // along the way into expando objects
                    var itemList = new List<object>();
                    foreach (var item in (ICollection)kvp.Value) {
                        if (item is IDictionary<string, object>) {
                            var expandoItem = ((IDictionary<string, object>)item).ToExpando();
                            itemList.Add(expandoItem);
                        } else {
                            itemList.Add(item);
                        }
                    }

                    expandoDic.Add(kvp.Key, itemList);
                } else {
                    expandoDic.Add(kvp);
                }
            }

            return expando;
        }

    }
}
