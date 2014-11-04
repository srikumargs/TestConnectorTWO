using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sage.Connector.Configuration.Contracts.Data;
using Sage.Connector.Configuration.Contracts.Data.DataTypes;
using Sage.Connector.Configuration.Contracts.Data.Responses;
using Sage.Connector.Configuration.Contracts.Data.SelectionValueTypes;
using Sage.Connector.Configuration.Mediator.JsonConverters;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainMediator.Core;
using Sage.Connector.DomainMediator.Core.JsonConverters;
using Sage.Connector.DomainMediator.Core.Utilities;
using Sage.Connector.ProcessExecution.Events;
using Sage.Connector.Sales.Contracts.Data;
using Sage.Connector.Service.Contracts.Data;
using Sage.Connector.Statements.Contracts.Data.Requests;
using System;
using System.AddIn.Hosting;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using SalesPaymentMethod = Sage.Connector.Sales.Contracts.Data.PaymentMethod;
using ServicePaymentMethod = Sage.Connector.Service.Contracts.Data.PaymentMethod;
using TaxCalcProvider = Sage.Connector.Service.Contracts.Data.TaxCalcProvider;

namespace Connector.DomainMediator.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class DomainMediatorUnitTests : AbstractDomainTest
    {

        /// <summary>
        /// Property Definition Test 
        /// </summary>
        [TestMethod]
        public void PropertyDefinitionTest()
        {

            KeyValuePair<String, string> pair = new KeyValuePair<string, string>();
            Assert.IsNull(pair.Key);
            Assert.IsNull(pair.Value);

        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Setup_GetBackOfficeFeatureConfigurations()
        {
            var executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Request_GetBackOfficeFeatureConfigurationsResponse, "Mock", "GetFeatureConfigurationProperties", "");
        }


        private void Request_GetBackOfficeFeatureConfigurationsResponse(object sender, ResponseEventArgs e)
        {
            Console.WriteLine("Host: Work progressing: {0}", e.Payload);

            //We're going to stop the add-in if it ever reports progress > 50%
            Debug.Print("Host: Request Response processing: {0}", e.Payload);


            var cfg = new DomainMediatorJsonSerializerSettings();
            cfg.ContractResolver = new DictionaryFriendlyContractResolver();
            cfg.Converters.Add(new KeyValuePairConverter());
            cfg.Converters.Add(new AbstractDataTypeConverter());
            cfg.Converters.Add(new AbstractSelectionValueTypesConverter());


            var featureMetaPropertyLists = JsonConvert.DeserializeObject<IList<KeyValuePair<FeatureMetadata, IList<PropertyDefinition>>>>(e.Payload, cfg);

            if (!featureMetaPropertyLists.Any())
            {
                //No setup required.
                return;
            }

            var featurePropSetups = new Dictionary<string, Dictionary<String, AbstractSelectionValueTypes>>();
            foreach (var featureSet in featureMetaPropertyLists)
            {
                var propDefinitions = (List<PropertyDefinition>)featureSet.Value;
                var propSetupDictionary = new Dictionary<String, AbstractSelectionValueTypes>();
                foreach (var propDef in propDefinitions)
                {
                    if (propDef.PropertyDataType.SelectionType.Equals(SelectionTypes.List))
                    {
                        propSetupDictionary.Add(propDef.PropertyName, new ListTypeValues());
                        continue;
                    }
                    if (propDef.PropertyDataType.SelectionType.Equals(SelectionTypes.Lookup))
                    {
                        propSetupDictionary.Add(propDef.PropertyName, new LookupTypeValues());
                    }

                    //other types unsupported for backoffice setup at this time. 
                }
                featurePropSetups.Add(featureSet.Key.Name, propSetupDictionary);
            }
            cfg = new DomainMediatorJsonSerializerSettings();
            cfg.ContractResolver = new DictionaryFriendlyContractResolver();
            cfg.Converters.Add(new KeyValuePairConverter());

            string setupFeatureConfigPayload = JsonConvert.SerializeObject(featurePropSetups, cfg);

            cfg.Converters.Add(new AbstractSelectionValueTypesConverter());
            var featurePropSetupsDeser = JsonConvert.DeserializeObject<IList<KeyValuePair<string,
                IList<KeyValuePair<String, AbstractSelectionValueTypes>>>>>(setupFeatureConfigPayload, cfg);

            var executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Request_SetupFeatureConfigurationsResponse, "Mock", "SetupCompanyFeaturePropertySelectionValues", setupFeatureConfigPayload);

        }



        private void Request_SetupFeatureConfigurationsResponse(object sender, ResponseEventArgs e)
        {
            Console.WriteLine("Host: Work progressing: {0}", e.Payload);

            //We're going to stop the add-in if it ever reports progress > 50%
            Debug.Print("Host: Request Response processing: {0}", e.Payload);

            var cfg = new DomainMediatorJsonSerializerSettings();
            cfg.ContractResolver = new DictionaryFriendlyContractResolver();
            cfg.Converters.Add(new KeyValuePairConverter());
            cfg.Converters.Add(new AbstractDataTypeConverter());
            cfg.Converters.Add(new AbstractSelectionValueTypesConverter());


            var featurePropSetupsDeser = JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<KeyValuePair<String, AbstractSelectionValueTypes>>>>>(e.Payload, cfg);

            var validateFeaturePropValuePairs = new List<KeyValuePair<string, IList<KeyValuePair<String, Object>>>>();
            foreach (var featurePropSetup in featurePropSetupsDeser)
            {
                var propValuePairs = new Dictionary<string, object>();

                foreach (var prop in featurePropSetup.Value.ToList())
                {
                    if (prop.Key.Equals("PaymentTerms"))
                    {
                        propValuePairs.Add(prop.Key, "Terms30Days");
                    }
                    else if (prop.Key.Equals("ListField"))
                    {
                        propValuePairs.Add(prop.Key, "List Item 3");
                    }

                }

                validateFeaturePropValuePairs.Add(new KeyValuePair<string, IList<KeyValuePair<String, object>>>(featurePropSetup.Key, propValuePairs.ToList()));
            }


            cfg = new DomainMediatorJsonSerializerSettings();
            cfg.ContractResolver = new DictionaryFriendlyContractResolver();
            cfg.Converters.Add(new KeyValuePairConverter());

            var validateFeatureConfigPayload =
                JsonConvert
                    .SerializeObject(validateFeaturePropValuePairs, cfg);
            var executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Setup_ValidationBackupOfficeFeatureConfigurationResponse, "Mock", "ValidateCompanyFeaturePropertyValuePairs", validateFeatureConfigPayload);

            executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Setup_SaveFeatureConfigurationResponse,
                "Mock", "SaveCompanyFeaturePropertyValuePairs", validateFeatureConfigPayload);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Setup_ValidateBackOfficeFeatureConfigurations()
        {
            //Mock BackOffice Test
            var propdefs = new Dictionary<string, ICollection<KeyValuePair<String, Object>>>();

            IDictionary<String, Object> customerDefs = new Dictionary<String, Object>();
            customerDefs.Add("PaymentTerms", "Terms30Days");
            customerDefs.Add("ListField", "List Item 3"); 
            propdefs.Add("SyncCustomers", customerDefs.ToList());

            var cfg = new DomainMediatorJsonSerializerSettings();

            string requestPayload = JsonConvert.SerializeObject(propdefs.ToList(), cfg);

            //"[{\"Key\":\"ProcessQuote\",\"Value\":[{\"Key\":\"ExpiryFromDays\",\"Value\":\"0\"}]}]"
            var executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Setup_ValidationBackupOfficeFeatureConfigurationResponse,
                "Mock", "ValidateCompanyFeaturePropertyValuePairs", requestPayload);


            executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Setup_SaveFeatureConfigurationResponse,
                "Mock", "SaveCompanyFeaturePropertyValuePairs", requestPayload);

        }

        private void Setup_ValidationBackupOfficeFeatureConfigurationResponse(object sender, ResponseEventArgs e)
        {
            Console.WriteLine("Host: Work progressing: {0}", e.Payload);

            //We're going to stop the add-in if it ever reports progress > 50%
            Debug.Print("Host: Setup_ValidationBackupOfficeFeatureConfigurationResponse processing: {0}", e.Payload);

            //now we could serialize the json payload. 
            Assert.IsFalse(String.IsNullOrWhiteSpace(e.Payload));

            var cfg = new DomainMediatorJsonSerializerSettings
            {
                ContractResolver = new DictionaryFriendlyContractResolver()
            };
            cfg.Converters.Add(new KeyValuePairConverter());

            var response = JsonConvert.DeserializeObject<IList<KeyValuePair<String, ICollection<PropertyValuePairValidationResponse>>>>(e.Payload, cfg);

            foreach (var featureValidation in response)
            {
                Debug.Print("Feature {0}", featureValidation.Key);
                foreach (var propPairResponse in featureValidation.Value)
                {
                    Debug.Print(". Key = {0}", propPairResponse.PropertyValuePair.Key);
                    Debug.Print("... Value = {0}", propPairResponse.PropertyValuePair.Value);
                    Debug.Print("... Status = {0}", propPairResponse.Status);
                    if (propPairResponse.Diagnoses.Any())
                    {
                        Debug.Print("... Diagnoses message = {0}", propPairResponse.Diagnoses[0].UserFacingMessage);
                    }
                }
            }



        }

        private void Setup_SaveFeatureConfigurationResponse(object sender, ResponseEventArgs e)
        {
            Console.WriteLine("Host: Work progressing: {0}", e.Payload);

            //We're going to stop the add-in if it ever reports progress > 50%
            Debug.Print("Host: Setup_SaveFeatureConfigurationResponse processing: {0}", e.Payload);

            string storageIdentifer = MockTestTenantId + "_SyncCustomers";
            using (var storageDictionary = new StorageDictionary<String, Object>(ProcessExecutionPath,
                          storageIdentifer))
            {
                object paymentTermsDefault = null;
                storageDictionary.TryGetValue("PaymentTerms", out paymentTermsDefault);
                Assert.IsNotNull(paymentTermsDefault);
                Assert.AreEqual("Terms30Days", paymentTermsDefault.ToString());
                object listfielddefault = null;
                storageDictionary.TryGetValue("ListField", out listfielddefault);
                Assert.IsNotNull(listfielddefault);
                Assert.AreEqual("List Item 3", listfielddefault.ToString());


            }

        }

        [TestMethod]
        public void Test_CanSerializeAndDeserializeDictionaryPropertyDefinitions()
        {
            //Mock BackOffice Test
            var propdefsSer = new Dictionary<string, ICollection<KeyValuePair<PropertyDefinition, String>>>();

            var propdefsDeSer = new Dictionary<string, Dictionary<PropertyDefinition, String>>();

            IDictionary<PropertyDefinition, String> quoteDefs = new Dictionary<PropertyDefinition, String>();
            quoteDefs.Add(new PropertyDefinition("MockTerms", "Mock Terms", "Desc mock terms",
                new StringType { SelectionType = SelectionTypes.Lookup }, false), "Terms30Days");
            propdefsSer.Add("IProcessQuote", quoteDefs.ToList());

            var cfg = new DomainMediatorJsonSerializerSettings();

            cfg.ContractResolver = new DictionaryFriendlyContractResolver();
            cfg.Converters.Add(new KeyValuePairConverter());



            //var serdata = propdefs.ToList();

            var serdata = quoteDefs.ToList();
            string json = JsonConvert.SerializeObject(serdata, cfg);

            cfg.Converters.Add(new AbstractDataTypeConverter());

            quoteDefs = JsonConvert.DeserializeObject<ICollection<KeyValuePair<PropertyDefinition, String>>>(json, cfg).ToDictionary(x => x.Key, x => x.Value);
            cfg = new DomainMediatorJsonSerializerSettings();

            cfg.ContractResolver = new DictionaryFriendlyContractResolver();
            cfg.Converters.Add(new KeyValuePairConverter());


            var propserdata = propdefsSer.ToList();
            json = JsonConvert.SerializeObject(propserdata, cfg);

            cfg.Converters.Add(new AbstractDataTypeConverter());

            var propDefsDeserList = JsonConvert.DeserializeObject<IList<KeyValuePair<String, ICollection<KeyValuePair<PropertyDefinition, String>>>>>(json, cfg);

            propdefsDeSer = propDefsDeserList.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value));





        }
        [TestMethod]
        public void Test_CanSerializeAndDeserializeDictionaryOfStrings()
        {
            //Mock BackOffice Test
            var propdefsSer = new Dictionary<string, IList<KeyValuePair<String, String>>>();
            ;
            var propdefsDeSer = new Dictionary<string, Dictionary<String, String>>();

            IDictionary<String, String> quoteDefs = new Dictionary<String, String>();
            quoteDefs.Add("MockTerms", "Terms30Days");
            propdefsSer.Add("IProcessQuote", quoteDefs.ToList());

            var cfg = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto
            };
            cfg.ContractResolver = new DictionaryFriendlyContractResolver();
            //  cfg.Converters.Add(new PropertyDefinitionConverter());
            cfg.Converters.Add(new KeyValuePairConverter());
            // cfg.Converters.Add(new DictionaryConverter());


            //var serdata = propdefs.ToList();

            var serdata = quoteDefs.ToList();
            string json = JsonConvert.SerializeObject(serdata, cfg);

            quoteDefs = JsonConvert.DeserializeObject<IList<KeyValuePair<String, String>>>(json, cfg).ToDictionary(x => x.Key, x => x.Value);


            var propserdata = propdefsSer.ToList();
            json = JsonConvert.SerializeObject(propserdata, cfg);

            var propDefsDeserList = JsonConvert.DeserializeObject<IList<KeyValuePair<String, IList<KeyValuePair<String, String>>>>>(json, cfg);

            propdefsDeSer = propDefsDeserList.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value));
        }



        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Setup_GetBackOfficeConfiguration()
        {
            var executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Request_ProcessResponse, null, "GetBackOfficeConfiguration", "");


        }

        //Test apply encoding and decoding 
        [TestMethod]
        public void Test_EncodingDecoding()
        {
            var quote = new Quote
            {
                ExternalId = "Quote ExtId",
                Id = Guid.NewGuid().ToString(),
                Customer = new ExternalReference
                {
                    Id = Guid.NewGuid().ToString()
                },
                Description = "Quote Desc",
                EntityStatus = EntityStatus.Active,
                SubmittedDate = DateTime.UtcNow,
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid().ToString(),
                    Street1 = "1234 Street 1",
                    Street2 = "Ste. 104",
                    City = "Irvine",
                    StateProvince = "CA",
                    PostalCode = "92618",
                    Country = "USA",
                    Name = "Irvine Office",
                    Contact = new AddressContact
                    {
                        FirstName = "Karen",
                        LastName = "Sellers",
                        EmailWork = "ks@sage.com",
                        PhoneWork = "949.555.1212"
                    }
                },
                Payment = new SalesDocumentPayment
                {
                    AmountPaid = 100m,
                    AuthorizationCode = "auth",
                    CreditCardLast4 = "1234",
                    ExpirationMonth = "01",
                    ExpirationYear = "2018",
                    PaymentMethod = SalesPaymentMethod.CreditCard,
                    PaymentReference = "deposit 100"

                }

            };

            for (var i = 0; i < 5; i++)
            {
                var detail = new QuoteDetail
                {
                    ExternalId = "QuoteDetail ExtId",
                    Id = Guid.NewGuid().ToString(),
                    EntityStatus = EntityStatus.Active,
                    InventoryItem = new ExternalReference
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture) // just making up external id 
                    },
                    Quantity = i
                };
                quote.Details.Add(detail);

            }

            ExternalIdUtilities.ApplyExternalIdEncoding(quote);

            //assert they got changed
            Assert.AreNotEqual("Quote ExtId", quote.ExternalId);

            var det = 0;
            foreach (var detail in quote.Details)
            {
                Assert.AreNotEqual("QuoteDetail ExtId", detail.ExternalId);
                Assert.AreNotEqual((1000 + det).ToString(CultureInfo.CurrentCulture), detail.InventoryItem.ExternalId);
                det++;
            }

            ExternalIdUtilities.ApplyExternalIdDecoding(quote);

            //assert they got changed back. 
            Assert.AreEqual("Quote ExtId", quote.ExternalId);

            det = 0;
            foreach (var detail in quote.Details)
            {
                Assert.AreEqual("QuoteDetail ExtId", detail.ExternalId);
                Assert.AreEqual((1000 + det).ToString(CultureInfo.CurrentCulture), detail.InventoryItem.ExternalId);
                det++;
            }


        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sales_ProcessQuote()
        {
            var quote = new Quote
            {
                Id = Guid.NewGuid().ToString(),
                Customer = new ExternalReference
                {
                    Id = Guid.NewGuid().ToString()
                },
                Description = "SalesDocument Desc",
                EntityStatus = EntityStatus.Active,
                SubmittedDate = DateTime.UtcNow,
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid().ToString(),
                    Street1 = "1234 Street 1",
                    Street2 = "Ste. 104",
                    City = "Irvine",
                    StateProvince = "CA",
                    PostalCode = "92618",
                    Country = "USA",
                    Name = "Irvine Office",
                    Contact = new AddressContact
                    {
                        FirstName = "Karen",
                        LastName = "Sellers",
                        EmailWork = "ks@sage.com",
                        PhoneWork = "949.555.1212"
                    }
                }

            };

            for (var i = 0; i < 5; i++)
            {
                var detail = new QuoteDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityStatus = EntityStatus.Active,
                    InventoryItem = new ExternalReference
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture) // just making up external id 
                    },
                    Quantity = i
                };
                quote.Details.Add(detail);

            }
            ExternalIdUtilities.ApplyExternalIdEncoding(quote);
            var payload = JsonConvert.SerializeObject(quote, new DomainMediatorJsonSerializerSettings());
            //const string quoteRequest = "{\"ExternalId\":\"1000\",\"Description\":\"Description 1\",\"QuoteNumber\":1000,\"Tax\":90.0,\"SandH\":25.0,\"QuoteTotal\":1000.0,\"SubTotal\":1000.0,\"Status\":\"Active\",\"ExpiryDate\":null,\"SubmittedDate\":\"2013-12-06T00:43:00.6311289Z\",\"Customer\":\"Customer 1\",\"DiscountPercent\":10.0}";
            ExecuteProcessRequest(null, Request_ProcessResponse, "Mock", "ProcessQuote", payload);
        }




        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sales_ProcessQuoteToOrder()
        {
            var quote = new QuoteToOrder
            {
                Id = Guid.NewGuid().ToString(),
                Customer = new ExternalReference
                {
                    Id = Guid.NewGuid().ToString()
                },
                Description = "QuoteToOrder Desc",
                EntityStatus = EntityStatus.Active,
                SubmittedDate = DateTime.UtcNow,
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid().ToString(),
                    Street1 = "1234 Street 1",
                    Street2 = "Ste. 104",
                    City = "Irvine",
                    StateProvince = "CA",
                    PostalCode = "92618",
                    Country = "USA",
                    Name = "Irvine Office",
                    Contact = new AddressContact
                    {
                        FirstName = "Karen",
                        LastName = "Sellers",
                        EmailWork = "ks@sage.com",
                        PhoneWork = "949.555.1212"
                    }
                },
                Payment = new SalesDocumentPayment
                {
                    AmountPaid = 100m,
                    AuthorizationCode = "auth",
                    CreditCardLast4 = "1234",
                    ExpirationMonth = "01",
                    ExpirationYear = "2018",
                    PaymentMethod = SalesPaymentMethod.CreditCard,
                    PaymentReference = "deposit 100"

                }

            };

            for (var i = 0; i < 5; i++)
            {
                var detail = new QuoteToOrderDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityStatus = EntityStatus.Active,
                    InventoryItem = new ExternalReference
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture) // just making up external id 
                    },
                    Quantity = i
                };
                quote.Details.Add(detail);

            }

            ExternalIdUtilities.ApplyExternalIdEncoding(quote);
            var payload = JsonConvert.SerializeObject(quote, new DomainMediatorJsonSerializerSettings());
            //const string quoteRequest = "{\"ExternalId\":\"1000\",\"Description\":\"Description 1\",\"QuoteNumber\":1000,\"Tax\":90.0,\"SandH\":25.0,\"QuoteTotal\":1000.0,\"SubTotal\":1000.0,\"Status\":\"Active\",\"ExpiryDate\":null,\"SubmittedDate\":\"2013-12-06T00:43:00.6311289Z\",\"Customer\":\"Customer 1\",\"DiscountPercent\":10.0}";
            ExecuteProcessRequest(null, Request_ProcessResponse, "Mock", "ProcessQuoteToOrder", payload);
        }





        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sales_ProcessPaidOrder()
        {
            var paidOrder = new PaidOrder
            {
                Id = Guid.NewGuid().ToString(),
                Customer = new ExternalReference
                {
                    ExternalId = "1001",
                    Id = Guid.NewGuid().ToString()
                },
                Description = "PaidOrder Desc",
                EntityStatus = EntityStatus.Active,
                SubmittedDate = DateTime.UtcNow,
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid().ToString(),
                    Street1 = "1234 Street 1",
                    Street2 = "Ste. 104",
                    City = "Irvine",
                    StateProvince = "CA",
                    PostalCode = "92618",
                    Country = "USA",
                    Name = "Irvine Office",
                    Contact = new AddressContact
                    {
                        FirstName = "Karen",
                        LastName = "Sellers",
                        EmailWork = "ks@sage.com",
                        PhoneWork = "949.555.1212"
                    }
                },
                Payment = new SalesDocumentPayment
                {
                    AmountPaid = 100m,
                    AuthorizationCode = "auth code",
                    CreditCardLast4 = "1234",
                    ExpirationMonth = "04",
                    ExpirationYear = "2017",
                    PaymentMethod = SalesPaymentMethod.CreditCard,
                    PaymentReference = "deposit 100"

                }

            };

            for (var i = 0; i < 5; i++)
            {
                var detail = new PaidOrderDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityStatus = EntityStatus.Active,
                    InventoryItem = new ExternalReference
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture) // just making up external id 
                    },
                    Quantity = i
                };
                paidOrder.Details.Add(detail);

            }

            ExternalIdUtilities.ApplyExternalIdEncoding(paidOrder);

            var payload = JsonConvert.SerializeObject(paidOrder, new DomainMediatorJsonSerializerSettings());
            ExecuteProcessRequest(null, Request_ProcessResponse, "Mock", "ProcessPaidOrder", payload);
        }




        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Statements_ProcessStatements()
        {
            var statementsRequest = new StatementsRequest { StatementDate = DateTime.UtcNow };
            for (var i = 0; i < 10; i++)
            {
                statementsRequest.CustomerReferences.Add(new ExternalReference
                {
                    ExternalId = (1000 + i).ToString(CultureInfo.CurrentCulture),
                    Id = Guid.NewGuid().ToString()
                });
            }

            ExternalIdUtilities.ApplyExternalIdEncoding(statementsRequest);
            var payload = JsonConvert.SerializeObject(statementsRequest, new DomainMediatorJsonSerializerSettings());
            ExecuteProcessRequest(null, Request_ProcessResponse, "Mock", "ProcessStatements", payload);
        }





        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Service_ProcessWorkOrderToInvoice()
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
                    PaymentMethod = ServicePaymentMethod.CreditCard,
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

            ExternalIdUtilities.ApplyExternalIdEncoding(workOrder);
            var payload = JsonConvert.SerializeObject(workOrder, new DomainMediatorJsonSerializerSettings());
            //const string quoteRequest = "{\"ExternalId\":\"1000\",\"Description\":\"Description 1\",\"QuoteNumber\":1000,\"Tax\":90.0,\"SandH\":25.0,\"QuoteTotal\":1000.0,\"SubTotal\":1000.0,\"Status\":\"Active\",\"ExpiryDate\":null,\"SubmittedDate\":\"2013-12-06T00:43:00.6311289Z\",\"Customer\":\"Customer 1\",\"DiscountPercent\":10.0}";
            ExecuteProcessRequest(null, Request_ProcessResponse, "Mock", "ProcessWorkOrderToInvoice", payload);
        }



    }




}
