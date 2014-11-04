/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the Items (and related entities)
    /// </summary>
    public static class Items
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.InventoryItems;


        #region Items View
        public const string ItemsView = "IC0310";
        public const string ItemNo = "ITEMNO";
        public const string FormattedItemNo = "FMTITEMNO";
        public const string Name = "DESC";
        public const string DefaultPriceList = "DEFPRICLST";
        public const string Status = "INACTIVE";
        public const string Kitting = "KITTING";
        public const int Index_FormattedItemNo = 3;
        #endregion

        #region Item Locations Super View
        public const string LocationsView = "IC0374";
        public const string QuantityOnHand = "QTONHANDA";
        #endregion

        #region Item Pricing View
        public const string PricingView = "IC0480";
        public const string CurrencyCode = "CURRENCY";
        public const string PriceList = "PRICELIST";
        public const string PriceStd = "DBASEPRICE";
        public const string UnitOfMeasure = "DBASEUNIT";
        #endregion

        #region Item Unit View
        public const string UnitView = "IC0750";
        public const string Conversion = "CONVERSION";
        public const string Unit = "UNIT";
        #endregion

        public const string BomView = "IC0200";
        public const string Bom_Filter = "ITEMNO = \"{0}\" ";
    }
}
