using System.ComponentModel.Composition;
using System.Globalization;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Taxes.Contracts.BackOffice;
using Sage.Connector.Taxes.Contracts.Data;

namespace Sage.Connector.MockPlugin.Taxes
{
    /// <summary>
    /// Mock backoffice plugin for Sync TaxCodes. 
    /// </summary>
    [Export(typeof(ISyncTaxCodes))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class SyncTaxCodesFeature : ISyncTaxCodes
    {
        private static int _count;


        /// <summary>
        /// Initialize the Tax Codes to get ready to start processing the sync.
        /// This could be something like loading the tax code business object to get 
        /// ready to process.  
        /// </summary>
        /// <returns><see cref="Response"/> A response status of failure will end the sync feature immediately.  </returns>
        public Response InitializeSyncTaxCodes()
        {
            _count = 0;
            return new Response { Status = Status.Success };
        }


        /// <summary>
        /// Get the next TaxCode that needs to be sync'd up to the cloud
        /// </summary>
        /// <returns></returns>
        public TaxCode GetNextSyncTaxCode()
        {
            // Return null when there isn't any more of the entity to Get.  
            // For this sample data, we are limiting the number of sync'd entities to 5
           

            if (_count == 5)
            {
                return null;
            }

            _count++;
            //Makind up external id
            string extId = (10 + _count).ToString(CultureInfo.CurrentCulture);


            var taxCode = new TaxCode
            {
                ExternalId = extId,
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                Description = extId + "Description",
                ShortDescription = extId + "Desc",
                MinimumTax = decimal.Zero,
                MaximumTax = 100000m + _count
                

            };
            var detail = new TaxCodeDetail
            {
                ExternalId = extId, // need something more unique if multiple contacts
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                Rate = int.Parse(extId)

            };
            taxCode.TaxCodeDetails.Add(detail);

            //making up a the tax code class ext id for a customer tax class
            string cExtId = "C" + extId;
            TaxCodeClass custClass = new TaxCodeClass
            {
                ExternalId = cExtId,// need something more unique if multiple addresses
                ExternalReference = cExtId,
                EntityStatus = EntityStatus.Active,
                ClassType = TaxClassTypes.Customer,
                Description = "CustClass " + cExtId,
                Sequence = 0,
                TaxClass = cExtId

            };
            taxCode.TaxCodeClasses.Add(custClass);

            //making up a the tax code class ext id for a item tax class
            cExtId = "I" + extId;
            TaxCodeClass itemClass = new TaxCodeClass
            {
                ExternalId = extId,// need something more unique if multiple addresses
                ExternalReference = cExtId,
                EntityStatus = EntityStatus.Active,
                ClassType = TaxClassTypes.Customer,
                Description = "ItemClass " + cExtId,
                Sequence = 1,
                TaxClass = cExtId

            };
            taxCode.TaxCodeClasses.Add(itemClass);


            return (taxCode);
        }


        /// <summary>
        /// Begin a login session to access the back office using the configuration provided
        /// </summary>
        /// <param name="sessionContext"><see cref="ISessionContext"/></param>
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
