/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARItemsView : IBaseView
    {
        string ARItemNo { get; }
        string Description { get; }
        void Pricing(string currency, out decimal price, out string uom);
        string ShortDesc { get; }
        bool Taxable { get; }
        StatusType Status { get; }
        IARItemPricingView itemPricing { set;}
        IARItemTaxView itemTax { set; }
    }

}

