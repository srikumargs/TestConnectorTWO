using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Statements.Contracts.BackOffice;
using Sage.Connector.Statements.Contracts.Data;
using Sage.Connector.Statements.Contracts.Data.Requests;
using Sage.Connector.Statements.Contracts.Data.Responses;

namespace Sage.Connector.MockPlugin.Statements
{
    /// <summary>
    /// Process Statements Feature
    /// </summary>
    [Export(typeof(IProcessStatements))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class ProcessStatementsFeature : IProcessStatements
    {
        private StatementsRequest _statementsRequest;
        private int _customerIndex = 0;

        private ISessionContext _sessionContext;
        /// <summary>
        /// Perform any initializations required for performance reasons or otherwise. 
        /// </summary>
        /// <param name="statementsRequest"><see cref="StatementsRequest"/></param>
        /// <returns><see cref=" Response"/>containing the status information.
        /// None of the statements will not be processed if a Status of Failure is returned from this call.  </returns>
        public Response InitializeProcessStatements(StatementsRequest statementsRequest)
        {
            _statementsRequest = statementsRequest;
            return new Response {Status = Status.Success};
        }


        /// <summary>
        /// Get the next statement
        /// </summary>
        /// <returns><see cref="StatementResponse"/>Set the Response Status appropriately for each statement, along with the Statement.</returns>
        public StatementResponse GetNextStatement()
        {
            var statementResponse = new StatementResponse();

            try
            {

                ExternalReference custRef = _statementsRequest.CustomerReferences.ToList()[_customerIndex];
                _customerIndex++;
               //just making up a unique statement external id base on request information
                string extId = _statementsRequest.StatementDate.ToString(CultureInfo.CurrentCulture)
                    + "-" + custRef.ExternalId;
               
                // The meat of statement response for a specific customers. 
                var statement = new Statement
                {
                    StatementDate = _statementsRequest.StatementDate,
                    Customer = custRef,
                    //fill in the rest ExternalId = extId,// need something more unique if multiple addresses
                    ExternalId = extId,
                    ExternalReference = extId,
                    EntityStatus = EntityStatus.Active,
                    CustomerName = "customer " + custRef.ExternalId,
                    CustomerCity = extId + "City",
                    CustomerCountry = "USA",
                    CustomerPostalCode = "99999",
                    CustomerStateProvince = "CA",
                    CustomerStreet1 = custRef.ExternalId + " Street1",
                    CustomerStreet2 = custRef.ExternalId + " Street2",
                    CustomerStreet3 = custRef.ExternalId + " Street3",
                    CustomerStreet4 = custRef.ExternalId + " Street4",
                    ContactName = "contact name",
                    BalanceDue = 1000m,
                    AgingBalance1 = 100m,
                    AgingBalance2 = 200m,
                    AgingBalance3 = 300m,
                    AgingBalance4 = 400m,
                    CreditAvailable = 3000m,
                    CreditLimit = 4000m,
                    CurrentBalance = 1000m,
                    CustomerNumber = "customer number",
                    OverdueMessage = "please remit payment",
                    SalesPersonName = "Suzy",
                    StandardMessage = "Use Sage Exchange",
                };

                for (var i = 0; i < 2; i++)
                {
                    string detExtId = statement.ExternalId + "-" + i;
                    var detail = new StatementDetail
                    {
                        Balance = (i + 1) * 100,
                        Description = "statement " + i,
                        ExternalId = detExtId,
                        ExternalReference = detExtId,
                        DueDate = DateTime.UtcNow.AddDays(15),
                        TransactionAmt = (i + 1) * 100,
                        Invoice = "inv" + detExtId,
                        TransactionDate = _statementsRequest.StatementDate.AddDays(-60),
                        TransactionReference = "trx reference",
                        TransactionSequence = i + 1,
                        TransactionType = "INV",
                    };

                    statement.Details.Add(detail);
                }

                statementResponse.Statement = statement;
                statementResponse.Status = Status.Success;
            }
            catch (Exception ex)
            {

                statementResponse.Status = Status.Failure;
                statementResponse.Diagnoses.Add(new Diagnosis
                {
                    Severity = Severity.Error, 
                    RawMessage = ex.Message + ex.StackTrace, 
                    UserFacingMessage = ex.Message 
                });

            }
            return statementResponse;
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

            _sessionContext = sessionContext;
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

