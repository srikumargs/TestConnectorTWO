using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Invoices.Contracts.BackOffice;
using Sage.Connector.Invoices.Contracts.Data;

namespace Sage.Connector.MockPlugin.Invoices
{

    /// <summary>
    /// Mock backoffice plugin for Sync Invoices. 
    /// </summary>
    [Export(typeof(ISyncInvoices))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class SyncInvoicesFeature : ISyncInvoices
    {
        private int _count;
        private int _startingCount;
        private IDictionary<string, object> _procesingPropBag;


        /// <summary>
        /// Initialize the GetInvoices to get ready to start processing the selection.
        ///             This could be something like loading the invoice business object to get 
        ///             ready to process.  
        /// </summary>
        /// <param name="processingPropBag">Processing Property Bag is a writable dictionary to persist property value pairs 
        ///             on a local machine store for the back office company for this feature. 
        ///             Because this is historical data (ex 2 years worth), these values can help with knowing which invoices to get on 
        ///             the next run, which can avoid upload of the same records more than once.  If for some reason a duplicate is sent, 
        ///             the sync mechanism will exclude it. 
        ///             Initially, the property bag is empty.  It is up to the process to set property information for the last 
        ///             invoice information processed to be used on the next run.    
        ///             As many property value pairs as desired can be used, the amount of data should not be large, meaning don't store
        ///             the set of 2 year history invoices in the property bag. </param>
        /// <returns>
        /// <see cref="T:Sage.Connector.DomainContracts.Responses.Response"/> A response status of failure will end the sync feature immediately.  
        /// </returns>
        public Response InitializeSyncInvoices(IDictionary<string, object> processingPropBag)
        {
            //no default property values for this sample invoice sync

            //using property bag to get the next counts
            _procesingPropBag = processingPropBag;
            _startingCount =0;
            object startCount;
            processingPropBag.TryGetValue("LastCount", out startCount);
            if (null != startCount)
                _startingCount = Convert.ToInt32(startCount);
            _count = 0;
            return new Response { Status = Status.Success };

        }

        /// <summary>
        /// Get the next Invoice that needs to be sync'd up to the cloud
        /// </summary>
        /// <returns></returns>
        public Invoice GetNextSyncInvoice()
        {
            // Return null when there isn't any more of the entity to Get.  
            // For this sample data, we are limiting the number of sync'd entities to 3

            if (_count == 3)
            {
                //processing 3 at a time for smaller payloads -- mock sample data only
                _procesingPropBag.Add("LastCount", _startingCount);
                _procesingPropBag.Add("LastProcessDate", DateTime.UtcNow);
                return null;
            }

            _count++;
            _startingCount++;
            //Just a sample that you could keep track of the last invoice processed

            string extId = (1000 + _startingCount).ToString(CultureInfo.CurrentCulture);

            // Data is completely made up.  There aren't any standards to follow except to populate the data
            // according to the specific back office
            var invoice = new Invoice
            {
                ExternalId = extId,
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                BillToCity = "Irvine",
                BillToCountry = "USA",
                BillToEmail = @"billTo@" + extId + ".com",
                BillToFirstName = "First" + extId,
                BillToLastName = "Last" + extId,
                BillToName = "Name" + extId,
                BillToPhone = "555" + extId + "999",
                BillToPostalCode = "9" + extId,
                BillToStateProvince = "CA",
                Comments = "Comments for " + extId,
                BillToStreet1 = extId + " Street 1",
                BillToStreet2 = "Street 2",
                BillToStreet3 = "Street 3",
                BillToStreet4 = "Street 4",
                Customer = (1000 + _count).ToString(CultureInfo.CurrentCulture),
                CustomerExternalReference = (1000 + _count).ToString(CultureInfo.CurrentCulture),
                Discount = _count * 10,
                DiscountDueDate = DateTime.UtcNow,
                Fob = "Fob",
                DocumentSource = DocumentSource.SalesOrder,
                Freight = 10m,
                InvoiceDate = DateTime.UtcNow,
                InvoiceNumber = "Inv" + extId,
                Miscellaneous = Decimal.Zero,
                InvoiceDueDate = DateTime.UtcNow.AddDays(20 + _count),
                NonTaxableAmt = 200m,
                OrderDate = DateTime.UtcNow.AddDays(-7),
                OrderNumber = "Order " + extId,
                PoNumber = "PO " + extId,
                SalespersonName = "1000" + _count + " Name",
                SalespersonExternalReference = "1000" + _count,
                ShipDate = DateTime.UtcNow.AddDays(20),
                ShipToCity = "Beaverton",
                ShipToCountry = "USA",
                ShipToEmail = "shipto@" + extId + ".com",
                ShipToFirstName = "ShipFirst" + extId,
                ShipToLastName = "ShipLast" + extId,
                ShipToName = "ShipName" + extId,
                ShipToPhone = "555" + extId + "888",
                ShipToPostalCode = "8" + extId,
                ShipToStateProvince = "OR",
                ShipToStreet1 = extId + " Ship Street 1",
                ShipToStreet2 = "Ship Street 2",
                ShipToStreet3 = "Ship Street 3",
                ShipToStreet4 = "Ship Street 4",
                ShipVia = "UPS",
                SubTotal = 500m,
                TaxableAmt = 300m,
                Taxes = 30m,
                Terms = "10net30",
                Total = 540m,
                Type = DocumentType.Invoice
            };

            for (var i = 0; i < _count%2 + 1; i++)
            {
                var invcDetail = new InvoiceDetail
                {
                    Comment = "Comment " + extId + " " + i,
                    Discount = 5m,
                    Total = 250m,
                    EntityStatus = EntityStatus.Active,
                    ExternalId = extId + "-" + i,
                    ExternalReference = extId + "-" + i,
                    Item = (1000 + i).ToString(CultureInfo.CurrentCulture),
                    ItemDescription = "Item desc for " + (1000 + i).ToString(CultureInfo.CurrentCulture),
                    ItemNumber = (1000 + i).ToString(CultureInfo.CurrentCulture),
                    LineItemType = LineItemType.InventoryItem,
                    Price = 200m,
                    Quantity = i,
                    QuantityBackOrdered = Decimal.Zero,
                    QuantityShipped = i,
                    QuantityShippedBaseUom = i,
                    UnitOfMeasure = "Ea",
                    Warehouse = "whse" + extId
                };


                invoice.Details.Add(invcDetail);
            }




            return (invoice);
        }


        /// <summary>
        /// Begin a login session to access the back office using the configuration provided
        /// </summary>
        /// <param name="sessionContext"><see cref="ISessionContext"/></param>
        /// <param name="backOfficeCompanyData"><see cref="IBackOfficeCompanyData"/></param>
        /// <returns>Response containing status </returns>
        public Response BeginSession(ISessionContext sessionContext, IBackOfficeCompanyData backOfficeCompanyData)
        {
            /* TODO by BackOffice:  Log into back office system such that when action is called
             * TODO:                the back office can use the login session to process the request.
             * 
             * TODO TO Be Developed by Connector: Feature Configurations Property value pairs will be sent in with the configuration. 
             * TODO by Back Office:  When that happens, the values can be used when the request is called.  
             * TODO:                  So, the property value pairs configuration  or the entiry back office configuration would 
             * TODO:                  need to be stored off in a module-level variable for later use. 
             */
            return new Response { Status = Status.Success };
        }

        /// <summary>
        /// end the Back office login session
        /// </summary>
        public void EndSession()
        {
            /* TODO by BackOffice:  Close the back office Login session            */

        }
    }
}
