using System;
using System.Collections.Generic;
using cts.commons.persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.test.Util;
using softWrench.sW4.Mapping;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Security.Services;

namespace softwrench.sW4.test.Security {
    [TestClass]
    public class ApplicationMetadataPolicyApplierTest : BaseOtbMetadataTest {

        private UserProfileManager _manager;
        private Mock<ISWDBHibernateDAO> _dao = TestUtil.CreateMock<ISWDBHibernateDAO>();
        private Mock<IMappingResolver> _mapper = TestUtil.CreateMock<IMappingResolver>();

        [TestInitialize]
        public void InitClass() {
            _dao = TestUtil.CreateMock<ISWDBHibernateDAO>();
            _mapper = TestUtil.CreateMock<IMappingResolver>();
            _manager = new UserProfileManager(_dao.Object, _mapper.Object);
            TestUtil.ResetMocks(_dao, _mapper);
        }

        [TestMethod]
        public void TestReadonlyIssue()
        {
            var schemas = MetadataProvider.Application("_workpackage").Schemas();
            var baseSchema = schemas[new ApplicationMetadataSchemaKey("adetail")];
            var p1 = new UserProfile();
            p1.ApplicationPermissions = new HashSet<ApplicationPermission>();
            p1.ApplicationPermissions.Add(new ApplicationPermission() { ApplicationName = "_workpackage", AllowUpdate = false, AllowCreation = false,AllowRemoval = false, AllowView = true});
            var profiles = new List<UserProfile>();
            profiles.Add(p1);

            var merged = _manager.BuildMergedProfile(profiles);


            var originalComp = baseSchema.LocateFieldByKey("dailyOutageMeetings_") as ApplicationCompositionDefinition;
            Assert.IsNotNull(originalComp);
            
            var collSchema = originalComp.Schema as ApplicationCompositionCollectionSchema;
            Assert.IsNotNull(collSchema);
            Assert.AreEqual(collSchema.AllowInsertion, "true");
            Assert.AreEqual(collSchema.AllowUpdate, "true");


            var securedSchema = baseSchema.ApplyPolicy(new List<Role>(), ClientPlatform.Web, null, merged);
            var comp = securedSchema.LocateFieldByKey("dailyOutageMeetings_") as ApplicationCompositionDefinition;

            Assert.AreNotSame(comp, originalComp);
            Assert.AreNotSame(comp.Schema, originalComp.Schema);

            Assert.IsNotNull(comp);
            collSchema = comp.Schema as ApplicationCompositionCollectionSchema;
            Assert.IsNotNull(collSchema);
            Assert.AreEqual(collSchema.AllowInsertion, "false");
            Assert.AreEqual(collSchema.AllowUpdate, "false");


            





        }


        public override string GetClientName() {
            return "firstsolar";
        }

    }
}
