/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;


namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARItemTaxView : IBaseView
    {
        string Item { get; }
        string Authority { get; }
        int TaxClass { get; }
    }
}
