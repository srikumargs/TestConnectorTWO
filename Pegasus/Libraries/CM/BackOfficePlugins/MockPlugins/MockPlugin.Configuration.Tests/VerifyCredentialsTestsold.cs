using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using BackOfficePluginTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sage.Connector.Configuration.Contracts.Features;
using Sage.Connector.DomainContracts.Core;
using Sage.Connector.DomainContracts.Core.BackOffice;
using Sage.Connector.DomainContracts.Core.Metadata;
using Sage.Connector.DomainMediator.Core;

namespace MockPlugin.Configuration.Tests
{
    [TestClass]
    class VerifyCredentialsTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IVerifyCredentials, IBackOfficeData>> _backOfficeHandlers;
#pragma warning restore 649

        private IBackOfficeCompanyData _backOfficeCompanyData;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            //Set up for back office company configuration
            _backOfficeCompanyData = new BackOfficeCompanyData
            {
                BackOfficeId = "Mock",
                ConnectionCredentials = DefaultConnectionCredentials.Credentials
            };
        }


        /// <summary>
        /// Get Company Connection Management Credentials Needed test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestGetCompanyConnectionManagementCredentialsNeeded()
        {
            string backOfficeId = _backOfficeCompanyData.BackOfficeId;
            string featureName = "GetCompanyConnectionCredentialsNeeded";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);
            
            var response = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded();
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Success, response.Status, string.Format("Unsuccessful response from {0}", featureName));
            Assert.AreEqual(3, response.Descriptions.Count, string.Format("Description count was unexpected in response from {0}", featureName));
            Assert.AreEqual(3, response.CurrentValues.Count, string.Format("CurrentValues count was unexpected in response from {0}", featureName));
        }

        //GetCompanyConnectionCredentialsNeeded
        //ValidateCompanyConnectionCredentials
        [TestMethod]
        public void TestValidateCompanyConnectionManagementCredentials()
        {
            string backOfficeId = _backOfficeCompanyData.BackOfficeId;
            string featureName = "ValidateCompanyConnectionManagementCredentials";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            var needed = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded();
            needed.CurrentValues["UserId"] = "admin";
            needed.CurrentValues["Password"] = "1";
            var response = featureProcessor.ValidateCompanyConnectionManagementCredentials(needed.CurrentValues);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Success, response.Status, string.Format("Unsuccessful response from {0}", featureName));

            needed.CurrentValues["UserId"] = "bad";
            needed.CurrentValues["Password"] = "values";
            response = featureProcessor.ValidateCompanyConnectionManagementCredentials(needed.CurrentValues);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Failure, response.Status, string.Format(" Unexpected response from {0}", featureName));
        }

              /// <summary>
        /// Get Company Connection Management Credentials Needed test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestGetCompanyConnectionCredentialsNeeded()
        {
            string backOfficeId = _backOfficeCompanyData.BackOfficeId;
            string featureName = "GetCompanyConnectionCredentialsNeeded";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);
            
            var needed = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded();

            needed.CurrentValues["UserId"] = "admin";
            needed.CurrentValues["Password"] = "1";
            var response = featureProcessor.GetCompanyConnectionCredentialsNeeded(needed.CurrentValues);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Success, response.Status, string.Format("Unsuccessful response from {0}", featureName));
            Assert.AreEqual(3, response.Descriptions.Count, string.Format("Description count was unexpected in response from {0}", featureName));
            Assert.AreEqual(3, response.CurrentValues.Count, string.Format("CurrentValues count was unexpected in response from {0}", featureName));

            string json = JsonConvert.SerializeObject(response);
            var j = JObject.Parse(json);
            var companyList = j["Descriptions"]["CompanyId"]["ValueName"];
            Assert.IsNotNull(companyList, "CompanyList not found");
            
            var list = companyList.ToObject<IList<String>>();
            Assert.AreEqual(28, list.Count, "Unexpected number of back office companies found");
        }
            
    }
}






