/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;

namespace Sage300ERP.Plugin.Native.Interfaces
{

    public interface IARAgedDocView : IBaseView
    {
        decimal AgedBalance1 { get; }
        decimal AgedBalance2 { get; }
        decimal AgedBalance3 { get; }
        decimal AgedBalance4 { get; }
        decimal BalanceDue { get; }
        decimal CurrentBalance { get; }
        decimal CreditAvailable { get; }
        decimal CreditLimit { get; }
        string CustomerNumber{ get; }
        string CustomerName { get; }
        string CustomerCity { get; }
        string CustomerState { get; }
        string CustomerCountry { get; }
        string CustomerPostalCode { get; }
        string CustomerStreet1 { get; }
        string CustomerStreet2 { get; }
        string CustomerStreet3 { get; }
        string CustomerStreet4 { get; }
        string ContactName { get; }
        string SalesPersonName { get; }
        bool IsBalanceForward();
        DateTime LastStatementDate { get; }
        decimal BalanceForward { get; }

        string DocumentNumber { get; }
        int RecordNumber { get; }
        decimal Balance { get; }
        string Description { get; }
        DateTime DueDate { get; }
        decimal TransactionAmt { get; }
        DateTime TransactionDate { get; }
        string TransactionReference { get; }
        string SortValue { get; }
        string TransactionType { get; }
        int DocumentType { get; }

        string GetFilter(int sequence);

    }

}
