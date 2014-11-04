using Sage.Connector.Configuration.Contracts.BackOffice;
using Sage.Connector.Configuration.Contracts.Data;
using Sage.Connector.Configuration.Contracts.Data.DataTypes;
using Sage.Connector.Configuration.Contracts.Data.Responses;
using Sage.Connector.Configuration.Contracts.Data.SelectionValueTypes;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.Sales.Contracts.BackOffice;
using Sage.Connector.Sales.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Sage.Connector.MockPlugin.Sales
{
    /// <summary>
    /// Mock backoffice plugin to process quote.
    /// </summary>
    [Export(typeof(IProcessQuote))]
    [Export(typeof(IManageFeatureConfiguration))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class ProcessQuoteFeature : IProcessQuote, IManageFeatureConfiguration
    {
        // ReSharper disable once NotAccessedField.Local  -- just an example
        private IDictionary<string, object> _defaultPropertyValues;

        /// <summary>
        /// Initialize the feature with the default property values
        /// </summary>
        /// <param name="defaultPropertyValues">readonly set of property name - value pairs</param>
        /// <returns>
        /// <see cref="T:Sage.Connector.DomainContracts.Responses.Response"/>
        /// </returns>
        public Response Initialize(IDictionary<string, object> defaultPropertyValues)
        {
            //If there aren't any default property values set up for this feature, this would be an empty dictionary
            _defaultPropertyValues = defaultPropertyValues;
            return new Response { Status = Status.Success };
        }
        /// <summary>
        /// Process the Quote
        /// </summary>
        public Response ProcessQuote(Quote quote)
        {
            var response = new Response();

            try
            {
                //The purpose is to be able to price and provide accurate totals for the Quote
                //regardless if the back office supports a Quote Entity.

                //TODO by Back Office:  Create a Quote if supported by BackOffice and price out the quote

                //just sample of using the feature configuration propery value. 
                int expiryFromDays =30;
                object expiryFromDaysValue;
                var parsed= _defaultPropertyValues.TryGetValue("ExpiryFromDays", out expiryFromDaysValue);
                parsed = parsed &&  int.TryParse(expiryFromDaysValue.ToString() , out expiryFromDays);

                if (!parsed)
                {
                    string msg = "Using expiry from days default of 30 days.";
                    response.Diagnoses.Add(new Diagnosis
                    {
                        Severity = Severity.Information,
                        RawMessage = msg,
                        UserFacingMessage = msg
                    });
                }
                //pretend the quote expires in 30 days. 
                quote.ExpiryDate = DateTime.UtcNow.Add(TimeSpan.FromDays(expiryFromDays));

                //making up external ids
                quote.ExternalId = DateTime.Now.Ticks.ToString(CultureInfo.CurrentCulture);
                quote.ExternalReference = quote.ExternalId;

                quote.DocumentTotal = 516m;
                quote.SandH = 6m;
                quote.SubTotal = 500m;
                quote.Tax = 10m;

                //just using count to make up unique external ids for the detail lines
                int count = 0;
                foreach (var detail in quote.Details)
                {

                    count++;
                    detail.ExternalId = quote.ExternalId + "-" + count;
                    detail.ExternalReference = detail.ExternalId;

                    detail.Price = 100m;
                }
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
                            UserFacingMessage = "Unexpected Error processing quote.",
                            RawMessage = ex.Message + ex.StackTrace
                        }
                    }
                };
            }

            response.Status = Status.Success;
            return response;
        }

        /// <summary>
        /// Get the list of propert definitions for the Process Quote feature
        /// </summary>
        /// <returns></returns>
        public ICollection<PropertyDefinition> GetFeatureProperties()
        {
            ICollection<PropertyDefinition> properties = new Collection<PropertyDefinition>();

            //TODO: Use Resource files for localization
            properties.Add(new PropertyDefinition("ExpiryFromDays", "Expire Days", "The number of days from today to expire the quote.", new IntegerType(), true));

            return properties;
        }


        /// <summary>
        /// Setup the set of entry values for the set of properties defined for this feature.
        /// </summary>
        /// <param name="propertyEntryValues">Data Type specific default selection values by property name
        ///             To be used in the configuration entry for the company. 
        ///             In the case of a List type, a list will be given to populate.  
        ///             In the case of Lookup type, a Dictionary will be given to populate with key value pairs. </param>
        public void SetupFeatureConfigurationEntryValues(IDictionary<string, AbstractSelectionValueTypes> propertyEntryValues)
        {
            //no op
        }


        /// <summary>
        /// Validate the feature property configuration values
        /// </summary>
        /// <param name="featurePropertyValuePairs"></param>
        /// <returns></returns>
        public ICollection<PropertyValuePairValidationResponse> ValidateFeatureConfigurationValues(IDictionary<string, object> featurePropertyValuePairs)
        {
            var validationResponse = new Collection<PropertyValuePairValidationResponse>();

            foreach (var propPair in featurePropertyValuePairs)
            {
                var propertyValidationResponse = new PropertyValuePairValidationResponse
                {
                    PropertyValuePair = propPair
                };
                validationResponse.Add(propertyValidationResponse);

                if (propPair.Key.Equals("ExpiryFromDays"))
                {
                    const int maxExpiryDays = 120;
                    int expiryFromDays;
                    bool parsed = int.TryParse((string)(propPair.Value ?? String.Empty), out expiryFromDays);

                    //validate mock terms - can go to back office to validate property value
                    if (!parsed || expiryFromDays > maxExpiryDays)
                    {
                        //TODO: Use Resource files for localization
                        String msg = String.Format("Invalid valid {0} value: '{1}'.  Must be an integer value less or equal to {2}",
                            "Expire Days", propPair.Value, maxExpiryDays);

                        propertyValidationResponse.Status = Status.Failure;
                        propertyValidationResponse.Diagnoses.Add(new Diagnosis
                        {
                            Severity = Severity.Error,
                            UserFacingMessage = msg,
                            RawMessage = msg
                        });

                    }
                    else
                    {
                        propertyValidationResponse.Status = Status.Success;
                    }
                }
            }
            return validationResponse;
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
