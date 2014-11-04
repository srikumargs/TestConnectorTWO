/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IInvoiceDetailsView : IBaseView
    {
        string LineItemID { get; set; }
        LineType LineType { get; set; }
        string ItemNumber { get; set;}
        string ItemDescription { get; set; }
        string UnitOfMeasure { get; set; }
        decimal Quantity { get; set; }
        decimal QuantityShipped { get; set; }
        decimal QuantityBackOrdered { get; set; }
        string Warehouse { get; set; }
        decimal Price { get; set; }
        decimal DiscountAmount { get; set; }
        decimal Total { get; set; }
        string Comment { get; set; }
    }
}
