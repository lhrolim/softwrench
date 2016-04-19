using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;

namespace softwrench.sW4.test {


    [DeploymentItem(@"App_data\Client\test_only", "Client\\test_only")]
    [DeploymentItem(@"App_data\Client\test2", "Client\\test2")]
    [DeploymentItem(@"App_data\Client\test3", "Client\\test3")]
    [DeploymentItem(@"App_data\Client\test4", "Client\\test4")]
    [DeploymentItem(@"App_data\jsons\", "jsons")]
    [DeploymentItem(@"..\Debug")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\@internal", "Client\\@internal")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\otb", "Client\\otb")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\hapag", "Client\\hapag")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\chicago", "Client\\chicago")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\entegra", "Client\\entegra")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\gric", "Client\\gric")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\firstsolar", "Client\\firstsolar")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\manchester", "Client\\manchester")]
    [DeploymentItem(@"..\\..\\..\softwrench.sW4.web\App_Data\client\kongsberg", "Client\\kongsberg")]
    [TestClass]
    public abstract class BaseMetadataTest
    {

    }
}
