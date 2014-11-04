/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the Tax Groups
    /// </summary>
    public static class TaxGroups
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.TaxSchedules;

        public const string ViewID = "TX0003";
        public const string TType = "TTYPE";  // Transaction Type
        public const string GroupID = "GROUPID";
        public const string Description = "DESC";
        public const string Authority1 = "AUTHORITY1";
        public const string Authority2 = "AUTHORITY2";
        public const string Authority3 = "AUTHORITY3";
        public const string Authority4 = "AUTHORITY4";
        public const string Authority5 = "AUTHORITY5";

        public const int MaxNumberOfAuthorities = 5;
        public const string FilterBySales = "TTYPE = 1";  // TTYPE (Transaction Type) is fixed to Sales.
        public const string FilterByGroupID = "GROUPID = \"{0}\" AND TTYPE = 1";  // TTYPE (Transaction Type) is fixed to Sales.
        public const int SalesType = 1;

    }
}
