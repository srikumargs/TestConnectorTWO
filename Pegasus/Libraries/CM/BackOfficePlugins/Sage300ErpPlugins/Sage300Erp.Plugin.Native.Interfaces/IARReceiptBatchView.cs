/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;
using System.Collections.Generic;


namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARReceiptBatchView : IBaseView
    {
        void GetOneBatch(string sourceApp, string currency);        
        decimal BatchNumber { get; set;}
        DateTime BatchDate  { get; set;}
        string Bank { get; set;}
        string Currency { get; set;}
        string RateType { get; set;}
        DateTime BankRateDate  { get; set;}
        decimal BankExchangeRate { get; set;}
        IARReceiptView ReceiptEntries { get; }

    }
}
