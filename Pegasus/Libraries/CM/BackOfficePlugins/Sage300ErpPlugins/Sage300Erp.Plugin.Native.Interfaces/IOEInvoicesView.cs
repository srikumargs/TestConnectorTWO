/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IOEInvoicesView : IInvoicesView
    {
        void ReadPaySchedule();
        decimal PaymentDiscount { get; set; }
    }
}
