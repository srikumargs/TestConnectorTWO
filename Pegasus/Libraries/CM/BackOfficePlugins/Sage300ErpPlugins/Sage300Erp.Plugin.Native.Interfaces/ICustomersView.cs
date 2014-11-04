/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface ICustomersView: IBaseView
    {
        bool CustomerExists(string customer);
        bool CheckCustomerCurrency(string customer, string currency);
        string City { get; }
        string Contact { get; }
        string Country { get; }
        string Email { get; }
        string EmailWork { get; }
        string ExternalId { get; set; }
        string Name { get; }
        string Phone { get; }
        string PhoneWork { get; }
        string PostalCode { get; }
        string StateProvince { get; }
        string Street1 { get; }
        string Street2 { get; }
        string Street3 { get; }
        string Street4 { get; }
        string URL { get; }
        StatusType Status { get; }
        string CurrencyCode { get; }
        bool CheckCreditLimit { get; }
        decimal CreditLimit { get; }
        decimal BalanceDue { get; }
        bool CreditOnHold { get; }
        string PaymentTerms { get; }
        decimal CreditAvailable { get; }
        string TaxGroup { get; }
        int[] TaxClasses { get; }  // up to 5 classes
        string SalespersonName { get; }
        bool PrintStatement { get; }
        DateTime LastStatementDate { get; }
        AccountType AccountType { get; }
        decimal BalanceForward { get; }
    }
}
