using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using BackOfficePluginTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Data.Metadata;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Payments.Contracts.BackOffice;
using Sage.Connector.Payments.Contracts.Data;

namespace MockPlugin.Payments.Tests
{
    [TestClass]
    public class ProcessPaymentTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IProcessPayment, IBackOfficeData>> _backOfficeHandlers;
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
        /// Process Payment for Invoice test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestProcessPaymentInvoice()
        {
            TestProcessPayment(true);
        }

        /// <summary>
        /// Process Payment for statement test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestProcessPaymentStatement()
        {
            TestProcessPayment(false);

        }

        /// <summary>
        /// process Payment 
        /// </summary>
        /// <param name="isInvoice">true if invoice, false if statement</param>
        private void TestProcessPayment(bool isInvoice)
        {
            IProcessPayment backofficeFeatureProcessor = null;
            try
            {

                //===========================================================================================================================================================
                // Process Code
                //===========================================================================================================================================================

                backofficeFeatureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                              where backOfficeHandler.Metadata.BackOfficeId.Equals(_backOfficeCompanyData.BackOfficeId,
                                                  StringComparison.CurrentCultureIgnoreCase)
                                              select backOfficeHandler.Value).FirstOrDefault();

                Assert.IsNotNull(backofficeFeatureProcessor,
                    String.Format("{0} BackOffice to Process Payment was not found!",
                        _backOfficeCompanyData.BackOfficeId));


                //Begin back office session with configuration in order to make back office connections.
                var clientResponse = backofficeFeatureProcessor.BeginSession(_sessionContext, _backOfficeCompanyData);

                Assert.IsNotNull(clientResponse,
                    String.Format("BeginSession Failed for Back Office {0}.",
                        _backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status, "Failure returned from BeginSession");


                var initDefaultProperties = backofficeFeatureProcessor as IBackOfficeFeaturePropertyHandler;
                if (initDefaultProperties != null)
                {
                    var defaultPropertyValues = new Dictionary<string, object>();
                    //TODO: by back office - implement testing default value scenarios -  See MockPlugin.SyncCustomersTests
                    initDefaultProperties.Initialize(new ReadOnlyDictionary<string, object>(defaultPropertyValues));
                }


                var payment = CreatePayment(isInvoice);

                var response = backofficeFeatureProcessor.ProcessPayment(payment);
                Assert.IsNotNull(response, "Null response from ProcessPayment");
                Assert.AreNotEqual(Status.Failure, response.Status, "Failure status returned from ProcessPayment.");

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

        /// <summary>
        /// TODO by backoffice plugin developer:  Need to set up data that the erp can process based on payment contract. 
        /// </summary>
        /// <returns></returns>
        private Payment CreatePayment(bool isInvoice)
        {
            var payment = new Payment
            {
                Customer = new ExternalReference
                {
                    //TODO  by backoffice plugin developer: needs to be an actual back office key to customer
                    ExternalId = "1001"
                },

                AmountPaid = 550m,
                AuthorizationCode = "xyz",
                CreditCardLast4 = "1234",
                ExpirationMonth = "05",
                ExpirationYear = "2015",
                Reference = "Ref 1001",
                TransactionDate = DateTime.UtcNow,
                Type = PaymentType.CreditCard,
                EntityStatus = EntityStatus.Active
            };


            if (isInvoice)
            {
                var detail = new PaymentDetail
                {
                    AppliedAmount = 550m,
                    Invoice = new ExternalReference
                    {
                        ExternalId = "Inv 1001",
                    }

                };

                payment.Details.Add(detail);
            }


            return payment;

        }
    }
}
