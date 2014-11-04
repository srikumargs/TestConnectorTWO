/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the Tax Classes
    /// </summary>
    public static class TaxClasses
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.TaxCodeClasses;

        public const string ViewID = "TX0001";
        public const string Authority = "AUTHORITY";
        public const string Description = "DESC";
        public const string ClassType = "CLASSTYPE";  // Transaction Type (Sales, Purchases)
        public const string ClassAxis = "CLASSAXIS";  // Class Type (Customers, Items)
        public const string Class = "CLASS";
        public const string Exempt = "EXEMPT";

        public const string FilterByAuthority = "AUTHORITY = \"{0}\" AND CLASSTYPE = 1";  // CLASSTYPE (Transaction Type) is fixed to Sales.
    }
}
