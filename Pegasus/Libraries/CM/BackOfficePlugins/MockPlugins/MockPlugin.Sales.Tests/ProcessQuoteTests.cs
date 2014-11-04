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
    public class ProcessQuoteTests : CatalogInitialization
    {
        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IProcessQuote, IBackOfficeData>> _backOfficeHandlers;
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
        /// Process Quote test  
        /// This is a happy path test, meaning null data or response status of failure will cause the test to fail.
        /// </summary>
        [TestMethod]
        public void TestProcessQuote()
        {
            IProcessQuote backofficeFeatureProcessor = null;
            try
            {


                //===========================================================================================================================================================
                // Process Code
                //===========================================================================================================================================================

                backofficeFeatureProcessor = (from backOfficeHandler in _backOfficeHandlers
                                              where backOfficeHandler.Metadata.BackOfficeId.Equals(_backOfficeCompanyData.BackOfficeId,
                                              StringComparison.CurrentCultureIgnoreCase)
                                              select backOfficeHandler.Value).FirstOrDefault();

                Assert.IsNotNull(backofficeFeatureProcessor, String.Format("{0} BackOffice to Process Quote was not found!",_backOfficeCompanyData.BackOfficeId));


                //Begin back office session with configuration in order to make back office connections.
                var clientResponse = backofficeFeatureProcessor.BeginSession(_sessionContext,_backOfficeCompanyData);

                //because the Mock back office Process Quote has a default property value, create a dictionary
 
                var initDefaultProperties = backofficeFeatureProcessor as IBackOfficeFeaturePropertyHandler;
                if (initDefaultProperties != null)
                {
                    //TODO: by back office developer - default value testing scenarios
                    var defaultPropertyValues = new Dictionary<string, object>();
                    defaultPropertyValues.Add("ExpiryFromDays", 120);
                    initDefaultProperties.Initialize(new ReadOnlyDictionary<string, object>(defaultPropertyValues));
                }

                Assert.IsNotNull(clientResponse, String.Format("BeginSession Failed for Back Office {0}.",_backOfficeCompanyData.BackOfficeId));
                Assert.AreNotEqual(Status.Failure, clientResponse.Status, "Failure returned from BeginSession");

                var quote = CreateQuote();

                var response = backofficeFeatureProcessor.ProcessQuote(quote);
                Assert.IsNotNull(response, "Null response from ProcessQuote");
                Assert.AreNotEqual(Status.Failure, response.Status, "Failure status returned from ProcessQuote.");

                Assert.IsNotNull(quote, "quote came back as null");

                //Expecting same count in details as delivered. 
                Assert.AreEqual(5, quote.Details.Count);


                //TODO by backoffice plugin developer:  validation checks on data.  

                Assert.AreEqual(516m, quote.DocumentTotal);
                Assert.AreEqual(6m, quote.SandH);
                Assert.AreEqual(500m, quote.SubTotal);
                Assert.AreEqual(10m, quote.Tax);
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
        /// TODO by backoffice plugin developer:  Need to set up data that the erp can process based on quote contract. 
        /// </summary>
        /// <returns></returns>
        private Quote CreateQuote()
        {
            var quote = new Quote
            {
                Customer = new ExternalReference
                {
                    //TODO  by backoffice plugin developer: needs to be an actual back office key to customer
                    ExternalId = "1001"
                },

                DocumentNumber = 1,
                Description = "Quote Desc",
                EntityStatus = EntityStatus.Active,
                //TODO: Setting to utc date, but may actually be local.. Requires conversation among connector/platform team
                SubmittedDate = DateTime.UtcNow,
                ShippingAddress = new Address
                {
                    //TODO: ask back office if they need an external id for address in this sales domain process quote contract
                    Street1 = "1234 Street 1",
                    Street2 = "Ste. 104",
                    City = "Irvine",
                    StateProvince = "CA",
                    PostalCode = "92618",
                    Country = "USA",
                    Name = "Irvine Office",
                    Contact = new AddressContact
                    {
                        //TODO: ask back office if they need an external id for contact in this sales domain process quote contract
                        FirstName = "Joe",
                        LastName = "Smith",
                        EmailWork = "js@sage.com",
                        PhoneWork = "949.555.1212"
                    }
                }

            };


            for (var i = 0; i < 5; i++)
            {
                var detail = new QuoteDetail
                {
                    InventoryItem = new ExternalReference
                    {
                        //TODO by back office plugin developer: 
                        //TODO: create back office erp external id related to some actual inventory item 
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture)
                    },
                    Quantity = i
                };
                quote.Details.Add(detail);

            }
            return quote;

        }
    }
}
