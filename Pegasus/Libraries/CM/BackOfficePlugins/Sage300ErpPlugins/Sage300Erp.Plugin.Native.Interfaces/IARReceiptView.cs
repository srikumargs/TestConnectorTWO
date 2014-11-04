/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARReceiptView : IBaseView
    {
        decimal AmountPaid { set; get; }
        string CheckReceiptNo { set; }
        string Customer { set; get;}
        string PaymentCode { set; get; }
        string Reference { set; get; }
        string DocumentNumber { set; get; }
        string EntryDescription { set; get; }
        PaymentType PaymentType { set; get; }
        DateTime ReceiptDate { set; get; }
        void GenerateNewReceipt(string sourceApp, int transactionType);
        void CreateOpenDocumentList();
        void ApplyPayment(string documentNo, out decimal termDiscountApplied, out DateTime discountDueDate, DateTime paymentDate);
        string ApplyTo { set; }
        void CreatePrepayment();
        string GetKeyReference();
        void CalculatePendingBalances(out decimal termDiscountAvailable, out DateTime discountDueDate, out DateTime dueDate, out decimal balanceDue, string customer, string documentNo);
        object AppliedReceiptView();
        void AutoApplyPayment(decimal amount);
    }
}
