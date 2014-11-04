﻿using System;
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
    /// Mock backoffice plugin for Sync InvoiceBalances. 
    /// </summary>
    [Export(typeof(ISyncInvoiceBalances))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class SyncInvoiceBalanceBalancesFeature : ISyncInvoiceBalances
    {
        private int _count;


        /// <summary>
        /// Initialize the Sync Invoice Balances to get ready to start processing the selection.
        ///             This could be something like loading the invoice business object to get 
        ///             ready to process.  
        /// </summary>
        /// <returns>
        /// <see cref="T:Sage.Connector.DomainContracts.Responses.Response"/> A response status of failure will end the sync feature immediately.  
        /// </returns>
        public Response InitializeSyncInvoiceBalances()
        {
            _count = 0;
            //no op because no feature configuration properties
            return new Response { Status = Status.Success };

        }

        /// <summary>
        /// Get the next InvoiceBalance that needs to be sync'd up to the cloud
        /// </summary>
        /// <returns></returns>
        public InvoiceBalance GetNextSyncInvoiceBalance()
        {
            // Return null when there isn't any more of the entity to Get.  
            // For this sample data, we are limiting the number of sync'd entities to 5

            if (_count == 3)
            {
                return null;
            }

            _count++;


            string extId = (1000 + _count).ToString(CultureInfo.CurrentCulture);

            // Data is completely made up.  There aren't any standards to follow except to populate the data
            // according to the specific back office
            var invoiceBalance = new InvoiceBalance
            {
                ExternalId = extId,
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
              
                Invoice = extId,
                Customer = (1000 + _count).ToString(CultureInfo.CurrentCulture),

                DiscountDueDate = DateTime.UtcNow,
                InvoiceDate = DateTime.UtcNow,
                InvoiceDueDate = DateTime.UtcNow.AddDays(20 + _count),
                Balance = 100m,
                PaymentDiscount = 10m,
                InvoiceNumber = "Inv" + extId,
             
                Total = 540m,

            };


            return (invoiceBalance);
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