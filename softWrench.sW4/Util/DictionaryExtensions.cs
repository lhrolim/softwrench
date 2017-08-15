using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using cts.commons.portable.Util;
using cts.commons.Util;

namespace softWrench.sW4.Util {
    public static class DictionaryExtensions {


        public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other) {
            if (other == null) {
                return dictionary;
            }
            foreach (var entry in other) {
                if (!dictionary.ContainsKey(entry.Key)) {
                    dictionary.Add(entry);
                }
            }
            return dictionary;
        }

        public static void AddE<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key, TValue value, string contextInfo = null) {
            try {
                map.Add(key, value);
            } catch (Exception e) {
                if (contextInfo == null) {
                    contextInfo = "";
                }
                //                LoggingUtil.DefaultLog.Error("key {0} already exists at dict".Fmt(key), e);
                throw new InvalidOperationException("key {0} already exists at dict {1}".Fmt(key, contextInfo), e);
            }

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
