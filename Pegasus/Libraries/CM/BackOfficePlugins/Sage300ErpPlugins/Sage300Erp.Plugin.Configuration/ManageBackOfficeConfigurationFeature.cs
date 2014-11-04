/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using ACCPAC.Advantage;
using Sage.Connector.Configuration.Contracts.Features;
using Sage.Connector.Configuration.Contracts.Metadata;
using Sage.Connector.Configuration.Contracts.Payload;
using Sage.Connector.DomainContracts.Core;
using Sage.Connector.DomainContracts.Core.BackOffice;
using Sage.Connector.DomainContracts.Core.Payload;
using Sage300Erp.Plugin.Native;

namespace Sage300Erp.Plugin.Configuration
{
    /// <summary>
    /// Manage Back Office Configuration feature requests
    /// </summary>
    [Export(typeof(IManageBackOfficeConfiguration))]
    [BackOfficeConfigMetadataExport("Sage300Erp", "Sage 300 ERP Plugin", "AnyX86")]
    public class ManageBackOfficeConfigurationFeature : IManageBackOfficeConfiguration
    {
        /// <summary>
        /// Validate the Back office configuration. 
        /// </summary>
        /// <param name="backOfficeConfiguration"></param>
        /// <returns></returns>
        public ValidationResponse ValidateBackOfficeConnection(IBackOfficeCompanyConfiguration backOfficeConfiguration)
        {
            //TODO: Review
            if (String.IsNullOrWhiteSpace(backOfficeConfiguration.CompanyId))
                return new ValidationResponse { IsValid = true };

            Diagnoses diagnoses = new Diagnoses();

            try
            {
                var erpSession = Common.GetErpSession(backOfficeConfiguration);
                if (erpSession == null)
                {
                    const string msg = "Could not get ERP Session.";

                    diagnoses.Add(new Diagnosis
                    {
                        Severity = Severity.Error,
                        UserFacingMessage = msg,
                        RawMessage = msg
                    });
                    return new ValidationResponse
                    {
                        IsValid = false,
                        Diagnoses = diagnoses
                    };
                }

                var dbLink = erpSession.OpenDBLink(DBLinkType.Company, DBLinkFlags.ReadOnly);
                if (dbLink == null)
                {
                    const string msg = "Invalid company for erp session.";
                    diagnoses.Add(new Diagnosis
                  {
                      Severity = Severity.Error,
                      UserFacingMessage = msg,
                      RawMessage = msg
                  });
                    return new ValidationResponse()
                      {
                          IsValid = false,
                          Diagnoses = diagnoses
                      };


                }
                return new ValidationResponse { IsValid = true };


            }
            catch (Exception ex)
            {
                diagnoses.Add(new Diagnosis
                {
                    Severity = Severity.Error,
                    UserFacingMessage = "Unexpected error occured validating the back office connection.",
                    RawMessage = ex.Message + ex.StackTrace
                });
                return new ValidationResponse
                {
                    IsValid = false,
                    Diagnoses = diagnoses
                };
            }
        }


        /// <summary>
        /// Get the set of Back office configurations available via this login information
        /// </summary>
        /// <param name="backOfficeLogin"></param>
        /// <returns></returns>
        public ICollection<BackOfficeCompanyConnection> GetBackOfficeCompanyConnections(IBackOfficeLogin backOfficeLogin)
        {
            //TODO:  Is there a way to get the list of back office connections for Sage 300 Erp?

            ICollection<BackOfficeCompanyConnection> boConns = new Collection<BackOfficeCompanyConnection>();
            var mockBackOfficeConnection = new BackOfficeCompanyConnection("SAMINC", "Sample Company Inc.", "SAMINC");
            boConns.Add(mockBackOfficeConnection);
            mockBackOfficeConnection = new BackOfficeCompanyConnection("SAMLTD", "Sample Company Ltd.", "SAMLTD");
            boConns.Add(mockBackOfficeConnection);
            return boConns;
        }
    }
}
