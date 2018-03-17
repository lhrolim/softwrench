using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.test.Util;
using softWrench.sW4.Mapping;
using softWrench.sW4.Security.Services;

namespace softwrench.sW4.test.Security {

    [TestClass]
    public class UserProfileManagerTest {

        private Mock<ISWDBHibernateDAO> _dao = TestUtil.CreateMock<ISWDBHibernateDAO>();
        private Mock<IMappingResolver> _mapper = TestUtil.CreateMock<IMappingResolver>();

        private UserProfileManager _manager;

        [TestInitialize]
        public void InitClass() {
            _dao = TestUtil.CreateMock<ISWDBHibernateDAO>();
            _mapper = TestUtil.CreateMock<IMappingResolver>();
            _manager = new UserProfileManager(_dao.Object, _mapper.Object);
            TestUtil.ResetMocks(_dao, _mapper);
        }

        [TestMethod]
        public void TestMerge() {
            var profiles = new List<UserProfile>();
            var p1 = new UserProfile();
            var p2 = new UserProfile();
            p1.ApplicationPermissions = new HashSet<ApplicationPermission>();
            p1.ApplicationPermissions.Add(new ApplicationPermission(){ApplicationName = "workorder",AllowUpdate = true, AllowCreation = false});
            p1.ApplicationPermissions.Add(ApplicationPermission.BlockInstance("asset"));
            p2.ApplicationPermissions = new HashSet<ApplicationPermission>();
            p2.ApplicationPermissions.Add(ApplicationPermission.AllowInstance("asset"));
            p2.ApplicationPermissions.Add(ApplicationPermission.BlockInstance("workorder"));
            profiles.Add(p1);
            profiles.Add(p2);
            var merged = _manager.BuildMergedProfile(profiles);
            var permissions = merged.Permissions;
            Assert.AreEqual(2,permissions.Count);
            var assetPerm = permissions.FirstOrDefault(f=> f.ApplicationName == "asset");
            Assert.IsNotNull(assetPerm);
            Assert.IsTrue(assetPerm.AllowUpdate);
            Assert.IsTrue(assetPerm.AllowCreation);
            var woPermission = permissions.FirstOrDefault(f=> f.ApplicationName == "workorder");
            Assert.IsNotNull(woPermission);
            Assert.IsTrue(woPermission.AllowUpdate);
            Assert.IsFalse(woPermission.AllowCreation);

        }
    }
}
