/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using Sage.Connector.DomainContracts.Core;
using Sage.Connector.DomainContracts.Core.BackOffice;
using Sage.Connector.DomainContracts.Core.FeatureConfiguration;
using Sage.Connector.DomainContracts.Core.Payload;
using Sage.Connector.Sync.Contracts.Features;
using Sage.Connector.Sync.Contracts.Payload;
using Sage.Connector.Sync.Contracts.Utilities;
using Sage300Erp.Plugin.Native;
using Sage300ERP.Plugin.Native.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace Sage300Erp.Plugin.Sync
{
    /// <summary>
    /// Synchronizes the A/R Customer data 
    /// </summary>
    [Export(typeof(ISyncCustomers))]
    [Export(typeof(IManageFeatureConfiguration))]
    [ExportMetadata("BackOfficeId", "Sage300Erp")]
    public class SyncCustomersFeature: AbstractNativeSyncBase, ISyncCustomers, IDisposable
    {
        private ICustomersView _customers;

        //todo kms: include tax groups code later, as well as contacts and addresses
// ReSharper disable once FieldCanBeMadeReadOnly.Local
        private ITaxGroupView _taxGroups = null;

        private bool _disposeView;


        /// <summary>
        /// Initialize the sync customers 
        /// </summary>
        /// <returns></returns>
        public Response InitializeSyncCustomers()
        {
            return base.Initialize();
        }
        /// <summary>
        /// Get the next Customer that needs to be sync'd up to the cloud
        /// </summary>
        /// <returns></returns>
        public Customer GetNextSyncCustomer()
        {
            //at this point, the view has been setup to be at the first record to be validated. 
            GetValidNextEntity();
            if (!MoreData)
                return null;

            var customer = new Customer
            {
                ExternalId = _customers.ExternalId,
                //ExternalReference = _customers.ExternalId,
                Name = _customers.Name ?? DummyConstants.Name,
                //EntityStatus = (Nephos.Model.Base.EntityStatus)_customers.Status,
                IsCreditLimitUsed = _customers.CheckCreditLimit,
                IsOnCreditHold = _customers.CreditOnHold,
                CreditLimit = _customers.CreditLimit,
                CreditAvailable = _customers.CreditAvailable,
                PaymentTerms = _customers.PaymentTerms,
                //TaxSchedule = taxSchedule,
            };

            var address = new CustomerAddress
            {
                ExternalId = customer.ExternalId,
                ExternalReference = customer.ExternalReference,
                //TODO:  We are setting this to mailing right now so that the integration testing will work.  We need to revisit to determine if we need to change this to Billing.
                Type = AddressType.Mailing,
                Name = customer.Name,
                Street1 = _customers.Street1 ?? DummyConstants.Street1,
                Street2 = _customers.Street2,
                Street3 = _customers.Street3,
                Street4 = _customers.Street4,
                City = _customers.City,
                StateProvince = _customers.StateProvince,
                PostalCode = _customers.PostalCode,
                Country = _customers.Country,
                Phone = _customers.Phone,
                Email = _customers.Email,
                URL = _customers.URL,
              //  TaxSchedule = taxSchedule,
                EntityStatus = (EntityStatus)_customers.Status
            };

            customer.Addresses.Add(address);
            NameTransform nameTransform = NameTransform.TransformName(_customers.Contact);

            var contact = new CustomerContact
            {
                ExternalId = customer.ExternalId,
                ExternalReference = customer.ExternalReference,
                FirstName = nameTransform.FirstName ?? DummyConstants.Name,
                LastName = nameTransform.LastName ?? DummyConstants.Name,
                PhoneWork = _customers.PhoneWork,
                EmailWork = _customers.EmailWork,
                EntityStatus = (EntityStatus)_customers.Status
            };
            customer.Contacts.Add(contact);

            //get ready for next call;
            MoreData = _customers.GoNext();

            return (customer);
        }

        ///<summary>
        /// Begin the Back office session for the given configuration
        /// </summary>
        /// <param name="backOfficeConfiguration"><see cref="IBackOfficeCompanyConfiguration"/></param>
        public override Response BeginSession(IBackOfficeCompanyConfiguration backOfficeConfiguration)
        {
            _disposeView = true;
             var response = base.BeginSession(backOfficeConfiguration);

            //TODO KMS: the assumption here is the back office configuration contains company specific 
            //TODO KMS: configuration related to the needs of this feature task. 

            return response;
        }


        /// <summary>
        /// Get the properties that must be configured to support the sync customers feature for mock sage 300 erp.
        /// </summary>
        /// <returns>List of property definitions.</returns>
        public ICollection<PropertyDefinition> GetFeatureProperties()
        {
            ICollection<PropertyDefinition> properties = new Collection<PropertyDefinition>();

            properties.Add(new PropertyDefinition("CurrencyCode", "Home Currency Code", "The Home Currency Code to default into the customer."));

            return properties;
        }


        /// <summary>
        /// Validate this feature's property configuration values.
        /// </summary>
        /// <param name="featurePropertyValuePairs"></param>
        /// <returns></returns>
        public ValidationResponse ValidateFeatureConfigurationValues(IDictionary<PropertyDefinition, string> featurePropertyValuePairs)
        {
            bool validState = true;

            //TODO Review:  We shouldn't have to deal with missing properties because the expectation is that the call to 
            //TODO Review: GetFeatureProperties has been performed before this call. 
            Diagnoses diagnoses = new Diagnoses();
            
            foreach (KeyValuePair<PropertyDefinition, String> propPair in featurePropertyValuePairs)
            {

                if (propPair.Key.PropertyName.Equals("CurrencyCode"))
                {
                    //validate mock sage 300 erp terms.. At this point the back office should be called to perform the validation.
                    if (!(propPair.Value.Equals("USD") || propPair.Value.Equals("CAD")))
                    {
                        string msg = String.Format("Invalid valid {0} value: {1}", propPair.Key.DisplayName,
                            propPair.Value);
                        diagnoses.Add(new Diagnosis
                        {
                            Severity = Severity.Error,
                            RawMessage = msg,
                            UserFacingMessage = msg
                        });
                        validState = false;
                    }
                }
            }
            return new ValidationResponse { IsValid = validState, Diagnoses = diagnoses} ;

        }


        /// <summary>
        /// Get the customers view view used for browsing.
        /// </summary>
        protected override IBrowseable BrowseView
        {
            get
            {
                if (_customers == null)
                {
                    //TODO KMS:  use the stored back office configuration to get to date for this task. 
                    _customers = new CustomersView(ErpSession, DbLink, "USD"); //_BackOfficeCompanyConfiguration.GetPropertyValue("CurrencyCode")
                }
                if (_taxGroups == null)
                {
                    //todo kms:
                    //_TaxGroups = new TaxGroupView(ErpSession, DbLink);

                }
                return _customers;
            }
        }



        // Dispose() calls Dispose(true)
        public new virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
       }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~SyncCustomersFeature()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        /// <summary>
        /// Dispose the customer view
        /// </summary>
        /// <param name="disposing">if we are to be disposing call dispose on the views used to support sync customers.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if ((_customers != null) && (_disposeView))
                {
                    // Call Dispose in order for this class to free up its resources (e.g. any Sage 300 views that is has opened).
                    _customers.Dispose();
                }
                if ((_taxGroups != null) && (_disposeView))
                {
                    // Call Dispose in order for this class to free up its resources (e.g. any Sage 300 views that is has opened).
                    _taxGroups.Dispose();
                }
            }
        }


        ///// <summary>
        ///// Transform the current record of the <see cref="MainView"/> to populate a Nephos.Model entity and save it to the NHibernate session.        
        ///// </summary>
        ///// <param name="message">The record identifier that will be included in the log message.</param>
        //protected override SyncEntityState Transform(out string message)
        //{
        //    SyncEntityState syncResult = SyncEntityState.Unchanged;

        //    TaxSchedule taxSchedule = new TaxSchedule
        //    {
        //        ExternalId = _customers.TaxGroup

        //    };

        //    var customer = new Customer
        //    {
        //        ExternalId = _customers.ExternalId,
        //        ExternalReference = _customers.ExternalId,
        //        Name = _customers.Name ?? DummyConstants.Name,
        //        EntityStatus = (Nephos.Model.Base.EntityStatus)_customers.Status,
        //        IsCreditLimitUsed = _customers.CheckCreditLimit,
        //        IsOnCreditHold = _customers.CreditOnHold,
        //        CreditLimit = _customers.CreditLimit,
        //        CreditAvailable = _customers.CreditAvailable,
        //        PaymentTerms = _customers.PaymentTerms,
        //        TaxSchedule = taxSchedule,
        //    };

        //    var address = new Address
        //    {
        //        ExternalId = customer.ExternalId,
        //        ExternalReference = customer.ExternalReference,
        //        Customer = customer,
        //        //TODO:  We are setting this to mailing right now so that the integration testing will work.  We need to revisit to determine if we need to change this to Billing.
        //        Type = AddressType.Mailing,
        //        Name = customer.Name,
        //        Street1 = _customers.Street1 ?? DummyConstants.Street1,
        //        Street2 = _customers.Street2,
        //        Street3 = _customers.Street3,
        //        Street4 = _customers.Street4,
        //        City = _customers.City,
        //        StateProvince = _customers.StateProvince,
        //        PostalCode = _customers.PostalCode,
        //        Country = _customers.Country,
        //        Phone = _customers.Phone,
        //        Email = _customers.Email,
        //        URL = _customers.URL,
        //        TaxSchedule = taxSchedule,
        //        EntityStatus = (Nephos.Model.Base.EntityStatus)_customers.Status
        //    };

        //    NameTransform nameTransform = NameTransform.TransformName(_customers.Contact);

        //    var contact = new Contact
        //    {
        //        ExternalId = customer.ExternalId,
        //        ExternalReference = customer.ExternalReference,
        //        Customer = customer,
        //        FirstName = nameTransform.FirstName ?? DummyConstants.Name,
        //        LastName = nameTransform.LastName ?? DummyConstants.Name,
        //        PhoneWork = _customers.PhoneWork,
        //        EmailWork = _customers.EmailWork,
        //        EntityStatus = (Nephos.Model.Base.EntityStatus)_customers.Status
        //    };

        //    IList<string> taxCodeList = new List<string>();

        //    // The AR customer record has 5 tax class fields (TAXSTTS1..5) which correspond to the up-to-five
        //    // tax authorities for the customer's tax group. So, we need to read the Tax Group record and match
        //    // those authority codes to the tax classes in the customer record.
        //    _TaxGroups.Browse(string.Format(TaxGroups.FilterByGroupID, _customers.TaxGroup), true);
        //    if (_TaxGroups.GoTop())
        //    {
        //        for (int taxAuthorityIndex = 0; taxAuthorityIndex < TaxGroups.MaxNumberOfAuthorities; taxAuthorityIndex++)
        //        {
        //            string taxAuthority = _TaxGroups.Authorities[taxAuthorityIndex];

        //            // If tax authority is blank, skip.
        //            if (!string.IsNullOrEmpty(taxAuthority))
        //            {
        //                CustomerTaxClass customerTaxClass = new CustomerTaxClass
        //                {
        //                    ExternalId = Common.BuildModelExternalId(_customers.ExternalId, taxAuthority),
        //                    ExternalReference = Common.BuildModelExternalId(_customers.ExternalId, taxAuthority),
        //                    TaxClass = _customers.TaxClasses[taxAuthorityIndex].ToString(),
        //                    EntityStatus = Nephos.Model.Base.EntityStatus.Active,
        //                };

        //                customer.TaxClasses.Add(customerTaxClass);
        //                taxCodeList.Add(taxAuthority);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Should not get here!
        //        Debug.Assert(false, "Did not find Customer Tax Group '" + _customers.TaxGroup + "' in the Sage 300 TXGRP table.");
        //    }

        //    customer.Addresses.Add(address);
        //    customer.Contacts.Add(contact);

        //    syncResult = ConnectorSession.SyncHandler.SyncEntity(customer);

        //    if (syncResult != SyncEntityState.Unchanged)
        //    {
        //        if (!string.IsNullOrWhiteSpace(_customers.TaxGroup))
        //        {
        //            AddReference(EntityResourceKindName.Customers, customer.Id, EntityResourceKindName.TaxSchedules, _customers.TaxGroup, true, _ReferenceList);
        //            AddReference(EntityResourceKindName.Addresses, address.Id, EntityResourceKindName.TaxSchedules, _customers.TaxGroup, true, _ReferenceList);
        //        }
        //        if (customer.TaxClasses.Any())
        //        {
        //            int i = 0;
        //            IList<Nephos.Model.CustomerTaxClass> customerTaxClasses = customer.TaxClasses.ToList<Nephos.Model.CustomerTaxClass>();
        //            foreach (var customerTaxClass in customerTaxClasses)
        //            {
        //                AddReference(EntityResourceKindName.CustomerTaxClasses, customerTaxClass.Id, EntityResourceKindName.TaxCodes, taxCodeList[i], false, _ReferenceList);
        //                i++;
        //            }
        //        }
        //    }

        //    message = customer.ExternalId;
        //    return syncResult;
        //}


        ///// <summary>
        ///// Apply the references from the referencesNeeded list to the nephos model.
        ///// This is meant to be run after the SData cloud connector has fetched the references.
        ///// </summary>
        ///// <param name="connectorSession"></param>
        ///// <param name="referencesNeeded"></param>
        //public bool FixupReferences(IConnectorSession connectorSession, IList<IReferenceInfo> referencesNeeded)
        //{
        //    NHibernate.ISession sess = connectorSession.Session;

        //    // Customers - Tax Schedule references
        //    var custTaxSchedRefs = from r in referencesNeeded
        //                           where r.ParentResourceKindName.Equals(EntityResourceKindName.Customers, StringComparison.CurrentCultureIgnoreCase)
        //                           && r.ReferenceResourceKindName.Equals(EntityResourceKindName.TaxSchedules, StringComparison.CurrentCultureIgnoreCase)
        //                           && r.ReferenceId != null
        //                           select r;

        //    if (custTaxSchedRefs.Any())
        //    {
        //        using (var nephosTransaction = sess.BeginTransaction())
        //        {
        //            foreach (var custTaxSchedRef in custTaxSchedRefs)
        //            {
        //                var referrer = sess.Get<Nephos.Model.Customer>(custTaxSchedRef.ParentId);
        //                referrer.TaxSchedule = sess.Get<Nephos.Model.TaxSchedule>(custTaxSchedRef.ReferenceId);
        //                sess.Update(referrer);
        //            }
        //            nephosTransaction.Commit();
        //        }
        //    }

        //    // Addresses - Tax Schedule references
        //    var addressTaxSchedRefs = from r in referencesNeeded
        //                              where r.ParentResourceKindName.Equals(EntityResourceKindName.Addresses, StringComparison.CurrentCultureIgnoreCase)
        //                              && r.ReferenceResourceKindName.Equals(EntityResourceKindName.TaxSchedules, StringComparison.CurrentCultureIgnoreCase)
        //                              && r.ReferenceId != null
        //                              select r;

        //    if (addressTaxSchedRefs.Any())
        //    {
        //        using (var nephosTransaction = sess.BeginTransaction())
        //        {
        //            foreach (var addressTaxSchedRef in addressTaxSchedRefs)
        //            {
        //                var referrer = sess.Get<Nephos.Model.Address>(addressTaxSchedRef.ParentId);
        //                referrer.TaxSchedule = sess.Get<Nephos.Model.TaxSchedule>(addressTaxSchedRef.ReferenceId);
        //                sess.Update(referrer);
        //            }
        //            nephosTransaction.Commit();
        //        }
        //    }

        //    // Tax Classes - Tax Code references
        //    var taxClassTaxCodeRefs = from r in referencesNeeded
        //                              where r.ParentResourceKindName.Equals(EntityResourceKindName.CustomerTaxClasses, StringComparison.CurrentCultureIgnoreCase)
        //                              && r.ReferenceResourceKindName.Equals(EntityResourceKindName.TaxCodes, StringComparison.CurrentCultureIgnoreCase)
        //                              && r.ReferenceId != null
        //                              select r;

        //    if (taxClassTaxCodeRefs.Any())
        //    {
        //        using (var nephosTransaction = sess.BeginTransaction())
        //        {
        //            foreach (var taxCodeRef in taxClassTaxCodeRefs)
        //            {
        //                var referrer = sess.Get<Nephos.Model.CustomerTaxClass>(taxCodeRef.ParentId);
        //                referrer.TaxCodeId = (Guid)taxCodeRef.ReferenceId;
        //                sess.Update(referrer);
        //            }
        //            nephosTransaction.Commit();
        //        }
        //    }

        //    return _MoreData;
        //}

        //public override void UploadReferenceBatch(IConnectorSession connectorSession, IList<IReferenceInfo> referencesNeeded, int batchSize)
        //{
        //    _ReferenceList = referencesNeeded;
        //    UploadBatch(connectorSession, batchSize);
        //}

    }
}
