using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.Mobile.Metadata.Parsing
{
    internal class JsonApplicationMetadataParser
    {
//        private static ApplicationSchemaDefinition ParseApplication(JObject json)
//        {
//            JsonConvert.DeserializeObject<ApplicationSchemaDefinition>(json, new MetadataConverter());
//
//            var originalMetadata = json.ToObject<ApplicationSchemaDefinition>();
//
//            return originalMetadata;
//
////            return new ApplicationSchemaDefinition(
////                Guid.Parse(json.Value<string>("id")),
////                json.Value<string>("name"),
////                json.Value<string>("title"),
////                json.Value<string>("entity"),
////                json.Value<string>("idFieldName"),
////                ParseSchema(json.Value<JObject>("schema")));
//        }
//
//        class MetadataConverter : CustomCreationConverter<ApplicationSchemaDefinition>
//        {
//            public override ApplicationSchemaDefinition Create(Type objectType)
//            {
//                throw new NotImplementedException();
//            }
//        }
//
//        private static MobileApplicationSchema ParseSchema(JObject mobile)
//        {
//            var isUserInteractionEnabled = mobile.Value<bool>("isUserInteractionEnabled");
//
//            MobileApplicationSchema.Preview title, subtitle, featured, excerpt;
//            title = subtitle = featured = excerpt = null;
//
//            var preview = mobile.Value<JObject>("previewTitle");
//            if (null != preview) title = ParsePreview(preview);
//
//            preview = mobile.Value<JObject>("previewSubtitle");
//            if (null != preview) subtitle = ParsePreview(preview);
//
//            preview = mobile.Value<JObject>("previewFeatured");
//            if (null != preview) featured = ParsePreview(preview);
//
//            preview = mobile.Value<JObject>("previewExcerpt");
//            if (null != preview) excerpt = ParsePreview(preview);
//
//            var fields = ParseFields(mobile.Property("fields").Values());
//            var compositions = ParseCompositions(mobile.Property("compositions").Values());
//
//            return new MobileApplicationSchema(
//                isUserInteractionEnabled,
//                fields,
//                compositions,
//                title,
//                subtitle,
//                featured,
//                excerpt);
//        }
//
//        private static MobileApplicationSchema.Preview ParsePreview(JToken json)
//        {
//            return new MobileApplicationSchema.Preview(
//                    json.Value<string>("label"),
//                    json.Value<string>("attribute"));
//        }
//
//        private static IEnumerable<ApplicationFieldDefinition> ParseFields(IEnumerable<JToken> json)
//        {
//            var tokens = json as IList<JToken> ?? json.ToList();
//
//            return tokens
//                .Cast<JObject>()
//                .Select(j => new ApplicationFieldDefinition(j.Value<string>("attribute"),
//                    j.Value<string>("label"),
//                    j.Value<bool>("isRequired"),
//                    j.Value<bool>("isReadOnly"),
//                    JsonWidgetParser.Parse(j.Value<JObject>("widget"))))
//                .ToList();
//        }
//
//        private static IEnumerable<ApplicationComposition> ParseCompositions(IEnumerable<JToken> json)
//        {
//            var tokens = json as IList<JToken> ?? json.ToList();
//
//            return tokens
//                .Cast<JObject>()
//                .Select(j => new ApplicationComposition(
//                    j.Value<string>("from"),
//                    ParseEntityAssociation(j.Value<JObject>("entityAssociation"))))
//                .ToList();
//        }
//
//        private static EntityAssociation ParseEntityAssociation(JToken json)
//        {
//            return new EntityAssociation(
//                json.Value<string>("to"),
//                ParseEntityAssociationAttributes(json.Value<JArray>("attributes")));
//        }
//
//        private static IEnumerable<EntityAssociationAttribute> ParseEntityAssociationAttributes(IEnumerable<JToken> json)
//        {
//            var tokens = json as IList<JToken> ?? json.ToList();
//
//            return tokens
//                .Cast<JObject>()
//                .Select(j => new EntityAssociationAttribute(
//                    j.Value<string>("to"),
//                    j.Value<string>("from"),
//                    j.Value<string>("literal")))
//                .ToList();
//        }
//
        public string ToJson(CompleteApplicationMetadataDefinition ApplicationSchemaDefinition)
        {
            return JsonConvert.SerializeObject(ApplicationSchemaDefinition, JsonParser.SerializerSettings());
        }
//
//        public ApplicationSchemaDefinition FromJson(JObject json)
//        {
//            return ParseApplication(json);
//        }
//
//        private static class JsonWidgetParser
//        {
//            private static LookupWidgetDefinition.Filter ParseLookupFilter(JToken filter)
//            {
//                var sourceField = filter.Value<string>("sourceField");
//                var targetField = filter.Value<string>("targetField");
//                var literal = filter.Value<string>("literal");
//
//                return new LookupWidgetDefinition.Filter(sourceField, targetField, literal);
//            }
//
//            private static IEnumerable<LookupWidgetDefinition.Filter> ParseLookupFilters(IEnumerable<JToken> filters)
//            {
//                return filters
//                    .Select(ParseLookupFilter)
//                    .ToList();
//            }
//
//            private static LookupWidget ParseLookup(JObject json)
//            {
//                var sourceApplication = json.Value<string>("sourceApplication");
//                var sourceField = json.Value<string>("sourceField");
//                var targetField = json.Value<string>("targetField");
//                var targetQualifier = json.Value<string>("targetQualifier");
//                var filters = ParseLookupFilters(json.Value<JArray>("filters"));
//
//                var sourceDisplay = json
//                    .Property("sourceDisplay")
//                    .Values()
//                    .Select(v => v.Value<string>())
//                    .ToList();
//
//                return new LookupWidget(sourceApplication, sourceField, sourceDisplay, targetField, targetQualifier, filters);
//            }
//
//            private static DateWidget ParseDate(JObject json)
//            {
//                var format = json.Value<string>("format");
//                var time = json.Value<bool>("time");
//                var min = json.Value<DateTime>("min");
//                var max = json.Value<DateTime>("max");
//
//                return new DateWidget(format, time, min, max);
//            }
//
//            private static NumberWidget ParseNumber(JObject json)
//            {
//                // This is a work around to avoid overflow when deserializing huge
//                // decimals. Check http://json.codeplex.com/workitem/23341
//                var readAsDecimal = new Func<string, decimal?>(s =>
//                                                              {
//                                                                  if (null == s)
//                                                                  {
//                                                                      return null;
//                                                                  }
//
//                                                                  using (var reader = new JsonTextReader(new StringReader(s)))
//                                                                  {
//                                                                      return reader.ReadAsDecimal().Value;
//                                                                  }
//                                                              });
//                
//                var decimals = json.Value<int>("decimals");
//                var min = readAsDecimal(json.Value<string>("min"));
//                var max = readAsDecimal(json.Value<string>("max"));
//
//                return new NumberWidget(decimals, min, max);
//            }
//
//            private static HiddenWidget ParseHidden()
//            {
//                return new HiddenWidget();
//            }
//
//            private static TextWidget ParseText()
//            {
//                return new TextWidget();
//            }
//
//            public static IWidget Parse(JObject json)
//            {
//                var type = json.Value<string>("type");
//
//                if (type == typeof(TextWidget).Name)
//                {
//                    return ParseText();
//                }
//
//                if (type == typeof(HiddenWidget).Name)
//                {
//                    return ParseHidden();
//                }
//
//                if (type == typeof(NumberWidget).Name)
//                {
//                    return ParseNumber(json);
//                }
//
//                if (type == typeof(DateWidget).Name)
//                {
//                    return ParseDate(json);
//                }
//
//                if (type == typeof(LookupWidget).Name)
//                {
//                    return ParseLookup(json);
//                }
//
//                throw new ArgumentOutOfRangeException();
//            }
//        }
    }
}