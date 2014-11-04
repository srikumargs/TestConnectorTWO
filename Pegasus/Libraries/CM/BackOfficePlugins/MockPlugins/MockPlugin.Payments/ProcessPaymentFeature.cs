using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Payments.Contracts.BackOffice;
using Sage.Connector.Payments.Contracts.Data;

namespace Sage.Connector.MockPlugin.Payments
{
    /// <summary>
    /// Mock backoffice plugin to process quote.
    /// </summary>
    [Export(typeof(IProcessPayment))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class ProcessPaymentFeature : IProcessPayment
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultPropertyValues"></param>
        /// <returns></returns>
        public Response InitializeProcessPayment(IDictionary<string, object> defaultPropertyValues)
        {
            //no op because no feature configuration properties
            return new Response { Status = Status.Success };

        }

        /// <summary>
        /// Process the Order
        /// </summary>
        public Response ProcessPayment(Payment payment)
        {
            try
            {

                //TODO for BackOffice: Process the payment in the back office using the back office 

                payment.ExternalId = "1001";
                payment.ExternalReference = "1001";
                payment.EntityStatus = EntityStatus.Active;

            }
            catch (Exception ex)
            {
                return new Response
                {
                    Status = Status.Failure,
                    Diagnoses = new Diagnoses
                    {
                        new Diagnosis
                        {
                            Severity = Severity.Error,
                            UserFacingMessage = "Unexpected Error processing payment.",
                            RawMessage = ex.Message + ex.StackTrace
                        }
                    }
                };
            }

            return new Response
            {
                Status = Status.Success
            };
        }


        /// <summary>
        /// Begin a login session to access the back office using the configuration provided
        /// </summary>
        /// /// <param name="sessionContext"><see cref="ISessionContext"/></param>
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
