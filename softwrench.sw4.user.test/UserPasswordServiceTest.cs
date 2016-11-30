using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.api.classes.configuration;
using softwrench.sw4.user.classes.config;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.exceptions;
using softwrench.sw4.user.classes.services;
using softwrench.sW4.test.Util;

namespace softwrench.sw4.user.test {
    [TestClass]
    public class UserPasswordServiceTest {

        private UserPasswordService _service;

        private readonly Mock<IConfigurationFacadeCommons> _facade = TestUtil.CreateMock<IConfigurationFacadeCommons>();
        private readonly Mock<ISWDBHibernateDAO> _dao = TestUtil.CreateMock<ISWDBHibernateDAO>();

        [TestInitialize]
        public void Init() {
            TestUtil.ResetMocks(_facade, _dao);
            _service = new UserPasswordService(_facade.Object, _dao.Object);
        }

        [TestMethod]
        public async Task TestPasswordHistoryEntryFound() {
            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10 };
            var list = new List<PasswordHistory>{
                new PasswordHistory {Id = 1, Password = "criptedfake", RegisterTime = DateTime.Now}
            };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.MinPasswordHistorySize)).ReturnsAsync(2);
            _dao.Setup(d => d.FindByQueryWithLimitAsync<PasswordHistory>(PasswordHistory.ByUserDesc, 2, 10))
                .ReturnsAsync(list);

            try {
                await _service.HandlePasswordHistory(user, "criptedfake");
                //exception is expected
                Assert.IsFalse(true);
            } catch (PasswordException) {

            }
            TestUtil.VerifyMocks(_facade, _dao);

        }

        [TestMethod]
        public async Task TestPasswordHistoryNoEntryFound()
        {
            var now = DateTime.Now;
            var futureDate = now.AddDays(10);

            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10, PasswordExpirationTime = now, ChangePassword = true};
            var list = new List<PasswordHistory>{
                new PasswordHistory {Id = 1, Password = "criptedfake", RegisterTime = DateTime.Now}
            };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.MinPasswordHistorySize)).ReturnsAsync(2);
            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.PasswordExpirationTime)).ReturnsAsync(10);

            _dao.Setup(d => d.FindByQueryWithLimitAsync<PasswordHistory>(PasswordHistory.ByUserDesc, 2, 10))
                .ReturnsAsync(list);

            _dao.Setup(d => d.SaveAsync(It.Is((Expression<Func<PasswordHistory, bool>>)(p => p.RegisterTime != null && p.Password == "newcripted" && p.UserId == 10)))).ReturnsAsync(new PasswordHistory());

            _dao.Setup(d => d.SaveAsync(It.Is((Expression<Func<User, bool>>)(u => u.PasswordExpirationTime.Value.Day ==futureDate.Day && u.Id==10 && !u.ChangePassword.Value)))).ReturnsAsync(new User());


            await _service.HandlePasswordHistory(user, "newcripted");
            //exception is expected


            TestUtil.VerifyMocks(_facade, _dao);

        }


        [TestMethod]
        public async Task TestPasswordBeforeLocking() {
            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10 };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.WrongPasswordAttempts)).ReturnsAsync(5);

            //asserting that the entry has been saved
            _dao.Setup(d => d.SaveAsync(It.Is((Expression<Func<AuthenticationAttempt, bool>>)(a => a.IsValid() && a.NumberOfAttempts == 1 && a.GlobalNumberOfAttempts == 1)))).ReturnsAsync(user.AuthenticationAttempts);

            var result = await _service.MatchPassword(user, "wrong");
            Assert.IsFalse(result);
            Assert.IsFalse(user.Locked.HasValue);
            Assert.IsNotNull(user.AuthenticationAttempts);
            Assert.IsNotNull(user.AuthenticationAttempts.RegisterTime);
            Assert.AreEqual(1, user.AuthenticationAttempts.NumberOfAttempts);
            Assert.AreEqual(1, user.AuthenticationAttempts.GlobalNumberOfAttempts);


            TestUtil.VerifyMocks(_facade, _dao);
        }


        [TestMethod]
        public async Task TestPasswordOnLocking() {
            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10, IsActive = true,AuthenticationAttempts = new AuthenticationAttempt { NumberOfAttempts = 5, GlobalNumberOfAttempts = 5 } };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.WrongPasswordAttempts)).ReturnsAsync(5);

            //asserting that the entry has been saved
            _dao.Setup(d => d.SaveAsync(It.Is((Expression<Func<AuthenticationAttempt, bool>>)(a => a.IsValid() && a.NumberOfAttempts == 6 && a.GlobalNumberOfAttempts == 6)))).ReturnsAsync(user.AuthenticationAttempts);
            _dao.Setup(d => d.SaveAsync(user)).ReturnsAsync(user);

            var result = await _service.MatchPassword(user, "wrong");
            Assert.IsFalse(result);
            Assert.IsTrue(user.Locked.Value);
            Assert.IsTrue(user.IsActive.Value);
            Assert.IsNotNull(user.AuthenticationAttempts);
            Assert.IsNotNull(user.AuthenticationAttempts.RegisterTime);
            Assert.AreEqual(6, user.AuthenticationAttempts.NumberOfAttempts);
            Assert.AreEqual(6, user.AuthenticationAttempts.GlobalNumberOfAttempts);


            TestUtil.VerifyMocks(_facade, _dao);
        }

        [TestMethod]
        public async Task TestPasswordOnLockingPreventSwAdmin() {
            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10, IsActive = true,Locked = false,Systemuser = true };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.WrongPasswordAttempts)).ReturnsAsync(5);

            //asserting that the entry has been saved

            var result = await _service.MatchPassword(user, "wrong");
            Assert.IsFalse(result);
            Assert.IsFalse(user.Locked.Value);
            Assert.IsNull(user.AuthenticationAttempts);
            
            TestUtil.VerifyMocks(_facade, _dao);
        }


        [TestMethod]
        public async Task UnLockUsersIfLimitGotHigher() {
            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10, Locked = true, AuthenticationAttempts = new AuthenticationAttempt { NumberOfAttempts = 5, GlobalNumberOfAttempts = 5 } };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.WrongPasswordAttempts)).ReturnsAsync(10);

            //asserting that the entry has been saved
            _dao.Setup(d => d.SaveAsync(It.Is((Expression<Func<AuthenticationAttempt, bool>>)(a => a.IsValid() && a.NumberOfAttempts == 6 && a.GlobalNumberOfAttempts == 6)))).ReturnsAsync(user.AuthenticationAttempts);
            _dao.Setup(d => d.SaveAsync(user)).ReturnsAsync(user);

            var result = await _service.MatchPassword(user, "wrong");
            Assert.IsFalse(result);
            Assert.IsFalse(user.Locked.Value);
            Assert.IsNotNull(user.AuthenticationAttempts);
            Assert.IsNotNull(user.AuthenticationAttempts.RegisterTime);
            Assert.AreEqual(6, user.AuthenticationAttempts.NumberOfAttempts);
            Assert.AreEqual(6, user.AuthenticationAttempts.GlobalNumberOfAttempts);


            TestUtil.VerifyMocks(_facade, _dao);
        }


        [TestMethod]
        public async Task UserAlreadyLockedDoNotClear() {
            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10, Locked = true, AuthenticationAttempts = new AuthenticationAttempt { NumberOfAttempts = 5, GlobalNumberOfAttempts = 5, RegisterTime = DateTime.Now} };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.WrongPasswordAttempts)).ReturnsAsync(10);

            //asserting that the entry has been saved
//            _dao.Setup(d => d.SaveAsync(user)).ReturnsAsync(user);

            var result = await _service.MatchPassword(user, "test");
            Assert.IsTrue(result);
            Assert.IsTrue(user.Locked.Value);
            Assert.IsNotNull(user.AuthenticationAttempts);
            Assert.IsNotNull(user.AuthenticationAttempts.RegisterTime);
            Assert.AreEqual(5, user.AuthenticationAttempts.NumberOfAttempts);
            Assert.AreEqual(5, user.AuthenticationAttempts.GlobalNumberOfAttempts);


            TestUtil.VerifyMocks(_facade, _dao);
        }


        [TestMethod]
        public async Task UnLockUsersIfLimitGotNull() {
            var user = new User { Password = AuthUtils.GetSha1HashData("test"), Id = 10, Locked = true, AuthenticationAttempts = new AuthenticationAttempt { NumberOfAttempts = 5, GlobalNumberOfAttempts = 5, UserId = 10 } };

            _facade.Setup(f => f.LookupAsync<int>(UserConfigurationConstants.WrongPasswordAttempts)).ReturnsAsync(0);

            //asserting that the entry has been saved
            _dao.Setup(d => d.SaveAsync(It.Is((Expression<Func<AuthenticationAttempt, bool>>)(a => a.UserId != null && a.NumberOfAttempts == 0 && a.GlobalNumberOfAttempts == 0)))).ReturnsAsync(user.AuthenticationAttempts);
            _dao.Setup(d => d.SaveAsync(user)).ReturnsAsync(user);

            var result = await _service.MatchPassword(user, "wrong");
            Assert.IsFalse(result);
            Assert.IsFalse(user.Locked.Value);
            Assert.IsNotNull(user.AuthenticationAttempts);
            Assert.IsNull(user.AuthenticationAttempts.RegisterTime);
            Assert.AreEqual(0, user.AuthenticationAttempts.NumberOfAttempts);
            Assert.AreEqual(0, user.AuthenticationAttempts.GlobalNumberOfAttempts);


            TestUtil.VerifyMocks(_facade, _dao);
        }
    }
}
