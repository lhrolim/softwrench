using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Validator;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Metadata.Parsing {
    class XmlTemplateHandler {

        private static readonly ILog Log = LogManager.GetLogger(typeof(XmlTemplateHandler));

        private readonly bool _isSWDB;

        public XmlTemplateHandler(bool isSWDB) {
            _isSWDB = isSWDB;
        }

        private List<TR> DoHandleTemplates<TR, T>(XContainer templates, ISet<string> alreadyParsedTemplates, IXmlMetadataParser<T> parser) {
            if (alreadyParsedTemplates == null) {
                alreadyParsedTemplates = new HashSet<string>();
            }
            var result = new List<TR>();
            if (templates == null) {
                return result;
            }
            foreach (var template in templates.Elements().Where(e => e.IsNamed(XmlMetadataSchema.TemplateElement))) {
                var realPath = RealPath(template);
                if (alreadyParsedTemplates.Contains(realPath.Item2)) {
                    Log.Debug("template {0} skipped because it was already added".Fmt(realPath));
                    continue;
                }
                Log.Debug("parsing template {0}".Fmt(realPath));
                using (var stream = MetadataParsingUtils.DoGetStreamForTemplate(realPath.Item1, realPath.Item2)) {
                    if (stream != null) {
                        var parsingResult = parser.Parse(stream, alreadyParsedTemplates);
                        if (parsingResult is IEnumerable<TR>) {
                            result = new List<TR>(MetadataMerger.Merge<TR>(result, (IEnumerable<TR>)parsingResult));
                        } else {
                            var istuple = parsingResult as Tuple<IEnumerable<TR>, EntityQueries>;
                            if (istuple != null) {
                                result.AddRange(istuple.Item1);
                            }
                        }
                        alreadyParsedTemplates.Add(realPath.Item1);
                    } else {
                        throw new InvalidOperationException("Template {0} not found".Fmt(realPath));
                    }
                }
            }
            return result;
        }


        [NotNull]
        public List<EntityMetadata> HandleTemplatesForEntities(XContainer templates, bool isSWDB, ISet<string> alreadyParsedTemplates) {
            return DoHandleTemplates<EntityMetadata, Tuple<IEnumerable<EntityMetadata>, EntityQueries>>(templates, alreadyParsedTemplates,
                new XmlEntitySourceMetadataParser(isSWDB, true));
        }

        [NotNull]
        public List<CompleteApplicationMetadataDefinition> HandleTemplatesForApplications(XContainer templates,
            [NotNull] IEnumerable<EntityMetadata> entityMetadata, IDictionary<string, CommandBarDefinition> commandBars, Boolean isSWDB, ISet<string> alreadyParsedTemplates) {
            return DoHandleTemplates<CompleteApplicationMetadataDefinition, IEnumerable<CompleteApplicationMetadataDefinition>>(templates, alreadyParsedTemplates,
             new XmlApplicationMetadataParser(entityMetadata, commandBars, isSWDB, true));
        }

        private Tuple<String, string> RealPath(XElement template) {
            var path = template.Attribute(XmlMetadataSchema.TemplatePathAttribute).Value;
            if (!path.EndsWith(".xml")) {
                path = path + ".xml";
            }
            return new Tuple<string, string>(path, MetadataParsingUtils.GetTemplateInternalPath(path, _isSWDB));

        }



    }
}
