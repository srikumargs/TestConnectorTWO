using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using BackOfficePluginTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Data.Metadata;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Service.Contracts.BackOffice;
using Sage.Connector.Service.Contracts.Data;

namespace MockPlugin.Service.Tests
{
    [TestClass]
    public class ProcessWorkOrderTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IProcessWorkOrderToInvoice, IBackOfficeData>> _backOfficeHandlers;
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
        /// Process Work Order Tests  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestProcessWorkOrder()
        {
            IProcessWorkOrderToInvoice backofficeFeatureProcessor = null;
            try
            {


                //===========================================================================================================================================================
                // Process Code
                //===========================================================================================================================================================

                backofficeFeatureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                              where backOfficeHandler.Metadata.BackOfficeId.Equals(_backOfficeCompanyData.BackOfficeId,
                                              StringComparison.CurrentCultureIgnoreCase)
                                              select backOfficeHandler.Value).FirstOrDefault();

                Assert.IsNotNull(backofficeFeatureProcessor, String.Format("{0} BackOffice to Process Work Order was not found!",_backOfficeCompanyData.BackOfficeId));

                //TODO KMS: uncomment when nuget package update for changes in contract are applied. 

                ////Begin back office session with configuration in order to make back office connections.
                //var clientResponse = backofficeFeatureProcessor.BeginSession(_backOfficeCompanyData);

                //Assert.IsNotNull(clientResponse, String.Format("BeginSession Failed for Back Office {0}.",_backOfficeCompanyData.BackOfficeId));
                //Assert.AreNotEqual(Status.Failure, clientResponse.Status, "Failure returned from BeginSession");

                var initDefaultProperties = backofficeFeatureProcessor as IBackOfficeFeaturePropertyHandler;
                if (initDefaultProperties != null)
                {
                    var defaultPropertyValues = new Dictionary<string, object>();
                    //TODO: by back office - implement testing default value scenarios -  See MockPlugin.SyncCustomersTests
                    initDefaultProperties.Initialize(new ReadOnlyDictionary<string, object>(defaultPropertyValues));
                }

                var workOrder = CreateWorkOrder();

                var response = backofficeFeatureProcessor.ProcessWorkOrder(workOrder);
                Assert.IsNotNull(response, "Null response from ProcessWorkOrder");
                Assert.AreNotEqual(Status.Failure, response.Status, "Failure status returned from ProcessWorkOrder.");

                Assert.IsNotNull(workOrder, "workOrder came back as null");
                Assert.IsTrue(workOrder.Details.Any());
                Assert.AreEqual(2, workOrder.Details.Count);

                //TODO by backoffice plugin developer:  validation checks on data.  

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
                    //TODO KMS by connector team: fix nuget package for change in contract
                   // backofficeFeatureProcessor.EndSession();
                }
            }

        }

        /// <summary>
        /// TODO by backoffice plugin developer:  Need to set up data that the erp can process based on workOrder contract. 
        /// </summary>
        /// <returns></returns>
        private WorkOrder CreateWorkOrder()
        {
            var workOrder = new WorkOrder
            {
                Id = Guid.NewGuid().ToString(),
                Customer = new ExternalReference
                {
                    ExternalId = "1001",
                    Id = Guid.NewGuid().ToString()
                },
                Description = "WorkOrder Desc",
                Location = new WorkOrderAddress
                {
                    Street1 = "1234 Street 1",
                    Street2 = "Ste. 104",
                    City = "Irvine",
                    StateProvince = "CA",
                    PostalCode = "92618",
                    Country = "USA",
                },
                Contact = new WorkOrderContact
                {
                    FirstName = "contactFirst",
                    LastName = "contactLast"
                },
                ApprovedDate = DateTime.UtcNow,
                Approver = new WorkOrderContact
                {
                    FirstName = "contactFirst",
                    LastName = "contactLast"
                },
                Payment = new WorkOrderPayment
                {
                    AmountPaid = 100m,
                    AuthorizationCode = "auth",
                    CreditCardLast4 = "1234",
                    ExpirationMonth = "01",
                    ExpirationYear = "2018",
                    //TODO KMS by Connector Team: regenerate nuget package for service to include changes to contract
                    //PaymentMethod = ServicePaymentMethod.CreditCard,
                    PaymentReference = "deposit 100"

                },
                BillingEmail = "billing@email.com",
                CompletedDate = DateTime.UtcNow.AddDays(30),
                PONumber = "PO1234",
                Note = "Note for this work order to invoice",
                ServiceEndDate = DateTime.UtcNow.AddDays(-20),
                SubTotal = 1000m,
                Recipient = "Joe Recipient",
                Tax = 10m,
                QuickCode = "WO",
                TaxCalcProvider = TaxCalcProvider.Sage,
                Total = 1300m,
                ServiceDate = DateTime.UtcNow.AddDays(10),
                TaxSchedule = new ExternalReference
                {
                    Id = Guid.NewGuid().ToString(),
                    ExternalId = "11",

                }


            };

            for (var i = 0; i < 2; i++)
            {
                var detail = new WorkOrderDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = "detail " + i,
                    Note = "Detail note",
                    Quantity = i,
                    Discount = 4m,
                    Total = 500m,
                    IsRateOverridden = false,
                    Rate = 10,
                    ServiceType = new ExternalReference
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture) // just making up external id 
                    },
                    ShortDescription = "dtl" + i,
                    Taxable = true,
                    UnitOfMeasure = "hours"
                };
                workOrder.Details.Add(detail);

            }
            return workOrder;

        }
    }
}
