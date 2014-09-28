using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Metadata.Extensions {
    internal static class ApplicationSchemaDefinitionExtensions {

        internal const String PreviewTitleConst = ApplicationMetadataConstants.PreviewTitle;
        internal const String PreviewSubTitleConst = ApplicationMetadataConstants.PreviewSubTitle;
        internal const String PreviewFeaturedConst = ApplicationMetadataConstants.PreviewFeatured;
        internal const String PreviewExcerptConst = ApplicationMetadataConstants.PreviewExcerpt;

        /// <summary>
        /// gets or sets this value, depending if the value parameter is set
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsUserInteractionEnabled(this ApplicationSchemaDefinition definition, Boolean? value = null) {
            if (value != null) {
                definition.ExtensionParameter(ApplicationMetadataConstants.IsUserInteractionEnabledProperty, value);
            }
            return (bool)definition.ExtensionParameter(ApplicationMetadataConstants.IsUserInteractionEnabledProperty);
            //            object userInteractionEnabled;
            //            definition.parameters.TryGetValue(ApplicationMetadataConstants.IsUserInteractionEnabledProperty, out userInteractionEnabled);
            //            var isInteractionEnabled = userInteractionEnabled != null && bool.Parse(userInteractionEnabled.ToString());
        }


        public static Preview PreviewTitle(this ApplicationSchemaDefinition definition, Preview value = null) {
            return SetOrRetrieve(definition, value, PreviewTitleConst);
        }

        public static Preview PreviewSubtitle(this ApplicationSchemaDefinition definition, Preview value = null) {
            return SetOrRetrieve(definition, value, PreviewSubTitleConst);
        }

        public static Preview PreviewFeatured(this ApplicationSchemaDefinition definition, Preview value = null) {
            return SetOrRetrieve(definition, value, PreviewFeaturedConst);
        }

        public static Preview PreviewExcerpt(this ApplicationSchemaDefinition definition, Preview value = null) {
            return SetOrRetrieve(definition, value, PreviewExcerptConst);
        }

        private static Preview SetOrRetrieve(ApplicationSchemaDefinition definition, Preview value, string constValue) {
            if (value != null) {
                definition.ExtensionParameter(constValue, value);
            }
            return (Preview)definition.ExtensionParameter(constValue);
        }




        public class Preview {
            private readonly string _label;
            private readonly string _attribute;

            public Preview(string label, string attribute) {
                if (label == null) throw new ArgumentNullException("label");
                if (attribute == null) throw new ArgumentNullException("attribute");

                _label = label;
                _attribute = attribute;
            }

            public string Label {
                get { return _label; }
            }

            public string Attribute {
                get { return _attribute; }
            }
        }

    }
}
