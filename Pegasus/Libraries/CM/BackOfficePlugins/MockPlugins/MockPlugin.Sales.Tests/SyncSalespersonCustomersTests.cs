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
using Sage.Connector.Sales.Contracts.BackOffice;

namespace MockPlugin.Sales.Tests
{
    [TestClass]
    public class SyncSalespersonCustomersTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<ISyncSalespersonCustomers, IBackOfficeData>> _backOfficeHandlers;
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
        /// Sync SalespersonCustomers test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestSyncSalespersonCustomers()
        {
            ISyncSalespersonCustomers backofficeFeatureProcessor = null;

            try
            {
                //===========================================================================================================================================================
                // Process Code
                //===========================================================================================================================================================

                backofficeFeatureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                              where backOfficeHandler.Metadata.BackOfficeId.Equals(_backOfficeCompanyData.BackOfficeId,
                                              StringComparison.CurrentCultureIgnoreCase)
                                              select backOfficeHandler.Value).FirstOrDefault();

                Assert.IsNotNull(backofficeFeatureProcessor, String.Format("{0} BackOffice to Sync SalespersonCustomers was not found!", _backOfficeCompanyData.BackOfficeId));

                //Begin back office session with configuration in order to make back office connections.
                var clientResponse = backofficeFeatureProcessor.BeginSession(_sessionContext,_backOfficeCompanyData);

                Assert.IsNotNull(clientResponse, String.Format("BeginSession Failed for Back Office {0}.", _backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status, "Failure returned from BeginSession");


                var initDefaultProperties = backofficeFeatureProcessor as IBackOfficeFeaturePropertyHandler;
                if (initDefaultProperties != null)
                {
                    var defaultPropertyValues = new Dictionary<string, object>();
                    //TODO: by back office - implement testing default value scenarios -  See MockPlugin.SyncCustomersTests
                    initDefaultProperties.Initialize(new ReadOnlyDictionary<string, object>(defaultPropertyValues));
                }

                //Initialize the statements processing to allow back office to set up for the set of salespersonCustomers that will be processed.
                clientResponse = backofficeFeatureProcessor.InitializeSyncSalespersonCustomers();

                Assert.IsNotNull(clientResponse, String.Format("InitializeSyncSalespersonCustomers Failed for Back Office {0}.", _backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status);
                //Loop  through sync logic to get the set of salespersonCustomer payload. 
                var salespersonCustomer = backofficeFeatureProcessor.GetNextSyncSalespersonCustomer();
                var count = 0;
                while (salespersonCustomer != null)
                {
                    count++;
                    //todo:  each back office can decide how to test on return information 
                    Assert.AreEqual((1000 + count).ToString(CultureInfo.CurrentCulture), salespersonCustomer.ExternalId, "unexpected salespersonCustomer data");
                    salespersonCustomer = backofficeFeatureProcessor.GetNextSyncSalespersonCustomer();
                }

                //TODO:  we need some way of knowing how many are coming from the back end.. For mock plugin, it is known. 
                Assert.AreEqual(5, count);
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
