/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface ITaxRateView : IBaseView
    {
        string Authority { get; }
        int BuyerClass { get; }
        decimal[] ItemRates { get; }  // up to 10 rates
    }
}
