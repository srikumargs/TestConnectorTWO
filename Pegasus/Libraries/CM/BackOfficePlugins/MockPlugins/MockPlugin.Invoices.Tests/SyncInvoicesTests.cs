﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using BackOfficePluginTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data.Metadata;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Invoices.Contracts.BackOffice;

namespace MockPlugin.Invoices.Tests
{
    [TestClass]
    public class SyncInvoicesTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<ISyncInvoices, IBackOfficeData>> _backOfficeHandlers;
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
        /// Sync Invoices test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestSyncInvoices()
        {
            ISyncInvoices backofficeFeatureProcessor = null;

            try
            {
                //===========================================================================================================================================================
                // Process Code
                //===========================================================================================================================================================

                backofficeFeatureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                              where backOfficeHandler.Metadata.BackOfficeId.Equals(_backOfficeCompanyData.BackOfficeId,
                                              StringComparison.CurrentCultureIgnoreCase)
                                              select backOfficeHandler.Value).FirstOrDefault();

                Assert.IsNotNull(backofficeFeatureProcessor, String.Format("{0} BackOffice to Sync Invoices was not found!",_backOfficeCompanyData.BackOfficeId));

                //Begin back office session with configuration in order to make back office connections.
                var clientResponse = backofficeFeatureProcessor.BeginSession(_sessionContext, _backOfficeCompanyData);

                Assert.IsNotNull(clientResponse, String.Format("BeginSession Failed for Back Office {0}.",_backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status, "Failure returned from BeginSession");

                var initDefaultProperties = backofficeFeatureProcessor as IBackOfficeFeaturePropertyHandler;
                if (initDefaultProperties != null)
                {
                    var defaultPropertyValues = new Dictionary<string, object>();
                    //TODO: by back office - implement testing default value scenarios -  See MockPlugin.SyncCustomersTests
                    initDefaultProperties.Initialize(new ReadOnlyDictionary<string, object>(defaultPropertyValues));
                }

                var propertyBag = new Dictionary<string, object>(); 
                //TODO by BackOffice developer.  Determine the state in which to start your test  
             
                //Initialize the statements processing to allow back office to set up for the set of invoices that will be processed.
                clientResponse = backofficeFeatureProcessor.InitializeSyncInvoices(propertyBag);

                Assert.IsNotNull(clientResponse, String.Format("InitializeSyncInvoices Failed for Back Office {0}.",_backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status);
                //Loop  through sync logic to get the set of invoice payload. 
                var invoice = backofficeFeatureProcessor.GetNextSyncInvoice();
                var count = 0;
                while (invoice != null)
                {
                    count++;
                    //todo:  each back office can decide how to test on return information 
                    Assert.AreEqual((1000 + count).ToString(CultureInfo.CurrentCulture), invoice.ExternalId, "unexpected invoice data");
                    invoice = backofficeFeatureProcessor.GetNextSyncInvoice();
                }

                //TODO:  we need some way of knowing how many are coming from the back end.. For mock plugin, it is known. 
                Assert.AreEqual(3, count);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                //make sure that in case of error, back office erp has a chance to close. 
                if (backofficeFeatureProcessor != null)
                {
                    backofficeFeatureProcessor.EndSession();
                }
            }

        }
    }
}
