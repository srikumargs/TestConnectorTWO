/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IOrdersView : IBaseView
    {
        void TurnOffAutoTaxCalculation();
        void DistributeTax();
        void SetPaymentLock();
        void ShipAll();
        bool IsOverCreditLimit();
        void CalculateTax();
        void SetTaxBaseFromNephos(decimal taxBase);
        void SetTaxAmountFromNephos(decimal TaxAmount);
        bool CreateInvoice { get; set; }
        string Customer { get; set; }
        string Description { get; set; }
        IOrderDetailsView Details { get; }
        decimal DiscountAmount { get; set; }
        decimal DiscountPercentage { get; set; }
        DateTime ExpiryDate { get; set; }
        void InsertNewOrder(OrderType type);
        short LinesOnOrder { get; }
        DateTime OrderDate { get; set; }
        string OrderNumber { get; set; }
        string ShippingAddress1 { get; set; }
        string ShippingAddress2 { get; set; }
        string ShippingAddress3 { get; set; }
        string ShippingAddress4 { get; set; }
        string ShippingCity { get; set; }
        string ShippingContact { get; set; }
        string ShippingContactEmail { get; set; }
        string ShippingContactPhoneNumber { get; set; }
        string ShippingCountry { get; set; }
        string ShippingEmail { get; set; }
        string ShippingFax { get; set; }
        string ShippingName { get; set; }
        string ShippingPhoneNumber { get; set; }
        string ShippingPostalCode { get; set; }
        string ShippingStateProvince { get; set; }
        string ShipToLocation { get; set; }
        DateTime ActualShipmentDate { get; set; }
        decimal Subtotal { get; }
        decimal TaxTotal { get; }
        decimal Total { get; }
        OrderType Type { get; set; }
        decimal Uniquifier { get; set; }
        bool OnHold { get; set; }
        string BillName { get; }
        string OrderSourceCurrency { get; }
        decimal OrderRate { get; }
        decimal OrderRateDate { get; }
        string OrderRateType { get; }
        int OrderRateOperator { get; }
        decimal OrderTotol { get; }
        decimal DiscountAvailable { get; set; }
        decimal AmountDueLessPrepayment { get; }
        decimal ReceiptBatchNumber { set; }
        string ReceiptType { set; }
        string CheckNumber { set; }
        string BankCode { set; }
        string PaymentSourceCurrency { set; }
        string PaymentRateType { set; }
        DateTime PaymentRateDate { set; }
        decimal PaymentRate { set; }
        decimal BankPayment { get;  set; }
        PaymentType OrderPaymentType { get;  set; }
        DateTime CheckDate { set; }
        string ApproveBy { set; }
        string ApprovePassword { set; }
        string LastInvoiceNumber { get; set; }
        DateTime InvoiceDate { get; set; }
        DateTime ShipmentDate { get; set; }
        string Terms { get; set; }
        string TaxGroup { get; set; }
        decimal TotalPayment { get; set; }
        decimal Tax1 { get; set; }
        decimal Tax2 { get; set; }
        decimal Tax3 { get; set; }
        decimal Tax4 { get; set; }
        decimal Tax5 { get; set; }
        decimal TaxBase1 { get; set; }
        decimal TaxBase2 { get; set; }
        decimal TaxBase3 { get; set; }
        decimal TaxBase4 { get; set; }
        decimal TaxBase5 { get; set; }
        string TaxAuthority1 { get; set; }
        string TaxAuthority2 { get; set; }
        string TaxAuthority3 { get; set; }
        string TaxAuthority4 { get; set; }
        string TaxAuthority5 { get; set; }
    }
}
