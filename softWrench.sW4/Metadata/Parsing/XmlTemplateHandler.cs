using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Validator;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Parsing {
    class XmlTemplateHandler : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(XmlTemplateHandler));

        private static List<R> DoHandleTemplates<R, T>(XContainer templates, bool isSWDB, ISet<string> alreadyParsedTemplates, IXmlMetadataParser<T> parser) {
            if (alreadyParsedTemplates == null) {
                alreadyParsedTemplates = new HashSet<string>();
            }
            var result = new List<R>();
            if (templates == null) {
                return result;
            }
            foreach (var template in templates.Elements().Where(e => e.IsNamed(XmlMetadataSchema.TemplateElement))) {
                var realPath = RealPath(template);
                if (alreadyParsedTemplates.Contains(realPath)) {
                    Log.Info("template {0} skipped because it was already added".Fmt(realPath));
                    continue;
                }
                Log.Info("parsing template {0}".Fmt(realPath));
                using (var stream = MetadataParsingUtils.DoGetStream(realPath)) {
                    if (stream != null) {
                        var parsingResult = parser.Parse(stream, alreadyParsedTemplates);
                        if (parsingResult is IEnumerable<R>) {
                            result.AddRange((IEnumerable<R>)parsingResult);
                        } else {
                            var Istuple = parsingResult as Tuple<IEnumerable<R>, EntityQueries>;
                            if (Istuple != null) {
                                result.AddRange(Istuple.Item1);
                            }
                        }
                        alreadyParsedTemplates.Add(realPath);
                    } else {
                        throw new InvalidOperationException("Template {0} not found".Fmt(realPath));
                    }
                }
            }
            return result;
        }


        [NotNull]
        public static List<EntityMetadata> HandleTemplatesForEntities(XContainer templates, bool isSWDB, ISet<string> alreadyParsedTemplates) {
            return DoHandleTemplates<EntityMetadata, Tuple<IEnumerable<EntityMetadata>, EntityQueries>>(templates, isSWDB, alreadyParsedTemplates,
                new XmlEntitySourceMetadataParser(isSWDB));
        }

        [NotNull]
        public static List<CompleteApplicationMetadataDefinition> HandleTemplatesForApplications(XContainer templates,
            [NotNull] IEnumerable<EntityMetadata> entityMetadata, IDictionary<string, CommandBarDefinition> commandBars, Boolean isSWDB, ISet<string> alreadyParsedTemplates) {
            return DoHandleTemplates<CompleteApplicationMetadataDefinition, IEnumerable<CompleteApplicationMetadataDefinition>>(templates, isSWDB, alreadyParsedTemplates,
             new XmlApplicationMetadataParser(entityMetadata, commandBars, isSWDB));
        }

        private static string RealPath(XElement template) {
            var path = template.Attribute(XmlMetadataSchema.TemplatePathAttribute).Value;
            if (!path.EndsWith(".xml")) {
                path = path + ".xml";
            }
            return MetadataParsingUtils.GetTemplateInternalPath(path);

        }



    }
}
