/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IOrderDetailsView : IBaseView
    {
        short DetailNumber { get; }
        void InsertNewDetail(LineType type, string code);
        string Item { get; set; }
        string MiscCharge { get; set; }
        decimal QtyOrdered { get; set; }
        LineType Type { get; set; }
        decimal OrderedUnitPrice { get; set; }
        string UnitofMeasaure { get; set; }
        decimal ExtendedAmount { get; set; }
        decimal PricingUnitPrice { get; set; }
        decimal UnitConverter { get; }
        decimal PricingUnitConverter { get; }
    }
}
