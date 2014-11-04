using Sage.Connector.Configuration.Contracts.BackOffice;
using Sage.Connector.Configuration.Contracts.Data;
using Sage.Connector.Configuration.Contracts.Data.DataTypes;
using Sage.Connector.Configuration.Contracts.Data.Responses;
using Sage.Connector.Configuration.Contracts.Data.SelectionValueTypes;
using Sage.Connector.Customers.Contracts.BackOffice;
using Sage.Connector.Customers.Contracts.Data;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Sage.Connector.MockPlugin.Customers
{
    /// <summary>
    /// Mock backoffice plugin for Sync Customers. 
    /// </summary>
    [Export(typeof(ISyncCustomers))]
    [Export(typeof(IManageFeatureConfiguration))]
    [ExportMetadata("BackOfficeId", "Mock")]
    public class SyncCustomersFeature : ISyncCustomers, IManageFeatureConfiguration
    {
        private int _count;
        private string _defaultPaymentTerms;

        /// <summary>
        /// Initialize the feature with the default property values
        /// </summary>
        /// <param name="defaultPropertyValues">readonly set of property name - value pairs</param>
        /// <returns><see cref="Response"/></returns>
        public Response Initialize(IDictionary<string, object> defaultPropertyValues)
        {
            object defaultPaymentTerms;
            defaultPropertyValues.TryGetValue("PaymentTerms", out defaultPaymentTerms);
            _defaultPaymentTerms = (string)defaultPaymentTerms;
            return new Response { Status = Status.Success };
        }

        /// <summary>
        /// Initialize the SyncCustomers to get ready to start processing the sync.
        /// This could be something like loading the customer business object to get 
        /// ready to process. 
        /// </summary>
        /// <returns><see cref="Response"/> A response status of failure will end the sync feature immediately.  </returns>
        public Response InitializeSyncCustomers()
        {
            _count = 0;
            return new Response { Status = Status.Success };
        }

        /// <summary>
        /// Get the next Customer that needs to be sync'd up to the cloud
        /// </summary>
        /// <returns></returns>
        public Customer GetNextSyncCustomer()
        {
            // Return null when there isn't any more of the entity to Get.  
            // For this sample data, we are limiting the number of sync'd entities to 5

            if (_count == 5)
            {
                return null;
            }

            _count++;


            // The mock is trying to create a change situation for delta syncs
            int rInt = new Random().Next(0, 9);
            bool creditFlag = (rInt % 2 == 0);


            string extId = (1000 + _count).ToString(CultureInfo.CurrentCulture);

            var customer = new Customer
            {
                ExternalId = extId,
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                CreditAvailable = creditFlag ? 1000 + _count : 0,
                CreditLimit = Decimal.Zero,
                IsCreditLimitUsed = creditFlag,
                IsOnCreditHold = !creditFlag,
                Name = "Name " + extId,
                PaymentTerms = _defaultPaymentTerms,
                TaxSchedule = "tax Sched",


            };

            //Multiple contacts are allowed if the back office supports them. 
            var contact = new CustomerContact
            {
                ExternalId = extId, // need something more unique if multiple contacts
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                EmailWork = extId + "@Work.com",
                FirstName = extId + "First",
                LastName = extId + "Last",
                PhoneWork = extId.PadRight(10, '9'),
                EmailPersonal = "email" + extId + "@personal.com",
                PhoneHome = extId.PadRight(10, '8'),
                PhoneMobile = extId.PadRight(10, '7'),
                Title = "CEO",
                Url = @"http://www.cust" + extId + ".com"
            };
            customer.Contacts.Add(contact);

            //Multiple addresses are allowed if the back office supports them. 
            var address = new CustomerAddress
            {
                ExternalId = extId,// need something more unique if multiple addresses
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                Name = extId,
                City = extId + "City",
                Country = "USA",
                Email = extId + "@Address.com",
                Phone = extId.PadRight(10, '8'),
                PostalCode = "99999",
                StateProvince = "CA",
                Street1 = extId + " Street1",
                Street2 = extId + " Street2",
                Street3 = extId + " Street3",
                Street4 = extId + " Street4",
                TaxSchedule = "tax sched",
                Type = AddressType.Mailing,
                URL = @"http:\\www.someurl" + extId + ".com",
                Contact = extId //Primary Contact for this address

            };
            customer.Addresses.Add(address);

            var custTaxClass = new CustomerTaxClass
            {
                ExternalId = extId,
                ExternalReference = extId,
                EntityStatus = EntityStatus.Active,
                TaxClass = "taxClass",
                TaxCode = "1"
            };

            customer.TaxClasses.Add(custTaxClass);

            return (customer);
        }


        /// <summary>
        /// Get the list of properties for this feature
        /// </summary>
        /// <returns>If there aren't any properties, then there is no need to implement <see cref="IManageFeatureConfiguration"/></returns>
        public ICollection<PropertyDefinition> GetFeatureProperties()
        {
            ICollection<PropertyDefinition> properties = new Collection<PropertyDefinition>();

            //TODO: Use Resource files for localization
            properties.Add(new PropertyDefinition(propertyName: "PaymentTerms", displayName: "Default PaymentTerms", description: "The default mock terms for the customer if not set up with one.", propertyDataType: new StringType { SelectionType = SelectionTypes.Lookup }, backOfficeValidation: false));
            properties.Add(new PropertyDefinition("ListField", "Default ListField", "The default list field for the customer if not set up with one.",
                new StringType { SelectionType = SelectionTypes.List }, false));
            properties.Add(new PropertyDefinition("StringMaxLengthField", "Default String (20)", "The default string with max length of 20 for data type testing.",
               new StringType{MaxLength = 20}, false));
            properties.Add(new PropertyDefinition("BooleanField", "Default Boolean", "The default boolean for data type testing.",
                  new BooleanType(), false));
            properties.Add(new PropertyDefinition("DateTimeField", "Default DateTime", "The default date time for data type testing.",
                new DateTimeType(), false));
            properties.Add(new PropertyDefinition("DecimalField", "Default Decimal", "The default decimal for data type testing.",
                new DecimalType(), false));
            properties.Add(new PropertyDefinition("IntegerField", "Default Integer", "The default integer for data type testing.",
                new IntegerType(), false));

            return properties;
        }




        /// <summary>
        /// Validate the Feature configuration. 
        /// The BeginSession is called before this method to allow for connection to the database.  
        /// EndSession is called after to close the connection to the database. 
        /// </summary>
        /// <param name="featurePropertyValuePairs"></param>
        /// <returns>Collection of property responses</returns>

        public ICollection<PropertyValuePairValidationResponse> ValidateFeatureConfigurationValues(IDictionary<string, object> featurePropertyValuePairs)
        {
            // The back office validation is turned off See GetFeatureProperties implementation above.
            // Validation is set to false because the user would select from a valid selection.
            // See MockPlugin.Sales for ProcessQuote to see how a validation is implemented

            return null;

        }


        /// <summary>
        /// Setup up the set of property definitions entry values used for this feature. 
        /// The BeginSession is called just prior to this method call in order to setup the 
        /// datebase session to access the database for any information to assist in this setup.
        /// </summary>
        /// <param name="propertyEntryValues"></param>
        public void SetupFeatureConfigurationEntryValues(IDictionary<string, AbstractSelectionValueTypes> propertyEntryValues)
        {
            //This is just an example

            foreach (var propPair in propertyEntryValues)
            {
                //this property is set up to be a lookup type.  Therefore the entry value contains
                //a dictionary to populate with the list of payment terms retrieved from the backoffice
                //using the back office connection from the BeginSession call that was performed just
                //prior to this method call.
                if (propPair.Key.Equals("PaymentTerms"))
                {
                    var values = (LookupTypeValues)propPair.Value;
                    //Begin Session was called.  use back office connection to access business object
                    //to loop through the set of payment terms to add to the lookup list
                    values.LookupValues.Add("KeyCOD", "COD");
                    values.LookupValues.Add("Net7", "payment seven days after invoice date");
                    values.LookupValues.Add("Net10", "Payment ten days after invoice date");
                    values.LookupValues.Add("Terms30Days", "Payment 30 days after invoice date");

                    continue;
                }

                //this property is set up to be a lookup type.  Therefore the entry value contains
                //a dictionary to populate with the list of payment terms retrieved from the backoffice
                //using the back office connection from the BeginSession call that was performed just
                //prior to this method call.
                if (propPair.Key.Equals("ListField"))
                {
                    var values = (ListTypeValues)propPair.Value;
                    //Begin Session was called.  use back office connection to access business object
                    //to loop through the set of payment terms to add to the  list
                    values.ListValues.Add("List Item 1");
                    values.ListValues.Add("List Item 2");
                    values.ListValues.Add("List Item 3");
                    values.ListValues.Add("List Item 4");

                    continue;
                }

                //TODO:  Do we really need value types here? 
                if (propPair.Key.Equals("Some Integer Type"))
                {
                    //TODO: Connector Team needs to add other data type value objects to set integer value.
                    //TODO:  Currently there is no default value because if there was, why not use that 
                    //TODO: instead of going through the hoops to have connector set it up? 
                }
            }
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
