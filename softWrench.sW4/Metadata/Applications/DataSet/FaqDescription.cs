using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class FaqDescription {
        private readonly string _id;
        private readonly string _language;
        private readonly IList<string> _categories;
        private readonly string _realDescription;


        public FaqDescription(string dbDescription) {
            var lidAndRestArray = dbDescription.Split('_');
            var lanIdDescription = lidAndRestArray[0];
            _language = lanIdDescription.Substring(0, 1);
            _id = lanIdDescription.Substring(1, 4);
            var categoryDescArray = lidAndRestArray[1].Split('|');
            _realDescription = categoryDescArray[1];
            _categories = new List<string>(categoryDescArray[0].Split('/'));
        }

        public string Id {
            get { return _id; }
        }

        public string RealDescription {
            get { return _realDescription; }
        }

        public string Language {
            get { return _language; }
        }

        public IList<string> Categories {
            get { return _categories; }
        }

        public bool HasSchemaId(string schemaId) {
            var has = false;

            switch (schemaId) {
                case "sd":
                    if (_categories.Any(category => category.Contains("SWD"))) {
                        has = true;
                    }
                    break;
                case "printer":
                    if (_categories.Any(category => category.Contains("Printer"))) {
                        has = true;
                    }
                    break;
                case "outlook":
                    if (_categories.Any(category => category.Contains("Outlook"))) {
                        has = true;
                    }
                    break;
                case "phone":
                    if (_categories.Any(category => category.Contains("Phone"))) {
                        has = true;
                    }
                    break;
            }
            return has;
        }

        public bool IsValid() {
            if (_language != "E" && _language != "G" && _language != "S") {
                return false;
            }
            int idAux;
            if (!int.TryParse(_id, out idAux)) {
                return false;
            }
            if (string.IsNullOrEmpty(_realDescription)) {
                return false;
            }
            if (_categories.Count <= 0) {
                return false;
            }
            return true;
        }
    }
}
