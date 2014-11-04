/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IAROBLDocumentView  : IBaseView
    {
        string InvoiceNumber { get; }
        string CustomerNumber { get; }
        string SourceApplication { get; }
    }
    
}
