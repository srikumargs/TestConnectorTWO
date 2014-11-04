/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the Customers (and related entities)
    /// </summary>
    public static class Customers
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.Customers;

        #region A/R Customers View

        public const string ViewID = "AR0024";

        public const string ID = "IDCUST";
        public const string Name = "NAMECUST";
        public const string Address1 = "TEXTSTRE1";
        public const string Address2 = "TEXTSTRE2";
        public const string Address3 = "TEXTSTRE3";
        public const string Address4 = "TEXTSTRE4";
        public const string City = "NAMECITY";
        public const string State = "CODESTTE";
        public const string PostalCode = "CODEPSTL";
        public const string Country = "CODECTRY";
        public const string Email = "EMAIL2";
        public const string URL = "WEBSITE";
        public const string Phone = "TEXTPHON1";
        public const string Contact = "NAMECTAC";
        public const string ContactPhone = "CTACPHONE";
        public const string ContactEmail = "EMAIL1";
        public const string Status = "SWACTV";
        public const string Currency = "CODECURN";
        public const string CheckCreditLimit = "SWCHKLIMIT";
        public const string CreditLimit = "AMTCRLIMT";
        public const string BalanceDue = "AMTBALDUET";
        public const string CreditOnHold = "SWHOLD";
        public const string TermCode = "CODETERM";
        public const string TaxGroup = "CODETAXGRP";
        public const string TaxClass1 = "TAXSTTS1";
        public const string TaxClass2 = "TAXSTTS2";
        public const string TaxClass3 = "TAXSTTS3";
        public const string TaxClass4 = "TAXSTTS4";
        public const string TaxClass5 = "TAXSTTS5";
        public const string SalesPersonCode = "CODESLSP1";
        public const string PrintStatement = "SWPRTSTMT";
        public const string AccountType = "SWBALFWD";
        public const string LastStatementDate = "DATELASTST";
        public const string BalanceForward = "AMTBALFWDT";
        #endregion

    }
}
