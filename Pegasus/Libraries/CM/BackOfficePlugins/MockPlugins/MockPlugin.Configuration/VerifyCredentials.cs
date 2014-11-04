using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sage.Connector.Configuration.Contracts.BackOffice;
using Sage.Connector.Configuration.Contracts.Data;
using Sage.Connector.Configuration.Contracts.Data.Descriptions;
using Sage.Connector.Configuration.Contracts.Data.Responses;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Responses;

namespace Sage.Connector.MockPlugin.Configuration
{
    /// <summary>
    /// Get and check the needed credentials to access the back office to create connections.
    /// </summary>
    [Export(typeof(IVerifyCredentials))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class VerifyCredentials :IVerifyCredentials
    {
        /// <summary>
        /// Get the credentials needed to be allowed to manage a connection to the back office.
        /// </summary>
        /// <returns></returns>
        public CompanyManagementCredentialsNeededResponse GetCompanyConnectionManagementCredentialsNeeded(ISessionContext sessionContext)
        {
           

            var retval = new CompanyManagementCredentialsNeededResponse();

            //use real descriptions based on the description system we have UX for it.
            retval.Descriptions = new Dictionary<string, object>();
            retval.Descriptions[CommonPropertyKeys.UserId] = new SimpleStringDescriptionBuilder
                                                                {
                                                                    DisplayName = "User Id",
                                                                }.ToDescription();
            
            retval.Descriptions[CommonPropertyKeys.Password] = new SimpleStringDescriptionBuilder
                                                                {
                                                                    DisplayName = "Password",
                                                                    IsPassword = true
                                                                }.ToDescription();

            retval.Descriptions[CommonPropertyKeys.BackOfficeModeAdminKey] = "FooDescription";

            //Default values for the credentials.
            //note the addition of a "hidden" key that round trips without being shown in current UI.
            //This approach will allow plugs to specify values the need until the new configuration UI is on line.
            //once the new configuration UI is on line values the only changes needed should in this method.
            retval.CurrentValues = new Dictionary<string, string>();
            retval.CurrentValues[CommonPropertyKeys.UserId] = "admin";
            retval.CurrentValues[CommonPropertyKeys.Password] = "1";
            retval.CurrentValues[CommonPropertyKeys.BackOfficeModeAdminKey] = "FooValue";

            //note that until the new configuration UX is in place the current hard coded UX will 
            //check for the existence of the userId and Password descriptions and will prefill the return the updated values if present.
            //if descriptions are provided for those items. Ux will still show if these are not present for now.

            retval.Status = Status.Success;
            return retval;
        }

        /// <summary>
        /// Validate the credentials needed to manage a back office connection.
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="companyManagementCredentials"></param>
        /// <returns></returns>
        public ValidateCompanyConnectionManagementCredentialsResponse ValidateCompanyConnectionManagementCredentials(ISessionContext sessionContext, IDictionary<string, string> companyManagementCredentials)
        {
            var retval = new ValidateCompanyConnectionManagementCredentialsResponse();

            if (companyManagementCredentials != null)
            {
                string userId;
                companyManagementCredentials.TryGetValue(CommonPropertyKeys.UserId, out userId);
                userId = userId ?? string.Empty;

                string password;
                companyManagementCredentials.TryGetValue(CommonPropertyKeys.Password, out password);
                password = password ?? string.Empty;

                string foo;
                companyManagementCredentials.TryGetValue(CommonPropertyKeys.BackOfficeModeAdminKey, out foo);
                foo = foo ?? string.Empty;

                if (userId.Equals("admin", StringComparison.OrdinalIgnoreCase)
                    && password.Equals("1")
                    && foo.Equals("FooValue")
                    )
                {
                    retval.Status = Status.Success;
                }
            }
            
            if (retval.Status != Status.Success)
            {
                String msg = "Invalid company connection credentials";
                retval.Status = Status.Failure;
                retval.Diagnoses = new Diagnoses { new Diagnosis { RawMessage = msg, UserFacingMessage = msg, Severity = Severity.Error } };
                
                //example of logging
                sessionContext.Logger.Write(this, LogLevel.Warning, "Invalid Credentials Presented");
            }
            
            retval.Credentials = companyManagementCredentials;
            
            return retval;
        }

        /// <summary>
        /// Get the credentials needed to access a specific company.
        /// includes the list of available companies if relevant to the back office
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="companyCredentials"></param>
        /// <returns></returns>
        public CompanyConnectionCredentialsNeededResponse GetCompanyConnectionCredentialsNeeded(ISessionContext sessionContext, CompanyCredentials companyCredentials)
        {

            var retval = new CompanyConnectionCredentialsNeededResponse();
            
            //pull out management credentials to check them, company check for preexisting values in an edit
            var companyManagementCredentials = companyCredentials.CompanyManagementCredentials;
            var companyConnectionCredentials = companyCredentials.CompanyConnectionCredentials;
            
            var checkManagementCredentials = ValidateCompanyConnectionManagementCredentials(sessionContext, companyManagementCredentials);
            if (checkManagementCredentials.Status == Status.Success)
            {
                //use real descriptions based on the description system we have UX for it.
                retval.Descriptions = new Dictionary<string, object>();
                retval.Descriptions[CommonPropertyKeys.UserId] = new SimpleStringDescriptionBuilder
                                                                    {
                                                                        DisplayName = "User Id",
                                                                    }.ToDescription();

                retval.Descriptions[CommonPropertyKeys.Password] = new SimpleStringDescriptionBuilder
                                                                    {
                                                                        DisplayName = "Password",
                                                                        IsPassword = true
                                                                    }.ToDescription();
                    
                retval.Descriptions[CommonPropertyKeys.CompanyId] = GetBackOfficeCompanyConnections(sessionContext, companyManagementCredentials);

                retval.Descriptions[CommonPropertyKeys.CompanyPath] = new PathStringDescriptionBuilder
                                                                    {
                                                                        DisplayName = "Company Path",
                                                                    }.ToDescription();

                //Lookup possible values in incoming connection dictionary to find values from last edit or use default
                retval.CurrentValues = new Dictionary<string, string>();
                retval.CurrentValues[CommonPropertyKeys.UserId] = NoThrowDictionaryLookup(companyConnectionCredentials,CommonPropertyKeys.UserId) ?? "User";
                retval.CurrentValues[CommonPropertyKeys.Password] = NoThrowDictionaryLookup(companyConnectionCredentials,CommonPropertyKeys.Password) ?? "password";
                retval.CurrentValues[CommonPropertyKeys.CompanyId] = NoThrowDictionaryLookup(companyConnectionCredentials,CommonPropertyKeys.CompanyId) ?? "";
                retval.CurrentValues[CommonPropertyKeys.CompanyPath] = NoThrowDictionaryLookup(companyConnectionCredentials,CommonPropertyKeys.CompanyPath) ?? @"C:\";
                retval.Status = Status.Success;
            }
            else
            {
                retval.Status = checkManagementCredentials.Status;
                retval.Diagnoses = checkManagementCredentials.Diagnoses;
            }
            
            return retval;

            //Note that if the back office uses per company administrator like S100Contractor, it is likely that the company will be specified
            //in the first Management check screen. One use of the companyManagementCredentials in addition to checking the basic admin rights
            //could be to allow the companyId specified in the first screen to be added to connection credentials property bag. In this case one would
            //not specify a description for that property and it would no be presented in the UX but would be present.

        }

        private string NoThrowDictionaryLookup(IDictionary<string, string> dictionary, string key)
        {
            string value = null;
            if(dictionary != null)
                dictionary.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// Validate Company Connection Credentials.
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        /// <remarks>
        /// One likely wants this call to be be fast. One of the easy ways of implementing OnBeginSession is to either call this method
        /// or OnBeginSession and this call common logic. Where ever located the common logic will be one of the most common area of code called.
        /// </remarks>
        public ValidateCompanyConnectionCredentialsResponse ValidateCompanyConnectionCredentials(ISessionContext sessionContext, IDictionary<string, string> credentials)
        {
            var retval = new ValidateCompanyConnectionCredentialsResponse();

            //Pass back the credentials with any needed changes
            retval.Credentials = credentials;

            if (credentials != null)
            {
                string userId;
                credentials.TryGetValue(CommonPropertyKeys.UserId, out userId);
                userId = userId ?? string.Empty;

                string password;
                credentials.TryGetValue(CommonPropertyKeys.Password, out password);
                password = password ?? string.Empty;

                string companyId;
                credentials.TryGetValue(CommonPropertyKeys.CompanyId, out companyId);
                companyId = companyId ?? string.Empty;

                if (userId.Equals("admin", StringComparison.OrdinalIgnoreCase) && password.Equals("1"))
                    retval.Status = Status.Success;

                if (userId.Equals("User", StringComparison.OrdinalIgnoreCase) && password.Equals("password")
                    && !companyId.Equals(_adminOnlyCompanyId))
                    retval.Status = Status.Success;

                if (!IsValidCompanyId(companyId))
                {
                    retval.Status = Status.Failure;
                }

                if (retval.Status == Status.Success)
                {
                    //in a non mock use a friendly "Display Name" for this.
                    retval.CompanyNameForDisplay = companyId;

                    //in a non mock this should be a company specific unique identifier for the back office.
                    //this will be used to help prevent inadvertently connecting to a company multiple times.
                    //This is expected to stay constant for a given company.
                    retval.CompanyUnqiueIdentifier = companyId;
                }
            }

            if (retval.Status != Status.Success)
            {
                String msg = "Invalid mock connection";
                retval.Status = Status.Failure;

                retval.Diagnoses = new Diagnoses {new Diagnosis {RawMessage = msg, UserFacingMessage = msg, Severity = Severity.Error}};
            }
            
            return retval;
        }

        /// <summary>
        /// special value used for setting up company that requires admin level rights in support of testing
        /// </summary>
        private readonly string _adminOnlyCompanyId = "AdminOnlyCompany";

        /// <summary>
        /// Build the list of companies to present in the UI
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private IDictionary<string, object> GetBackOfficeCompanyConnections(ISessionContext sessionContext, IDictionary<string, string> credentials)
        {
            var builder = new ThreePartListDescriptionBuilder()
            {
                DisplayName = "Company Name"
            };
            builder.Add("Company","Company Name",@"C:\DataFolder\Company");
            builder.Add("IndeterminateCompany", "Indeterminate Company", @"C:\DataFolder\Indeterminate");
            
            //special company only admin credentials will have access
            builder.Add(_adminOnlyCompanyId,"Admin Only Company",@"C:\DataFolder\AdminOnly");

            for (int compNumber = 0; compNumber < 25; compNumber++)
            {
                //check to see if cancel is requested. In this case throw. Could also check  IsCancellationRequested
                sessionContext.CancellationToken.ThrowIfCancellationRequested();

                builder.Add("Company" + compNumber,"Company " + compNumber, @"C:\DataFolder\Company" + compNumber);
            }

            var retval = builder.ToDescription();
            return retval;
        }


        private bool IsValidCompanyId(string companyId)
        {
            //build known list of company ids
            if(_knownCompanyIds == null)
            {
                _knownCompanyIds = new List<string>();
                _knownCompanyIds.Add("Company");
                _knownCompanyIds.Add("IndeterminateCompany");
                _knownCompanyIds.Add(_adminOnlyCompanyId);
                for (int compNumber = 0; compNumber < 25; compNumber++)
                {
                    _knownCompanyIds.Add("Company" + compNumber);
                }
            }

            bool retval = _knownCompanyIds.Contains(companyId);
            return retval;
        }

        /// <summary>
        /// cache list if known company ids. 
        /// </summary>
        private List<string> _knownCompanyIds;
    }
}


