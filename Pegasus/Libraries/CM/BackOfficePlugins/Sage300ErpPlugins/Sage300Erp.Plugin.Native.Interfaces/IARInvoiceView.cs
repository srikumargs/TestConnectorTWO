/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARInvoiceView : IInvoicesView
    {
        bool IsOverCreditLimit();
        void InsertNewInvoice();
        void TurnOffAutoTaxCalculation();
        void DistributeTax();
        void CalculateTax();
        bool CustomerExists(string customer);
        bool ItemDetailType { get; }
        ARInvoiceTransactionType TransactionType { get; }
        string Description { get; set; }
        string Note { get; set; }
        string LocationContactEmail { get; set; }
        string LocationContactPhoneNumber { get; set; }
        string LocationFax { get; set; }
        string ContactEmail { get; set; }
        string ContactFax { get; set; }
        string ContactPhone { get; set; }
        string Term { set; }
        decimal Tax1 { get; set; }
        decimal Tax2 { get; set; }
        decimal Tax3 { get; set; }
        decimal Tax4 { get; set; }
        decimal Tax5 { get; set; }
        decimal TaxBase1 { get; set; }
        decimal TaxBase2 { get; set; }
        decimal TaxBase3 { get; set; }
        decimal TaxBase4 { get; set; }
        decimal TaxBase5 { get; set; }
        string TaxAuthority1 { get; set; }
        string TaxAuthority2 { get; set; }
        string TaxAuthority3 { get; set; }
        string TaxAuthority4 { get; set; }
        string TaxAuthority5 { get; set; }
        decimal TaxTotal { get; set; }
        string TaxGroup { get; set; }
        string PrepaymentID { get; set; }
        decimal PrepaymentAmount { get; set; }
        decimal BalanceDue { get; }
        decimal PaymentDiscount { get; set; }
        string SourceApplication { get; }
        void SetIndex(int index);
        void CalculatePendingBalanceEtc(string customer, string invoice);
        IARInvoiceDetailView ARInvoiceDetails { get; }
    }
}
