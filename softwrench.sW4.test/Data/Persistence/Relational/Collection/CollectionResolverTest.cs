using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Util;
using softwrench.sW4.test.Poco;
using softwrench.sW4.test.Util;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.test.Data.Persistence.Relational.Collection {

    [TestClass]
    public class CollectionResolverTest : BaseOtbMetadataTest {

        private SlicedEntityMetadata _srMetadata;
        private ApplicationMetadata _srAppMetadata;

        private Mock<IContextLookuper> lookuperMock = TestUtil.CreateMock<IContextLookuper>();

        [TestInitialize]
        public override void Init() {
            base.Init();
            _srAppMetadata = MetadataProvider.Application("servicerequest").StaticFromSchema("editdetail");
            _srMetadata = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("sr"), _srAppMetadata.Schema);
            TestUtil.ResetMocks(lookuperMock);
        }

        [TestMethod]
        public void TestAllowNull() {
            
            var resolver = new CollectionResolver(null, lookuperMock.Object);

            var contextToUse = new ContextHolder();

            var listOfEntities = new List<AttributeHolder> { PocoSr.BasicForRelationship()};

            var parameter = new InternalCollectionResolverParameter() {
                CollectionAssociation = _srMetadata.LocateAssociationByName("worklog"),
                ExternalParameters = new CollectionResolverParameters(_srAppMetadata, listOfEntities, null),
                Ctx = contextToUse
            };

            var matchingResultWrapper = new CollectionResolver.CollectionMatchingResultWrapper();
            var dto = resolver.BuildSearchRequestDto(parameter, matchingResultWrapper);
            Assert.AreEqual("recordkey&&siteid&&class", dto.SearchParams);
            Assert.AreEqual("1,,,nullor:site,,,SR", dto.SearchValues);

            TestUtil.VerifyMocks(lookuperMock);
        }
    }
}
