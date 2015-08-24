using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace softWrench.sW4.Metadata.Parsing {
    public interface IXmlMetadataParser<out T> {

        T Parse([NotNull] TextReader stream,ISet<string> alreadyParsedTemplates = null);
    }
}