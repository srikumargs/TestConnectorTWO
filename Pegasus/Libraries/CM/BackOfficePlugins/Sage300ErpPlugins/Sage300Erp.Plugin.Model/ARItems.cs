/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the A/R Items (and related entities)
    /// </summary>
    public static class ARItems
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.ServiceTypes;

        #region A/R Items View
        public const string ItemsView = "AR0010";
        public const string ItemNo = "IDITEM";
        public const string Description = "TEXTDESC";
        public const string Status = "SWACTV";
        #endregion

        #region A/R Item Pricing View
        public const string PricingView = "AR0009";
        public const string CurrencyCode = "CODECURN";
        public const string Rate = "AMTPRICE";
        public const string UnitOfMeasure = "UNITMEAS";
        #endregion

        #region A/R Item Tax Class view
        public const string TaxView = "AR0011";
        public const string Authority = "CODETAX";
        public const string TaxClass = "TAXSTTS";

        #endregion
    }
}
