/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the Tax Rates
    /// </summary>
    public static class TaxRates
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.TaxCodeDetails;

        public const string ViewID = "TX0004";
        public const string Authority = "AUTHORITY";
        public const string TType = "TTYPE";  // Transaction Type
        public const string BuyerClass = "BUYERCLASS";
        public const string ItemRate1 = "ITEMRATE1";
        public const string ItemRate2 = "ITEMRATE2";
        public const string ItemRate3 = "ITEMRATE3";
        public const string ItemRate4 = "ITEMRATE4";
        public const string ItemRate5 = "ITEMRATE5";
        public const string ItemRate6 = "ITEMRATE6";
        public const string ItemRate7 = "ITEMRATE7";
        public const string ItemRate8 = "ITEMRATE8";
        public const string ItemRate9 = "ITEMRATE9";
        public const string ItemRate10 = "ITEMRATE10";

        public const string FilterByAuthority = "AUTHORITY = \"{0}\" AND TTYPE = 1";  // TTYPE (Transaction Type) is fixed to Sales.
    }
}
