
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Service.Contracts.BackOffice;
using Sage.Connector.Service.Contracts.BackOffice.Responses;
using Sage.Connector.Service.Contracts.Data;

namespace Sage.Connector.MockPlugin.Service
{
    /// <summary>
    /// Mock backoffice plugin to process quote.
    /// </summary>
    [Export(typeof(IProcessWorkOrderToInvoice))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class ProcessWorkOrderToInvoiceFeature : IProcessWorkOrderToInvoice
    {
        /// <summary>
        /// Initialize the Process WorkOrder
        /// </summary>
        /// <param name="defaultPropertyValues">ReadOnly dictionary of default property values</param>
        /// <returns>
        /// <see cref="T:Sage.Connector.DomainContracts.Responses.Response"/> A response status of failure stop the feature from processing  
        /// </returns>
        public Response InitializeProcessWorkOrder(IDictionary<string, object> defaultPropertyValues)
        {
            //no op because no feature configuration properties
            return new Response {Status = Status.Success};
        }

        /// <summary>
        /// Process the Quote to an Order
        /// </summary>
        public WorkOrderToInvoiceResponse ProcessWorkOrder(WorkOrder workOrder)
        { 
            var workOrderToInvoiceResponse = new WorkOrderToInvoiceResponse();
            try
            {
                //TODO by BackOffice: Create the order from quote information using the back office 
                //TODO:                 login session created from BeginSession call.

                //Making up ExternalId for the mock 
                //TODO by BackOffice:  Set external ids to that of the newly created ERP Order

                workOrderToInvoiceResponse.DocumentReference = "invoice document Ref"; 

            }
            catch (Exception ex)
            {

                workOrderToInvoiceResponse.Status = Status.Failure;
                workOrderToInvoiceResponse.Diagnoses = new Diagnoses
                {
                    new Diagnosis
                    {
                        Severity = Severity.Error,
                        UserFacingMessage = "Unexpected Error processing quote to order.",
                        RawMessage = ex.Message + ex.StackTrace
                    }
                };
                return workOrderToInvoiceResponse;
            }

            workOrderToInvoiceResponse.Status = Status.Success;
            return workOrderToInvoiceResponse;

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
