/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

namespace Sage300ERP.Plugin.Native.Types
{
    /// <summary>
    /// The transaction type for an A/R Invoice 
    /// </summary>
    public enum ARInvoiceTransactionType
    {
        InvoiceItemIssued = 11,
        InvoiceSummaryEntered = 12,
        InvoiceRecurringCharge = 13,
        InvoiceSummaryIssued = 14,
        InvoiceItemEntered = 15,
        DebitNoteItemIssued = 21,
        DebitNoteSummaryEntered = 22,
        DebitNoteSummaryIssued = 24,
        DebitNoteItemEntered = 25,
        CreditNoteItemIssued = 31,
        CreditNoteSummaryEntered = 32,
        CreditNoteSummaryIssued = 34,
        CreditNoteItemEntered = 35,
        InterestCharge = 40
    };
}
