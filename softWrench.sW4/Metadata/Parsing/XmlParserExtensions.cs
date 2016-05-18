using System;
using System.Linq;
using System.Xml.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Metadata.Parsing {
    internal static class XmlParserExtensions {
        public delegate bool AttributeValueConverter<T>(string input, out T output);

        public static T ValueOrDefault<T>([CanBeNull] this XAttribute attribute, T @default, [NotNull] AttributeValueConverter<T> converter) {
            if (converter == null) throw new ArgumentNullException("converter");

            // If the attribute is missing,
            // we will use the default.
            if (null == attribute) {
                return @default;
            }

            T value;

            // If we can't parse the attribute to the
            // target type, let's return the default.
            return (converter(attribute.Value, out value))
                ? value
                : @default;
        }

        public static string ElementValue([NotNull] this XElement rootElement, string childElementName) {
            var element = rootElement.Elements().FirstOrDefault(f => f.Name.LocalName.EqualsIc(childElementName));
            return element == null ? null : element.Value;
        }

        public static bool ValueOrDefault([CanBeNull] this XAttribute attribute, bool @default) {
            AttributeValueConverter<bool> converter = (string input, out bool output) => bool.TryParse(input, out output);
            return ValueOrDefault(attribute, @default, converter);
        }

        public static bool? ValueOrDefault([CanBeNull] this XAttribute attribute, bool? @default) {
            var result = attribute.ValueOrDefault((string)null);
            if (result != null) {
                return result.Equals("true", StringComparison.CurrentCultureIgnoreCase);
            }
            return null;
        }


        public static DateTime ValueOrDefault([CanBeNull] this XAttribute attribute, DateTime @default) {
            AttributeValueConverter<DateTime> converter = (string input, out DateTime output) => DateTime.TryParse(input, out output);
            return ValueOrDefault(attribute, @default, converter);
        }

        public static decimal ValueOrDefault([CanBeNull] this XAttribute attribute, decimal @default) {
            AttributeValueConverter<decimal> converter = (string input, out decimal output) => decimal.TryParse(input, out output);
            return ValueOrDefault(attribute, @default, converter);
        }

        public static decimal? ValueOrDefault([CanBeNull] this XAttribute attribute, decimal? @default) {
            AttributeValueConverter<decimal?> converter = (string input, out decimal? output) => {
                decimal outputAsDecimal;
                if (decimal.TryParse(input, out outputAsDecimal)) {
                    output = outputAsDecimal;
                    return true;
                }

                output = null;
                return false;
            };

            return ValueOrDefault(attribute, @default, converter);
        }

        public static int ValueOrDefault([CanBeNull] this XAttribute attribute, int @default) {
            AttributeValueConverter<int> converter = (string input, out int output) => int.TryParse(input, out output);
            return ValueOrDefault(attribute, @default, converter);
        }

        public static string ValueOrDefault([CanBeNull] this XAttribute attribute, string @default) {
            AttributeValueConverter<string> converter = (string input, out string output) => {
                output = input;
                return true;
            };

            return ValueOrDefault(attribute, @default, converter);
        }

        //        public static object ValueOrNull([CanBeNull] this XAttribute attribute) {
        //            AttributeValueConverter<object> converter = (string input, out object output) =>
        //            {
        //                output = input;
        //                return true;
        //            };
        //
        //            if (converter == null) throw new ArgumentNullException("converter");
        //
        //            // If the attribute is missing,
        //            // we will use the default.
        //            if (null == attribute) {
        //                return null;
        //            }
        //
        //            object value;
        //
        //            // If we can't parse the attribute to the
        //            // target type, let's return the default.
        //            return (converter(attribute.Value, out value))
        //                ? value
        //                : null;
        //        }


    }
}