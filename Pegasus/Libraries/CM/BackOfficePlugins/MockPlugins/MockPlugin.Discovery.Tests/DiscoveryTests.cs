using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using BackOfficePluginTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sage.Connector.Discovery.Contracts.BackOffice;
using Sage.Connector.Discovery.Contracts.Data.Metadata;
using Sage.Connector.DomainContracts.BackOffice;

namespace MockPlugin.Discovery.Tests
{
    [TestClass]
    public class DiscoveryTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        //Use special import as IDiscovery exports richer meta data
        private IEnumerable<Lazy<IDiscovery, IBackOfficeConfigMetadata>> _backOfficeHandlers;
        //private IEnumerable<Lazy<IDiscovery, IBackOfficeData>> _backOfficeHandlers;
#pragma warning restore 649

        
        private CancellationTokenSource _cancellationTokenSource;
        private ISessionContext _sessionContext;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _cancellationTokenSource = new CancellationTokenSource();
            _sessionContext = new SessionContext(new Logger(), _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Is Back Office Installed test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestIsBackOfficeInstalled()
        {
            string backOfficeId = "Mock";
            string featureName = "IsBackOfficeInstalled";

            //Use special import as IDiscovery exports richer meta data
            //var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            IDiscovery featureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                    where backOfficeHandler.Metadata.BackOfficeId.Equals(backOfficeId, StringComparison.CurrentCultureIgnoreCase)
                                    select backOfficeHandler.Value).FirstOrDefault();

            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            var response = featureProcessor.IsBackOfficeInstalled(_sessionContext);
            Assert.IsTrue(response, string.Format("False response from {0}", featureName));
        }

        /// <summary>
        /// Get Plugin Information test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestGetPluginInformation()
        {
            string backOfficeId = "Mock";
            string featureName = "GetPluginInformation";

            //Use special import as IDiscovery exports richer meta data
            //var featureProcessor = TestHelpers.FindFeatureProcessor(_backOfficeHandlers, backOfficeId);
            var featureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                    where backOfficeHandler.Metadata.BackOfficeId.Equals(backOfficeId, StringComparison.CurrentCultureIgnoreCase)
                                    select backOfficeHandler.Value).FirstOrDefault();
            TestHelpers.ValidateFeatureProcessor(featureProcessor, backOfficeId);

            var response = featureProcessor.GetPluginInformation(_sessionContext);
            Assert.IsNotNull(response, string.Format("Null response from {0}", featureName));
        }
    }
}
