/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IItemsView : IBaseView
    {
        string FormattedItemNo { get; set; }
        string ItemNo { get; }
        string Name { get; }
        void Pricing(string currency, out decimal priceStd, out string uom);
        decimal Quantity(string uom);
        StatusType Status { get; }
        string GetUnformattedItemNo(string itemno);
        bool IsKittingItem();
        bool IsBomItem();

    }
}
