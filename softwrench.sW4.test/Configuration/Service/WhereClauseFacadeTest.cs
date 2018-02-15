using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.test.Util;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.test.Configuration.Service {
    [TestClass]
    public class WhereClauseFacadeTest {

        private IWhereClauseFacade _facade;

        private Mock<ConfigurationService> _service;
        private Mock<ConfigurationCache> _cache;

        [TestInitialize]
        public void Init() {
            _service = TestUtil.CreateMock<ConfigurationService>();
            _cache = TestUtil.CreateMock<ConfigurationCache>();
            TestUtil.ResetMocks(_service, _cache);
            _facade = new WhereClauseFacade(_service.Object, null, null,_cache.Object);
        }


        /// <summary>
        /// Ensures that if there´s more than one profile matching the app, bu both pointing to the same whereclause, just the default value is returned
        /// </summary>
        [TestMethod]
        public async Task Two_Profiles_with_sameClause_disregard() {
            var user = InMemoryUser.TestInstance();
            var up = new UserProfile {
                Name = "new",
                ApplicationPermissions =
                    new HashSet<ApplicationPermission> { ApplicationPermission.AllowInstance("assignment") }
            };

            var up2 = new UserProfile {
                Name = "offline",
                ApplicationPermissions =
                    new HashSet<ApplicationPermission> { ApplicationPermission.AllowInstance("assignment") }
            };

            user.Profiles.Add(up);
            user.Profiles.Add(up2);

            _service.Setup(s => s.Lookup<string>(It.IsAny<string>(), It.IsAny<ContextHolder>(), null)).ReturnsAsync("1=1");

            var result = await _facade.ProfilesByApplication("assignment", user);
            Assert.AreEqual(0, result.Count);

            TestUtil.VerifyMocks(_service);
        }


        /// <summary>
        /// There´s now 3 sgs. two have exactly the same whereclause while the second one has a different.
        /// there should be 2 different profiles on screen to choose from
        /// </summary>
        //TODO: combine the names of the profiles that point to the same whereclause
        [TestMethod]
        public async Task three_Profiles_2_Equal_one_different() {
            var user = InMemoryUser.TestInstance();
            var up = new UserProfile {
                Name = "new",
                Id = 1,
                ApplicationPermissions =
                    new HashSet<ApplicationPermission> { ApplicationPermission.AllowInstance("assignment") }
            };

            var up2 = new UserProfile {
                Name = "offline",
                Id = 1,
                ApplicationPermissions =
                    new HashSet<ApplicationPermission> { ApplicationPermission.AllowInstance("assignment") }
            };

            var up3 = new UserProfile {
                Name = "offline2",
                Id = 3,
                ApplicationPermissions =
                    new HashSet<ApplicationPermission> { ApplicationPermission.AllowInstance("assignment") }
            };

            user.Profiles.Add(up);
            user.Profiles.Add(up2);
            user.Profiles.Add(up3);


            var expression = (Expression<Func<ContextHolder, bool>>)(h => h.CurrentSelectedProfile == 3);
            var expression2 = (Expression<Func<ContextHolder, bool>>)(h => h.CurrentSelectedProfile == 1);


            _service.Setup(s => s.Lookup<string>(It.IsAny<string>(), It.Is(expression), null)).ReturnsAsync("1=1");
            _service.Setup(s => s.Lookup<string>(It.IsAny<string>(), It.Is(expression2), null)).ReturnsAsync("1!=1");
            
//            _service.Setup(s => s.Lookup<string>(It.IsAny<string>(), It.IsAny<ContextHolder>(), null)).ReturnsAsync("1=1");

            var result = await _facade.ProfilesByApplication("assignment", user);

            TestUtil.VerifyMocks(_service);

            Assert.AreEqual(2, result.Count);

            
        }
    }
}
