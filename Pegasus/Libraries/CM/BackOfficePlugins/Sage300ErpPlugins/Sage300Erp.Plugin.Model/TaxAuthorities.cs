/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the Tax Authorities
    /// </summary>
    public static class TaxAuthorities
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.TaxCodes;

        public const string ViewID = "TX0002";
        public const string Authority = "AUTHORITY";
        public const string Description = "DESC";
        public const string MinTax = "MINTAX";
        public const string MaxTax = "MAXTAX";
    }
}
