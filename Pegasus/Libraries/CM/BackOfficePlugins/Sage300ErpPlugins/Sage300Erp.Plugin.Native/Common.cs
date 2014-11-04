/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using ACCPAC.Advantage;
using Microsoft.Win32;
using Sage.Connector.DomainContracts.Core.BackOffice;
using Sage300ERP.Plugin.Native.Types;
using System;
using System.Collections.Generic;
using System.Text;


namespace Sage300Erp.Plugin.Native
{
    /// <summary>
    /// Helper class that contains static methods shared across the upload and download features of the plugin.
    /// </summary>
    public static class Common
    {
        #region Sage 300 ERP Session Constants
        private const string AppID = "SC";
        private const string AppVersion = "61A";
        private const string ProgramName = "SC0001";
        #endregion

        #region Sage 300 ERP Registry Values
        private const string RegistryPath = "SOFTWARE\\ACCPAC International, Inc.\\ACCPAC\\Configuration";
        private const string WebHelpUrlName = "WebHelpURL";
        private const string WebHelpUrlValue = "http://<hostname>/ACCPAC/Help";
        #endregion

        #region Strings/Messages
        public const string AppID_IC = "IC";
        public const string AppID_OE = "OE";
        public const string AppID_AR = "AR";

        private const string ErrorERPCompany = "The company id you have specified is invalid.";
        private const string ErrorERPPassword = "You must specify a password for this company.";
        #endregion

        #region Default Payment Code
        public const string Default_CC_PaymentCode = "MOBCC";
        public const string Default_Check_PaymentCode = "MCHCK";
        public const string Default_Cash_PaymentCode = "MCASH";
        public const string CreditCard_Prefix = "************";
        #endregion

        public const string Default_Misc_Charge_Code = "MSANDH";
        public const string Default_Terms = "00";
        public const string Default_MobileTaxGroup = "MOBILETAX";
        public const string Currency_USD = "USD";
        public const string Currency_CAD = "CAD";
        public enum AgeBy { DueDate, DocumentDate};
        public static Dictionary<int, string> DocTransactionDescriptions = new Dictionary<int, string>
        {
            {1, "Invoice"},
            {2, "Debit Note"},
            {3, "Credit Note"},
            {4, "Interest"},
            {5, "Uapplied Cash"},
            {6, "Debit Note Applied To"},
            {7, "Applied Debit Note"},
            {8, "Credit Note Applied To"},
            {9, "Applied Credit Note"},
            {10, "PrePayment"},
            {14, "Adjustment"},
            {11, "Receipt"},
            {19, "Refund"}
        };


        // Define a delimiter as an ASCII Unit Separator + pipe symbol ("|"). The unit separator is the 
        // real delimiter and the pipe is just used to make it human readable for ExternalReference.
        public const string ExternalIdDelimiter = "\x1f" + "\x7c";  

        public enum DocumentType
        {
            Invoice = 1,
        }

        ///// <summary>
        ///// Gets an empty instance of the object that will be used to gather configuration from the host.
        ///// </summary>
        //public static object GetConfigurationInstance()
        //{
        //    var config = new ConfigurationObject()
        //    {
        //        UserId = string.Empty,
        //        Password = string.Empty,
        //        Company = string.Empty,
        //        CurrencyCode = Currency_USD,
        //        AgeBy =  (int)AgeBy.DueDate,
        //        Days = 30
        //    };
        //    return config;
        //}

        /// <summary>
        /// Due to a bug in the Sage 300 ERP 2012 install - there are some scenarios where the WebHelpURL
        /// value is null or empty.
        /// </summary>
        private static void CheckWebHelpUrl()
        {
            try
            {
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(RegistryPath);
                if (key != null)
                {
                    Object value = key.GetValue(WebHelpUrlName);
                    if ((value == null) || (string.IsNullOrEmpty(value.ToString())))
                    {
                        key.SetValue(WebHelpUrlName, WebHelpUrlValue);
                    }
                }
            }
            catch
            {
                //Swallow any exception that might arise from this - because this is only meant to be a preventative 
                //measure - and shouldn't prevent the upload from running.  Chances are that their registry settings 
                //were set correctly by the Sage 300 ERP install.
            }

        }

        ///// <summary>
        ///// Constructs a session to access the Sage 300 ERP data.  You must dipose this object when you are finished.
        ///// /// </summary>
        ///// <param name="connectorSession">The <see cref="IConnectorSession"/> for the plugin.</param>
        ///// <returns></returns>
        //public static Session GetErpSession(IConnectorSession connectorSession)
        //{
        //    return GetErpSession((ConfigurationObject)connectorSession.PluginConfiguration);
        //}

        /// <summary>
        /// Constructs a session to access the Sage 300 ERP data.  You must dipose this object when you are finished.
        /// /// </summary>
        /// <param name="configuration">The <see cref="IBackOfficeCompanyConfiguration"/> for the plugin.</param>
        /// <returns></returns>
        public static Session GetErpSession(IBackOfficeCompanyConfiguration configuration)
        {
            CheckWebHelpUrl();

            Session session = new Session();
            session.Init("", AppID, ProgramName, AppVersion);
            try
            {                    
                session.Open(configuration.UserId.ToUpper(), configuration.Password.ToUpper(), configuration.CompanyId.ToUpper(), DateTime.Today, 0);
            }
            catch
            {
                session.Dispose();
                session = null;
                throw;
            }
            return session;
        }

        /// <summary>
        /// Validates currency code against home currency if it is a single currency company.
        /// </summary>
        /// <param name="session">ERP Sage 300 session.</param>
        /// <param name="currency">Currency code</param>
        public static void ValidateHomeCurrencyInSingleCurrencyCompany(Session session, string currency)
        {
            var dbLink = session.OpenDBLink(DBLinkType.Company, DBLinkFlags.ReadWrite);

            if (dbLink.Company.Multicurrency) return;

            if (dbLink.Company.HomeCurrency != currency)
            {
                var message = string.Format("Currency {0} is not available in this ERP company", currency); 
                throw new ApplicationException(message);
            }

        }
        /// <summary>
        /// Validates that the company is a valid Sage 300 ERP company - and - if that company has security turned on
        /// that the password is not blank.  This method will throw an ApplicationException if the company or password
        /// is invalid.
        /// </summary>
        /// <param name="company">the id of the Sage 300 ERP company</param>
        /// <param name="password">the password for logging into the Sage 300 ERP company</param>
        public static void ValidateERPCompanyPassword(string company, string password)
        {
            bool valid = false;
            Session session = new Session();
            session.Init("", AppID, ProgramName, AppVersion);
            foreach (Organization org in session.Organizations)
            {
                if (org.ID.Trim().Equals(company.Trim().ToUpper()))
                {
                    if (org.SecurityEnabled)
                    {
                        if (string.IsNullOrEmpty(password))
                            throw new ApplicationException(ErrorERPPassword);

                    }
                    valid = true;
                    break;
                }
            }

            session.Dispose();

            if (!valid)
            {
                throw new ApplicationException(ErrorERPCompany);
            }
        }

        /// <summary>
        /// Check if the application is active
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="dbLink"></param>
        /// <returns></returns>
        public static bool IsApplicationActive(string appID, DBLink dbLink)
        {
            ActiveApplications apps = dbLink.ActiveApplications;
            foreach (ActiveApplication app in apps)
            {
                if (app.Selector.Equals(appID) && app.IsInstalled)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the IC application is active
        /// <param name="dbLink"></param>
        /// <returns></return
        public static bool IsICActive(DBLink dbLink)
        {
            return (IsApplicationActive(AppID_IC, dbLink));
        }


        /// <summary>
        /// Check if the OE application is active
        /// <param name="dbLink"></param>
        /// <returns></return
        /// 
        public static bool IsOEActive(DBLink dbLink)
        {
            return (IsApplicationActive(AppID_OE, dbLink));
        }

        /// <summary>
        /// Convert to Payment Type (ERP) from Payment method (Nephos)
        /// </summary>
        /// <param name="paymentMethod"><see cref="PaymentMethod"/></param>
        /// <returns></returns>
        public static PaymentType ToPaymentType(PaymentMethod paymentMethod)
        {
            PaymentType type;

            switch (paymentMethod)
            {
                case PaymentMethod.CreditCard:
                    type = PaymentType.CreditCard;
                    break;
                case PaymentMethod.Check:
                    type = PaymentType.Check;
                    break;
                case PaymentMethod.Cash:
                    type = PaymentType.Cash;
                    break;
                default:
                    type = PaymentType.Other;
                    break;
            }

            return type;
        }

        /// <summary>
        /// Builds an ExternalId for a Nephos Model Entity from a variable list of string parameters represeting the key values. 
        /// The delimiter between the key values is two characters: the non-printable ASCII 'unit separator' character plus the 
        /// pipe ("|") symbol. The pipe symbol is used as a printable delimiter so that it can also be used for ExternalReference.
        /// </summary>
        /// <param name="keyStrings"></param>
        /// <returns>ExternalId string</returns>
        /// 
        public static string BuildModelExternalId(params string[] keyStrings)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0 ; i < keyStrings.Length ; i++)
            {
                if (i > 0)
                    sb.Append(ExternalIdDelimiter);

                sb.Append(keyStrings[i]);
            }
            return (sb.ToString());
        }

        /// <summary>
        /// Splits a string built using BuildModelExternalId to the individual strings.
        /// </summary>
        /// <param name="externalId"></param>
        /// <returns>Array of parsed strings</returns>
        /// 
        public static string[] SplitModelExternalId(string externalId)
        {
            return externalId.Split(new[] {ExternalIdDelimiter}, StringSplitOptions.None);
        }

        //TODO KMS: fix up
        ///// <summary>
        ///// Performs the necessary actions to distribute the tax coming from the cloud 
        ///// to the ERP.
        ///// </summary>
        ///// <param name="model">WorkOrder or Order</param>
        ///// <param name="header">OrderView or InvoiceView</param>/// 
        //public static void ProcessTaxes(dynamic model, dynamic header)
        //{
        //    if (model.TaxCalcProvider == TaxCalcProvider.Avalara)
        //        ProcessAvalaraTax(model, header);
        //    else
        //        ProcessSageERPTax(model, header);

        //}

        //TODO KMS: fix up
        //public static void ProcessAvalaraTax(dynamic model, dynamic header)
        //{
        //    header.Tax1 = model.Tax;
        //    if (model.GetType() == typeof (Order))
        //        header.TaxBase1 = model.AmountPaid - model.Tax;  //model.OrderTotal: switch to amountPaid as OrderTotal has 3 decimal, which is wrong!!!
        //    else
        //        header.TaxBase1 = model.Total - model.Tax;
        //    header.DistributeTax();
        //}

        //TODO KMS: fix up
        //public static void ProcessSageERPTax(dynamic model, dynamic header)
        //{
        //        // Get the ERP to calculate the tax to figure out how it should be distributed among
        //        // the authorities in the invoice header since the cloud does not give us this information.
        //        header.CalculateTax();

        //        if (model.GetType() == typeof(Order))
        //            header.TurnOffAutoTaxCalculation();

        //        decimal taxTotalDelta = model.Tax - header.TaxTotal;

        //        // If there is a difference between the cloud-calculated tax and the ERP-calculated
        //        // tax, take the cloud as gospel and apply the difference to the manually set taxes
        //        // on the ERP side. The difference is presumed to be small and we simply find an
        //        // appropriate authority to apply it to. We use the logic that if the tax amount for
        //        // the authority is zero, it probably had a rate of zero or was tax exempt, and so
        //        // we should apply the difference to the first authority with a non-zero tax amount.
        //        //
        //        // (Edge Case: If for some reason the cloud calculated a non-zero tax total but the ERP
        //        // calculated a zero tax total, then all the amounts for the authorities will be zero
        //        // and the above algorithm would wind up setting the difference into the last tax
        //        // authority bucket, which may be a blank/invalid authority. So, for this edge case,
        //        // we'll just put it into the first (non-blank) authority.)
        //    if (taxTotalDelta != 0M)
        //    {
        //        // Find the first non-zero tax amount and apply the difference there.
        //        if (header.Tax1 != 0M)
        //            header.Tax1 += taxTotalDelta;
        //        else if (header.Tax2 != 0M)
        //            header.Tax2 += taxTotalDelta;
        //        else if (header.Tax3 != 0M)
        //            header.Tax3 += taxTotalDelta;
        //        else if (header.Tax4 != 0M)
        //            header.Tax4 += taxTotalDelta;
        //        else if (header.Tax5 != 0M)
        //            header.Tax5 += taxTotalDelta;
        //        else
        //            // Edge case of ERP calculating tax total of zero even though the cloud did not.
        //            AssignTaxByNonBlankAuthority(taxTotalDelta, header);

        //        // Calculate a tax base using the values from the cloud. Note that the cloud is not currently
        //        // storing the tax included amounts, so this will be off if using tax included pricing.
        //        // If the ERP calculates a tax base as zero (e.g. tax exempt) then just leave it as zero.

        //        decimal taxBase;
        //        if (model.GetType() == typeof (Order))
        //            taxBase = model.AmountPaid - model.Tax; //model.OrderTotal: switch to amountPaid as OrderTotal has 3 decimal, which is wrong
        //        else
        //            taxBase = model.Total - model.Tax;
        //        if (header.TaxBase1 != 0) header.TaxBase1 = taxBase;
        //        if (header.TaxBase2 != 0) header.TaxBase2 = taxBase;
        //        if (header.TaxBase3 != 0) header.TaxBase3 = taxBase;
        //        if (header.TaxBase4 != 0) header.TaxBase4 = taxBase;
        //        if (header.TaxBase5 != 0) header.TaxBase5 = taxBase;

        //        // Distribute any adjustments we made in the header taxes to the details.
        //        header.DistributeTax();
        //    }
        //}


        private static void AssignTaxByNonBlankAuthority(decimal taxTotalDelta, dynamic header)
        {
            bool assignSucceeded = false;
            decimal oldValue;

            // Note that the tax class may be tax exempt for a given authority, in which case the PUT to 
            // Sage 300 tax field would fail, so we simply check that the assignment 'stuck' by re-reading it.
            // TODO: Ideally should open the TaxClassView and read the tax class settings instead of checking that assignment stuck.
            if (!String.IsNullOrEmpty(header.TaxAuthority1))
            {
                oldValue = header.Tax1;
                header.Tax1 += taxTotalDelta;
                if (oldValue == header.Tax1 + taxTotalDelta)
                    assignSucceeded = true;
            }
            if (!assignSucceeded && !String.IsNullOrEmpty(header.TaxAuthority2))
            {
                oldValue = header.Tax2;
                header.Tax2 += taxTotalDelta;
                if (oldValue == header.Tax2 + taxTotalDelta)
                    assignSucceeded = true;
            }
            if (!assignSucceeded && !String.IsNullOrEmpty(header.TaxAuthority3))
            {
                oldValue = header.Tax3;
                header.Tax3 += taxTotalDelta;
                if (oldValue == header.Tax3 + taxTotalDelta)
                    assignSucceeded = true;
            }
            if (!assignSucceeded && !String.IsNullOrEmpty(header.TaxAuthority4))
            {
                oldValue = header.Tax4;
                header.Tax4 += taxTotalDelta;
                if (oldValue == header.Tax4 + taxTotalDelta)
                    assignSucceeded = true;
            }
            if (!assignSucceeded && !String.IsNullOrEmpty(header.TaxAuthority5))
            {
                oldValue = header.Tax5;
                header.Tax5 += taxTotalDelta;
                if (oldValue == header.Tax5 + taxTotalDelta)
                    assignSucceeded = true;
            }
            if (!assignSucceeded)
            {
                // Should not get here unless an ERP user has changed the tax setup while a cloud user was entering a work order.
                throw (new Exception("Cannot process taxes. The web application calculated a non-zero tax but Sage 300 ERP reports all tax authorities as exempt."));
            }
        }
    }

    /// <summary>
    /// todo kms:  what is this going to look like for pegasus?
    /// </summary>
    public enum PaymentMethod
    {
        CreditCard,
        Check,
        Cash
    }
}
