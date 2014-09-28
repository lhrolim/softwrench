using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softWrench.Mobile.Metadata.Applications.Schema {
    public class MobileApplicationSchema :ApplicationSchemaDefinition{
        private readonly bool _isUserInteractionEnabled;

        private readonly Preview _previewTitle;
        private readonly Preview _previewSubtitle;
        private readonly Preview _previewFeatured;
        private readonly Preview _previewExcerpt;


        public MobileApplicationSchema(

            bool isUserInteractionEnabled,
            IEnumerable<IApplicationDisplayable> fields,
            Preview previewTitle, Preview previewSubtitle, Preview previewFeatured, Preview previewExcerpt) {

            _isUserInteractionEnabled = isUserInteractionEnabled;
            _fields = DisplayableUtil.GetDisplayable<ApplicationFieldDefinition>(typeof(ApplicationFieldDefinition), fields);
            Compositions = DisplayableUtil.GetDisplayable<ApplicationCompositionDefinition>(typeof(ApplicationCompositionDefinition), fields);
            _previewTitle = previewTitle;
            _previewSubtitle = previewSubtitle;
            _previewFeatured = previewFeatured;
            _previewExcerpt = previewExcerpt;
        }

        public bool IsUserInteractionEnabled {
            get { return _isUserInteractionEnabled; }
        }


        public Preview PreviewTitle {
            get { return _previewTitle; }
        }

        public Preview PreviewSubtitle {
            get { return _previewSubtitle; }
        }

        public Preview PreviewFeatured {
            get { return _previewFeatured; }
        }

        public Preview PreviewExcerpt {
            get { return _previewExcerpt; }
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