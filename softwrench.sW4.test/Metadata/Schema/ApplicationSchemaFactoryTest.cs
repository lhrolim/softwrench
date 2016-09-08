using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.Schema;

namespace softwrench.sW4.test.Metadata.Schema {
    [TestClass]
    public class ApplicationSchemaFactoryTest : BaseOtbMetadataTest {

        private ApplicationSchemaDefinition _schema;

        [TestInitialize]
        public override void Init() {
            base.Init();
            _schema = MetadataProvider.Application("servicerequest").Schema(new ApplicationMetadataSchemaKey("list"));
        }

        [TestMethod]
        public void TestSecureCreation() {
            var profile = new MergedUserProfile();
            profile.Permissions = new List<ApplicationPermission>()
            {
                new ApplicationPermission()
                {
                    ApplicationName = "servicerequest",
                    AllowCreation = false
                }
            };

            Assert.IsNotNull(_schema.NewSchemaRepresentation);

            //if there´s a such allowcreation=false restriction, disable it
            var securedSchema = _schema.ApplyPolicy(new List<Role>(), ClientPlatform.Web, null, profile);
            Assert.IsNull(securedSchema.NewSchemaRepresentation);

            //if there´s no such allowcreation=false restriction, allow it
            securedSchema = _schema.ApplyPolicy(new List<Role>(), ClientPlatform.Web, null, new MergedUserProfile());
            Assert.IsNotNull(securedSchema.NewSchemaRepresentation);
        }
    }
}
