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
using Sage.Connector.Sales.Contracts.BackOffice;
using Sage.Connector.Sales.Contracts.Data;

namespace MockPlugin.Sales.Tests
{
    [TestClass]
    public class ProcessPaidOrderTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IProcessPaidOrder, IBackOfficeData>> _backOfficeHandlers;
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
        /// Process Paid Order test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestProcessPaidOrder()
        {
            IProcessPaidOrder backofficeFeatureProcessor = null;
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
                    String.Format("{0} BackOffice to Process Paid Order was not found!",
                        _backOfficeCompanyData.BackOfficeId));


                //Begin back office session with configuration in order to make back office connections.
                var clientResponse = backofficeFeatureProcessor.BeginSession(_sessionContext,_backOfficeCompanyData);

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

                var paidOrder = CreatePaidOrder();

                var response = backofficeFeatureProcessor.ProcessPaidOrder(paidOrder);
                Assert.IsNotNull(response, "Null response from ProcessPaidOrder");
                Assert.AreNotEqual(Status.Failure, response.Status, "Failure status returned from ProcessPaidOrder.");

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
        /// TODO by backoffice plugin developer:  Need to set up data that the erp can process based on paidOrder contract. 
        /// </summary>
        /// <returns></returns>
        private PaidOrder CreatePaidOrder()
        {
            var paidOrder = new PaidOrder
            {
                Customer = new ExternalReference
                {
                    //TODO  by backoffice plugin developer: needs to be an actual back office key to customer
                    ExternalId = "1001"
                },

                DocumentNumber = 1,
                Description = "PaidOrder Desc",
                EntityStatus = EntityStatus.Active,
                //TODO: Setting to utc date, but may actually be local.. Requires conversation among connector/platform team
                SubmittedDate = DateTime.UtcNow,
                ShippingAddress = new Address
                {
                    //TODO: ask back office if they need an external id for address in this sales domain process paidOrder contract
                    Street1 = "1234 Street 1",
                    Street2 = "Ste. 104",
                    City = "Irvine",
                    StateProvince = "CA",
                    PostalCode = "92618",
                    Country = "USA",
                    Name = "Irvine Office",
                    Contact = new AddressContact
                    {
                        //TODO: ask back office if they need an external id for contact in this sales domain process paidOrder contract
                        FirstName = "Joe",
                        LastName = "Smith",
                        EmailWork = "js@sage.com",
                        PhoneWork = "949.555.1212"
                    }
                },
                DocumentTotal = 200m,
                SubTotal = 150m,
                Tax = 15m,
                SandH = 35m



            };


            for (var i = 0; i < 5; i++)
            {
                var detail = new PaidOrderDetail
                {
                    InventoryItem = new ExternalReference
                    {
                        //TODO by back office plugin developer: 
                        //TODO: create back office erp external id related to some actual inventory item 
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture)
                    },
                    Quantity = i,
                    Price = 10m * i
                };
                paidOrder.Details.Add(detail);

            }
            return paidOrder;

        }
    }
}
