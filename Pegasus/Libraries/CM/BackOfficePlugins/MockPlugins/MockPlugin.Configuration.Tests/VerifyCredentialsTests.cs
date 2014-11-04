using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BackOfficePluginTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sage.Connector.Configuration.Contracts.BackOffice;
using Sage.Connector.Configuration.Contracts.Data;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data.Metadata;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.MockPlugin.Configuration;

namespace MockPlugin.Configuration.Tests
{
    [TestClass]
    public class VerifyCredentialsTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IVerifyCredentials, IBackOfficeData>> _backOfficeHandlers;
#pragma warning restore 649

        private IBackOfficeCompanyData _backOfficeCompanyData;
        private CancellationTokenSource _cancellationTokenSource;
        private ISessionContext _sessionContext;

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

            _cancellationTokenSource = new CancellationTokenSource();
            _sessionContext = new SessionContext(new Logger(), _cancellationTokenSource.Token);
        }


        /// <summary>
        /// Get Company Connection Management Credentials Needed test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestGetCompanyConnectionManagementCredentialsNeeded()
        {
            string backOfficeId = _backOfficeCompanyData.BackOfficeId;
            const string featureName = "GetCompanyConnectionCredentialsNeeded";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            var response = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded(_sessionContext);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Success, response.Status, string.Format("Unsuccessful response from {0}", featureName));
            Assert.AreEqual(3, response.Descriptions.Count, string.Format("Description count was unexpected in response from {0}", featureName));
            Assert.AreEqual(3, response.CurrentValues.Count, string.Format("CurrentValues count was unexpected in response from {0}", featureName));
        }

        [TestMethod]
        public void TestValidateCompanyConnectionManagementCredentials()
        {
            string backOfficeId = _backOfficeCompanyData.BackOfficeId;
            const string featureName = "ValidateCompanyConnectionManagementCredentials";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            var needed = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded(_sessionContext);
            needed.CurrentValues[CommonPropertyKeys.UserId] = "admin";
            needed.CurrentValues[CommonPropertyKeys.Password] = "1";
            var response = featureProcessor.ValidateCompanyConnectionManagementCredentials(_sessionContext, needed.CurrentValues);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Success, response.Status, string.Format("Unsuccessful response from {0}", featureName));

            needed.CurrentValues[CommonPropertyKeys.UserId] = "bad";
            needed.CurrentValues[CommonPropertyKeys.Password] = "values";
            response = featureProcessor.ValidateCompanyConnectionManagementCredentials(_sessionContext, needed.CurrentValues);
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
            const string featureName = "GetCompanyConnectionCredentialsNeeded";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            var mgtCredentialsNeeded = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded(_sessionContext);

            mgtCredentialsNeeded.CurrentValues[CommonPropertyKeys.UserId] = "admin";
            mgtCredentialsNeeded.CurrentValues[CommonPropertyKeys.Password] = "1";
            var companyCredentials = new CompanyCredentials
            {
                CompanyManagementCredentials = mgtCredentialsNeeded.CurrentValues,
                CompanyConnectionCredentials = new Dictionary<string, string>()
            };
            var response = featureProcessor.GetCompanyConnectionCredentialsNeeded(_sessionContext, companyCredentials);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Success, response.Status, string.Format("Unsuccessful response from {0}", featureName));
            Assert.AreEqual(4, response.Descriptions.Count, string.Format("Description count was unexpected in response from {0}", featureName));
            Assert.AreEqual(4, response.CurrentValues.Count, string.Format("CurrentValues count was unexpected in response from {0}", featureName));

            //Dig into the CompanyId description and make sure there is a valueName collection as we are expecting a list.
            string json = JsonConvert.SerializeObject(response);
            var j = JObject.Parse(json);
            var companyList = j["Descriptions"]["CompanyId"]["ValueName"];
            Assert.IsNotNull(companyList, "CompanyList not found");

            var list = companyList.ToObject<IList<String>>();
            Assert.AreEqual(28, list.Count, "Unexpected number of back office companies found");
        }
        
        /// <summary>
        /// ValidateCompanyConnection Credentials
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestValidateCompanyConnectionCredentials()
        {
            string backOfficeId = _backOfficeCompanyData.BackOfficeId;
            const string featureName = "ValidateCompanyConnectionCredentials";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            //get the management credentials as we need them to get connection credentials.
            //in this case they default to valid values so can pass the default values straight on to ConnectionCredentials
            var mgtNeeded = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded(_sessionContext);

            //Get the connection credentials needed.
            var companyCredentials = new CompanyCredentials
            {
                CompanyManagementCredentials = mgtNeeded.CurrentValues,
                CompanyConnectionCredentials = new Dictionary<string, string>()
            };
            var neededConCredentials = featureProcessor.GetCompanyConnectionCredentialsNeeded(_sessionContext, companyCredentials);
            
            //supply a valid CompanyId
            neededConCredentials.CurrentValues[CommonPropertyKeys.CompanyId] = "Company0";
            neededConCredentials.CurrentValues[CommonPropertyKeys.UserId] = "bad";
            neededConCredentials.CurrentValues[CommonPropertyKeys.Password] = "values";
            var response = featureProcessor.ValidateCompanyConnectionCredentials(_sessionContext, neededConCredentials.CurrentValues);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Failure, response.Status, string.Format(" Unexpected response from {0}", featureName));

            neededConCredentials.CurrentValues[CommonPropertyKeys.UserId] = "user";
            neededConCredentials.CurrentValues[CommonPropertyKeys.Password] = "password";
            response = featureProcessor.ValidateCompanyConnectionCredentials(_sessionContext, neededConCredentials.CurrentValues);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
            Assert.AreEqual(Status.Success, response.Status, string.Format(" Unexpected response from {0}", featureName));
        }

        ///<summary>
        /// Test / Demonstrate cancellation
        /// </summary>
        [TestMethod]
        public void TestCanclRequestInGetCompanyConnectionCredentialsNeeded()
        {
            string backOfficeId = _backOfficeCompanyData.BackOfficeId;
            const string featureName = "GetCompanyConnectionCredentialsNeeded";

            var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            var mgtCredentialsNeeded = featureProcessor.GetCompanyConnectionManagementCredentialsNeeded(_sessionContext);

            mgtCredentialsNeeded.CurrentValues[CommonPropertyKeys.UserId] = "admin";
            mgtCredentialsNeeded.CurrentValues[CommonPropertyKeys.Password] = "1";
            var companyCredentials = new CompanyCredentials
            {
                CompanyManagementCredentials = mgtCredentialsNeeded.CurrentValues,
                CompanyConnectionCredentials = new Dictionary<string, string>()
            };

            try
            {
                //call GetCompanyConnectionCredentialsNeeded with an already canceled CancellationToken.
                //In a live connector this will be triggered asynchronously in the case of requested cancel.
                //The canceling a request feature is not yet active in the connector, but the expected behavior
                //is that if the token is triggered the plugin will exit and fail the request. Once triggered there
                //will likely be a short period of time after which if the plugin has not yet exited the process
                //and its children will be terminated by the connector.

                var cancellationTokenSource = new CancellationTokenSource();
                var context = new SessionContext(new Logger(), cancellationTokenSource.Token);
                cancellationTokenSource.Cancel();
                var response = featureProcessor.GetCompanyConnectionCredentialsNeeded(context, companyCredentials);
                Assert.Fail("Expected OperationCanceledException in {0} did not occur", featureName);
            }
            catch (OperationCanceledException)
            {
                //no content as this is the expected happy path for this test
            }
        }
    }
}






