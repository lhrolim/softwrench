namespace softWrench.sW4.Web.Controllers.Utilities {
    public class EntityMetadataEditorResult {
        private readonly string _content;
        private readonly string _type;

        public EntityMetadataEditorResult(string content, string type) {
            this._content = content;
            this._type = type;
        }

        public string Content {
            get { return _content; }
        }

        public string Type {
            get { return _type; }
        }
    }
}