/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;

namespace Sage300ERP.Plugin.Native.Interfaces
{

    public interface IARItemPricingView : IBaseView
    {
        string currency { set; }
        decimal rate { get; }
        string uom { get; }

    }

}

