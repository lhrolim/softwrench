using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using cts.commons.persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.api.classes.audit;
using softwrench.sW4.test.Util;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.test.Configuration.Service {
    [TestClass]
    public class WhereClauseRegisterServiceTest {

        private WhereClauseRegisterService _service;

        private Mock<ISWDBHibernateDAO> _swdbDAO;
        private Mock<EntityRepository> _entityRepository;
        private Mock<ConfigurationCache> _configurationCache;
        private Mock<IAuditManagerCommons> _auditManagerCommons;


        [TestInitialize]
        public void Init() {
            _swdbDAO = TestUtil.CreateMock<ISWDBHibernateDAO>();
            _entityRepository = TestUtil.CreateMock<EntityRepository>();
            _configurationCache = TestUtil.CreateMock<ConfigurationCache>();
            _auditManagerCommons = TestUtil.CreateMock<IAuditManagerCommons>();
            TestUtil.ResetMocks(_swdbDAO, _entityRepository, _configurationCache);
            _service = new WhereClauseRegisterService(_swdbDAO.Object, null, _entityRepository.Object, _configurationCache.Object,_auditManagerCommons.Object);
        }


        [TestMethod]
        public async Task TestRegisterNoCondition() {
            //TODO: improve this test
            var result = await _service.DoRegister("test", "value", null);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.ValueCreation, result.Operation);

            //second time system starts
            //            result = await _service.DoRegister("test", "value", null);
            //            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.ValueUpdate, result);
            //
            TestUtil.VerifyMocks(_swdbDAO);

        }

        [TestMethod]
        public async Task TestRegisterWithConditionFirstTimeCreatingValue() {
            var conditionToRegister = new WhereClauseRegisterCondition {
                OfflineOnly = true,
                Global = true,
                Alias = "offline"
            };

            var existingDefinition = WhereClauseRegisterService.GetDefinitionToSave("test", "newvalue", false);



            var existingCondition = new WhereClauseCondition {
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

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<Condition>(Condition.ByAlias, conditionToRegister.Alias,"test")).ReturnsAsync(existingCondition);

            _swdbDAO.Setup(f => f.SaveAsync(It.Is((Expression<Func<Condition, bool>>) (u => u.Alias == existingCondition.Alias)))).ReturnsAsync(existingCondition);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<PropertyValue>(PropertyValue.ByDefinitionConditionIdModuleProfile, "test", existingCondition.Id, null, null)).ReturnsAsync(null);

            _swdbDAO.Setup(f => f.SaveAsync(newValue)).ReturnsAsync(newValue);

            var result = await _service.DoRegister("test", "newvalue", conditionToRegister);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.ValueCreation, result.Operation);

            TestUtil.VerifyMocks(_swdbDAO);


        }


        [TestMethod]
        public async Task TestRegisterWithConditionSecondTimeUpdatingValue() {
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

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<Condition>(Condition.ByAlias, conditionToRegister.Alias, "test")).ReturnsAsync(existingCondition);

            _swdbDAO.Setup(f => f.SaveAsync(It.Is((Expression<Func<Condition, bool>>)(u => u.Alias == existingCondition.Alias)))).ReturnsAsync(existingCondition);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<PropertyValue>(PropertyValue.ByDefinitionConditionIdModuleProfile, "test", existingCondition.Id, null, null)).ReturnsAsync(oldValue);

            _swdbDAO.Setup(f => f.SaveAsync(newValue)).ReturnsAsync(newValue);

            _configurationCache.Setup(s => s.ClearCache("test"));

            //second time system starts
            var result = await _service.DoRegister("test", "newvalue", conditionToRegister);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.ValueUpdate, result.Operation);

            TestUtil.VerifyMocks(_swdbDAO, _configurationCache);


        }


        [TestMethod]
        public async Task TestRegisterNewFromScreenNoAlias() {
            var conditionToRegister = new WhereClauseRegisterCondition {
                OfflineOnly = true,
                AppContext = new ApplicationLookupContext {
                    Schema = "list"
                }
            };



            var existingDefinition = WhereClauseRegisterService.GetDefinitionToSave("test", "newvalue", false);

            var newValue = new PropertyValue {
                SystemStringValue = "newvalue",
                Condition = conditionToRegister.RealCondition,
                Definition = existingDefinition
            };


            Expression<Func<PropertyDefinition, bool>> defComparison = u => u.FullKey == existingDefinition.FullKey;

            _swdbDAO.Setup(f => f.SaveAsync(It.Is(defComparison))).ReturnsAsync(existingDefinition);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<Condition>(Condition.ByAlias, conditionToRegister.Alias, "test")).ReturnsAsync(null);

            _swdbDAO.Setup(f => f.FindSingleByQueryAsync<PropertyValue>(PropertyValue.ByDefinitionNoCondition, "test", null)).ReturnsAsync(null);

            _swdbDAO.Setup(f => f.SaveAsync(newValue)).ReturnsAsync(newValue);

            var result = await _service.DoRegister("test", "newvalue", conditionToRegister);
            Assert.AreEqual(WhereClauseRegisterService.WCRegisterOperation.ValueCreation, result.Operation);
            Assert.AreEqual(newValue, result.PropertyValue);

            TestUtil.VerifyMocks(_swdbDAO, _configurationCache);


        }
    }
}
