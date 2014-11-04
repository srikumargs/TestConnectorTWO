/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARInvoiceBatchView : IBaseView
    {
        void GetOneBatch();
        void UpdateInvoiceBatch();
        string SourceApplication { get; }
        decimal BatchNumber { get; }

        IARInvoiceView Invoice { get; }
    }
}
