﻿using System.ComponentModel.Composition;
using System.Globalization;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Sales.Contracts.BackOffice;
using Sage.Connector.Sales.Contracts.Data;

namespace Sage.Connector.MockPlugin.Sales
{
    /// <summary>
    /// Mock backoffice plugin for Sync SalespersonCustomers. 
    /// </summary>
    [Export(typeof(ISyncSalespersonCustomers))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class SyncSalespersonCustomerCustomersFeature : ISyncSalespersonCustomers
    {
        private int _count;

        /// <summary>
        /// Initialize the SyncSalespersonCustomers to get ready to start processing the sync.
        /// This could be something like loading the salespersonCustomer business object to get 
        /// ready to process. 
        /// </summary>
        /// <returns><see cref="Response"/> A response status of failure will end the sync feature immediately.  </returns>
        public Response InitializeSyncSalespersonCustomers()
        {
            _count = 0;
            return new Response { Status = Status.Success };
        }

        /// <summary>
        /// Get the next SalespersonCustomer that needs to be sync'd up to the cloud
        /// </summary>
        /// <returns></returns>
        public SalespersonCustomer GetNextSyncSalespersonCustomer()
        {
            // Return null when there isn't any more of the entity to Get.  
            // For this sample data, we are limiting the number of sync'd entities to 5

            if (_count == 5)
            {
                return null;
            }

            _count++;

            string extId = (1000 + _count).ToString(CultureInfo.CurrentCulture);

            var salespersonCustomer = new SalespersonCustomer
            {
                ExternalId = extId,
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                Salesperson = extId,
                Customer = (1000 + _count).ToString(CultureInfo.CurrentCulture)
            };


            return (salespersonCustomer);
        }



        /// <summary>
        /// Begin a login session to access the back office using the configuration provided
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="backOfficeCompanyData"><see cref="IBackOfficeCompanyData"/></param>
        /// <returns>Response containing status </returns>
        public Response BeginSession(ISessionContext sessionContext,  IBackOfficeCompanyData backOfficeCompanyData)
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
