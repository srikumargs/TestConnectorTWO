/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/



namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the O/E Invoice (and related entities)
    /// </summary>
    public static class OEInvoices
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.Invoices;

        #region OE invoice header view
        public const int Index_InvoiceNumber = 6;
        public const string InvoiceView = "OE0420";
        public const string InvoiceNumber = "INVNUMBER";
        public const string InvoiceDate = "INVDATE";
        public const string OrderNumber = "ORDNUMBER";
        public const string OrderDate = "ORDDATE";
        public const string PONumber = "PONUMBER";
        public const string Salesperson = "SALES1NAME";
        public const string ShipDate = "SHIPDATE";
        public const string ShipViaDesc = "VIADESC";
        public const string Customer = "CUSTOMER";
        public const string Terms = "TERMDESC";
        public const string Comments = "COMMENT";
        public const string FOB = "FOB";
        public const string SubTotal = "INVNTDTDIS";
        public const string Discount = "INVDISCAMT";

        public const string Miscellaneous = "INVMISC";
        public const string Taxes = "INVTAXTOT";
        public const string Total = "INVNETWTX";
        public const string Payment = "ORDPAYTOT";
        public const string PaymentDiscount = "DISCAVAIL";

        public const string BillToContactID = "BILCONTACT";
        public const string BillToName = "BILCONTACT";
        public const string BillToStreet1 = "BILADDR1";
        public const string BillToStreet2 = "BILADDR2";
        public const string BillToStreet3 = "BILADDR3";
        public const string BillToStreet4 = "BILADDR4";
        public const string BillToCity = "BILCITY";
        public const string BillToStateProvince = "BILSTATE";
        public const string BillToPostalCode = "BILZIP";
        public const string BillToCountry = "BILCOUNTRY";
        public const string BillToPhone = "BILPHONE";
        public const string BillToEmail = "BILEMAIL";
        public const string ShipToContactID = "SHPCONTACT";
        public const string ShipToName = "SHPCONTACT";
        public const string ShipToStreet1 = "SHPADDR1";
        public const string ShipToStreet2 = "SHPADDR2";
        public const string ShipToStreet3 = "SHPADDR3";
        public const string ShipToStreet4 = "SHPADDR4";
        public const string ShipToCity = "SHPCITY";
        public const string ShipToStateProvince = "SHPSTATE";
        public const string ShipToPostalCode = "SHPZIP";
        public const string ShipToCountry = "SHPCOUNTRY";
        public const string ShipToPhone = "SHPPHONE";
        public const string ShipToEmail = "SHPEMAIL";

        #endregion

        #region Invoice detail view
        public const string InvoiceDetailView = "OE0400";
        public const string LineType = "LINETYPE";
        public const string LineItemID = "LINENUM";
        public const string UnformattedItemNumber = "UNFMTITEM";
        public const string ItemNumber = "ITEM";
        public const string MiscellaneousChargeCode = "MISCCHARGE";
        public const string ItemDescription = "DESC";
        public const string UnitOfMeasure = "INVUNIT";
        public const string Quantity = "QTYORDERED";
        public const string QuantityShipped = "QTYSHIPPED";
        public const string QuantityBackOrdered = "QTYBACKORD";
        public const string Warehouse = "LOCDESC";
        public const string Price = "UNITPRICE";
        public const string ItemDiscountRate = "DISCPER";
        public const string ItemDiscountAmount = "INVDISC";
        public const string ItemTotal = "EDCINVMISC";
        public const string PRPRICEBY = "PRPRICEBY";
        public const string EXTINVMISC = "EXTINVMISC";
        public const string HasComment = "COMMINST";
        public const string DetailNo = "DETAILNUM";
        public const string INVLINES = "INVLINES";
        #endregion

        #region Payment Schedule view
        public const string PaymentScheduleView = "OE0720";
        public const string InvoiceDueDate = "DUEDATE";
        public const string DiscountDueDate = "DISCDATE";
        #endregion

        #region Comment View
        public const string CommentView = "OE0160";
        public const string UNIQUIFIER = "UNIQUIFIER";
        public const string COIN = "COIN";
        public const string COINTYPE = "COINTYPE";
        public const string Comment_Filter = "COINTYPE = 1 AND DETAILNUM = {0}";
        #endregion

    }
}
