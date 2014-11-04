using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using BackOfficePluginTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Data.Metadata;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Statements.Contracts.BackOffice;
using Sage.Connector.Statements.Contracts.Data.Requests;

namespace MockPlugin.Statements.Tests
{
    [TestClass]
    public class ProcessStatementsTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IProcessStatements, IBackOfficeData>> _backOfficeHandlers;
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
        /// Process Statements test.  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>

        [TestMethod]
        public void TestProcessStatements()
        {
            IProcessStatements backofficeFeatureProcessor = null;
            try
            {
                //===========================================================================================================================================================
                // Process Code
                //===========================================================================================================================================================

                backofficeFeatureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                                                 where backOfficeHandler.Metadata.BackOfficeId.Equals(_backOfficeCompanyData.BackOfficeId,
                                                                 StringComparison.CurrentCultureIgnoreCase)
                                                                 select backOfficeHandler.Value).FirstOrDefault();

                Assert.IsNotNull(backofficeFeatureProcessor, String.Format("{0} BackOffice to Process Statements was not found!", _backOfficeCompanyData.BackOfficeId));

                //Begin back office session with configuration in order to make back office connections.
                var clientResponse = backofficeFeatureProcessor.BeginSession(_sessionContext, _backOfficeCompanyData);

                Assert.IsNotNull(clientResponse, String.Format("BeginSession Failed for Back Office {0}.", _backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status, "Failure returned from BeginSession");

                var statementsRequest = new StatementsRequest
                {
                    StatementDate = DateTime.Now,
                    CustomerReferences = new Collection<ExternalReference>()
                };

                //TODO: by plugin developer
                //TODO: create a set of customer reference valid for the back office

                for (var i = 0; i < 5; i++)
                {
                    statementsRequest.CustomerReferences.Add(new ExternalReference
                    {
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture)
                    });
                }

                //Initialize the statements processing to allow back office to set up for the set of customers that will be processed.
                clientResponse = backofficeFeatureProcessor.InitializeProcessStatements(statementsRequest);

                Assert.IsNotNull(clientResponse, String.Format("InitializeProcessStatements Failed for Back Office {0}.",_backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status);


                var statementRequest = new StatementRequest { StatementDate = statementsRequest.StatementDate };
                int customerCount = statementsRequest.CustomerReferences.Count;
                for (var i =0; i< customerCount; i++)
                {

                    var statementResponse = backofficeFeatureProcessor.GetNextStatement();
                    Assert.IsNotNull(statementResponse, "response should not be null.  At minumum should have status and diagnoses");
                    //it is actually okay to have a null statement as long as there is a diagnosis with serverity error
                    //TODO: this is happy path test.  so another test should get created to test for the diagnosis issue. 
                    Assert.IsNotNull(statementResponse.Statement);

                }
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
