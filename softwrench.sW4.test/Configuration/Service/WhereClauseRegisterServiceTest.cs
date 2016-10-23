using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using cts.commons.persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.test.Util;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;

namespace softwrench.sW4.test.Configuration.Service {
    [TestClass]
    public class WhereClauseRegisterServiceTest {

        private WhereClauseRegisterService _service;

        private readonly Mock<ISWDBHibernateDAO> _swdbDAO = TestUtil.CreateMock<ISWDBHibernateDAO>();
        private readonly Mock<UserProfileManager> _userManager = TestUtil.CreateMock<UserProfileManager>();


        [TestInitialize]
        public void Init() {
            TestUtil.ResetMocks(_swdbDAO, _userManager);
            _service = new WhereClauseRegisterService(_swdbDAO.Object, _userManager.Object);
        }


        [TestMethod]
        public async Task TestRegisterNoCondition() {
            var result = await _service.DoRegister("test", "value", null);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.SimpleDefinitionUpdate, result);

            //second time system starts
            result = await _service.DoRegister("test", "value", null);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.SimpleDefinitionUpdate, result);

            TestUtil.VerifyMocks(_swdbDAO, _userManager);

        }

        [TestMethod]
        public async Task TestRegisterWithConditionFirstTimeCreatingValue() {
            var conditionToRegister = new WhereClauseRegisterCondition {
                OfflineOnly = true,
                Global = true,
                Alias = "offline"
            };

            var existingDefinition = WhereClauseRegisterService.GetDefinitionToSave("test", "newvalue", false);



            var existingCondition = new WhereClauseCondition() {
                Id = 1,
                OfflineOnly = true,
                Global = true,
                Alias = "offline"
            };

            var oldValue = new PropertyValue {
                StringValue = "oldvalue",
                Condition = existingCondition,
                Definition = existingDefinition
            };

            var newValue = new PropertyValue {
                SystemStringValue = "newvalue",
                Condition = existingCondition,
                Definition = existingDefinition

            };

            Expression<Func<PropertyDefinition, bool>> defComparison = u => u.FullKey == existingDefinition.FullKey;

            _swdbDAO.Setup(f => f.SaveAsync(It.Is(defComparison))).ReturnsAsync(existingDefinition);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<Condition>(Condition.ByAlias, conditionToRegister.Alias)).ReturnsAsync(existingCondition);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<PropertyValue>(PropertyValue.ByDefinitionConditionIdModuleProfile, "test", existingCondition.Id, null, null)).ReturnsAsync(null);

            _swdbDAO.Setup(f => f.SaveAsync(newValue)).ReturnsAsync(newValue);

            //second time system starts
            var result = await _service.DoRegister("test", "newvalue", conditionToRegister);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.ValueCreation, result);

            TestUtil.VerifyMocks(_swdbDAO, _userManager);


        }


        [TestMethod]
        public async Task TestRegisterWithConditionSecondTimeUpdatingValue() {
            var conditionToRegister = new WhereClauseRegisterCondition {
                OfflineOnly = true,
                Global = true,
                Alias = "offline"
            };

            var existingDefinition = WhereClauseRegisterService.GetDefinitionToSave("test", "newvalue",false);



            var existingCondition = new WhereClauseCondition() {
                Id = 1,
                OfflineOnly = true,
                Global = true,
                Alias = "offline"
            };

            var oldValue = new PropertyValue {
                StringValue = "oldvalue",
                Condition = existingCondition,
                Definition = existingDefinition
            };

            var newValue = new PropertyValue {
                SystemStringValue = "newvalue",
                Condition = existingCondition,
                Definition = existingDefinition

            };

            Expression<Func<PropertyDefinition, bool>> defComparison = u => u.FullKey == existingDefinition.FullKey;

            _swdbDAO.Setup(f => f.SaveAsync(It.Is(defComparison))).ReturnsAsync(existingDefinition);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<Condition>(Condition.ByAlias, conditionToRegister.Alias)).ReturnsAsync(existingCondition);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<PropertyValue>(PropertyValue.ByDefinitionConditionIdModuleProfile, "test", existingCondition.Id, null, null)).ReturnsAsync(oldValue);

            _swdbDAO.Setup(f => f.SaveAsync(newValue)).ReturnsAsync(newValue);

            //second time system starts
            var result = await _service.DoRegister("test", "newvalue", conditionToRegister);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.ValueUpdate, result);

            TestUtil.VerifyMocks(_swdbDAO, _userManager);


        }
    }
}
