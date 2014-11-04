/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IInvoicesView : IBaseView
    {
         string ExternalID { get; set; }
         string InvoiceNumber { get; set; }
         DateTime InvoiceDate { get; set; }
         DateTime InvoiceDueDate { get; set; }
         string OrderNumber { get; set; }
         DateTime OrderDate { get; set; }
         string PONumber { get; set; }
         string SalespersonName { get; }
         DateTime ShipDate { get; set; }
         string ShipViaDescription { get; }
         string TermsDescription { get; }
         string Customer { get; set; }
         string Comments { get; set; }
         string FOB { get; set; }
         decimal SubTotal { get; set; }
         decimal Discount { get; set; }
         decimal Miscellaneous { get; set; }
         decimal Taxes { get; set; }
         decimal Total { get; set; }
         object DiscountDueDate { get; set; }

         string BillToName { get; set; }
         string BillToStreet1 { get; set; }
         string BillToStreet2 { get; set; }
         string BillToStreet3 { get; set; }
         string BillToStreet4 { get; set; }
         string BillToCity { get; set; }
         string BillToStateProvince { get; set; }
         string BillToPostalCode { get; set; }
         string BillToCountry { get; set; }
         string BillToPhone { get; set; }
         string BillToEmail { get; set; }

         string ShipToName { get; set; }
         string ShipToStreet1 { get; set; }
         string ShipToStreet2 { get; set; }
         string ShipToStreet3 { get; set; }
         string ShipToStreet4 { get; set; }
         string ShipToCity { get; set; }
         string ShipToStateProvince { get; set; }
         string ShipToPostalCode { get; set; }
         string ShipToCountry { get; set; }
         string ShipToPhone { get; set; }
         string ShipToEmail { get; set; }
         int DetailCount { get; }
         IInvoiceDetailsView InvoiceDetails { get; }
    }

}
