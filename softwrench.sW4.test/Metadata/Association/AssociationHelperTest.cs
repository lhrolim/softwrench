﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using System.Linq;

namespace softwrench.sW4.test.Metadata.Association {
    [TestClass]
    public class AssociationHelperTest {
        private readonly AssociationUpdateRequest _blankRequest = new AssociationUpdateRequest();


        [TestMethod]
        public void NoneRequestOverriding() {
            var result = AssociationHelper.BuildAssociationsToPrefetch(new DetailRequest { AssociationsToFetch = "#none" }, WithProperty("#all")).ToFetchList;
            Assert.AreEqual("#none", result[0]);
        }

        [TestMethod]
        public void BothNulls() {
            var result = AssociationHelper.BuildAssociationsToPrefetch(_blankRequest, new ApplicationSchemaDefinition()).ToFetchList;
            Assert.AreEqual("#none", result[0]);
        }

        [TestMethod]
        public void RequestNullReturnApp() {
            var result = AssociationHelper.BuildAssociationsToPrefetch(_blankRequest, WithProperty("#all")).ToFetchList;
            Assert.AreEqual("#all", result.First());
            result = AssociationHelper.BuildAssociationsToPrefetch(_blankRequest, WithProperty("fromlocation,xxx")).ToFetchList;
            Assert.AreEqual("fromlocation", result[0]);
            Assert.AreEqual("xxx", result[1]);
        }

        [TestMethod]
        public void TestMerge() {
            var result = AssociationHelper.BuildAssociationsToPrefetch(new DetailRequest { AssociationsToFetch = "x" }, WithProperty("y")).ToFetchList;
            Assert.AreEqual("y", result[0]);
            Assert.AreEqual("x", result[1]);
        }


        [TestMethod]
        public void TestAllButSchema() {
            var result = AssociationHelper.BuildAssociationsToPrefetch(new DetailRequest { AssociationsToFetch = AssociationHelper.AllButSchema }, WithProperty("y"));
            Assert.IsTrue(result.ShouldResolve("x"));
            Assert.IsTrue(result.ShouldResolve("z"));
            Assert.IsFalse(result.ShouldResolve("y"));
        }


        [TestMethod]
        public void TestAllButSchema2() {
            var result = AssociationHelper.BuildAssociationsToPrefetch(new DetailRequest { AssociationsToFetch = AssociationHelper.AllButSchema }, new ApplicationSchemaDefinition());
            Assert.IsTrue(result.ShouldResolve("x"));
            Assert.IsTrue(result.ShouldResolve("z"));
            Assert.IsTrue(result.ShouldResolve("y"));
        }



        private static ApplicationSchemaDefinition WithProperty(string value) {
            return new ApplicationSchemaDefinition() { Properties = { { ApplicationSchemaPropertiesCatalog.PreFetchAssociations, value } } };
        }
    }
}
