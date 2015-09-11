using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata.Menu.Containers;
using softWrench.sW4.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sw4.Shared2.Metadata.Modules;

namespace softWrench.sW4.Metadata.Menu {

    class XmlTemplateMenuMetadataParser  {

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public MenuTemplateCatalog Parse([NotNull] TextReader stream) {
          if (stream == null) throw new ArgumentNullException("stream");

            var document = XDocument.Load(stream);
            var menuElement = document.Root;
            if (null == menuElement) throw new InvalidDataException();

            var leafs = XmlMenuParserCommons.BuildLeafs(menuElement, null);
            var menuDefinition = new MenuTemplateCatalog(leafs);
            return menuDefinition;
        }


       


    }
}
