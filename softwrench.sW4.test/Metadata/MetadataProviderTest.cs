using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata {
    [TestClass]
    public class MetadataProviderTest : BaseOtbMetadataTest {



        [TestMethod]
        public void NumberOfSuggetedCompositionsConsiderProperty() {
            var srlist = MetadataProvider.Application("servicerequest").Schema(new ApplicationMetadataSchemaKey("list", SchemaMode.None, ClientPlatform.Web));
            var compositions = MetadataProvider.BuildRelatedCompositionsList(srlist);
            var associationOptions = compositions as IList<AssociationOption> ?? compositions.ToList();
            Assert.AreEqual(2, associationOptions.Count());
            Assert.IsTrue(associationOptions.Any(c => c.Value.Equals("worklog_")));
            Assert.IsTrue(associationOptions.Any(c => c.Value.Equals("commlog_")));
        }

        [TestMethod]
        public void NumberOfSuggetedCompositionsExcludeBlankCompositions() {
            var srlist = MetadataProvider.Application("servicerequest").Schema(new ApplicationMetadataSchemaKey("list2", SchemaMode.None, ClientPlatform.Web));
            var compositions = MetadataProvider.BuildRelatedCompositionsList(srlist);
            var associationOptions = compositions as IList<AssociationOption> ?? compositions.ToList();
            //related records should be disregarded, cause it has no fields
            Assert.AreEqual(2, associationOptions.Count());
            Assert.IsTrue(associationOptions.Any(c => c.Value.Equals("worklog_")));
            Assert.IsTrue(associationOptions.Any(c => c.Value.Equals("commlog_")));
        }

        public override string GetClientName() {
            return "test4";
        }
    }
}
