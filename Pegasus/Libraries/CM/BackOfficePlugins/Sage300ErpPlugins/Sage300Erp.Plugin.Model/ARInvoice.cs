/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the A/R Invoice (and related entities)
    /// </summary>
    public static class ARInvoices 
    {
        //todo kms: determine if this is necessary
        //public const string EntityName = EntityDisplayName.Invoices;

        public const string HeaderOptionalView = "AR0402";
        public const string DetailOptionalView = "AR0401";
        public const string PaymentScheduleView = "AR0034";
        public const string CustomerBalanceView = "AR0160";
        public const string AROBLDocumentView = "AR0036";

        #region A/R Invoice Batch View
        public const string BatchView = "AR0031";

        public const string BatchNumber = "CNTBTCH";
        public const string BatchType = "BTCHTYPE";
        public const string BatchDescription = "BTCHDESC";
        public const string SourceApplication = "SRCEAPPL";
 
        public const int IndexBatchStatus = 1;
        public const int BatchType_Entered = 1;
        public const int BatchType_External = 5;
        public const int BatchStatus_Open = 1;
        public const string Filter_PostedAROEInvoices = "BTCHSTTS = 3";
        #endregion

        #region A/R invoice header view
        public const int Index_DocumentNo = 2;
        public const string InvoiceView = "AR0032";
        public const string TransactionType = "IDTRX";
        public const string InvoiceDate = "DATEINVC";
        public const string DueDate = "DATEDUE";
        public const string AssignedTo = "CODESLSP1";  
        public const string Customer = "IDCUST";
        public const string PONumber = "CUSTPO";
        public const string OrderNumber = "ORDRNBR";
        public const string Terms = "TERMCODE";
        public const string Note = "SPECINST";
        public const string SubTotal = "AMTINVCTOT";
        public const string InvoiceTotal = "AMTNETTOT";
        public const string TaxTotal = "AMTTAXTOT";
        public const string Tax1 = "AMTTAX1";
        public const string Tax2 = "AMTTAX2";
        public const string Tax3 = "AMTTAX3";
        public const string Tax4 = "AMTTAX4";
        public const string Tax5 = "AMTTAX5";
        public const string TaxBase1 = "BASETAX1";
        public const string TaxBase2 = "BASETAX2";
        public const string TaxBase3 = "BASETAX3";
        public const string TaxBase4 = "BASETAX4";
        public const string TaxBase5 = "BASETAX5";
        public const string TaxAuthority1 = "CODETAX1";
        public const string TaxAuthority2 = "CODETAX2";
        public const string TaxAuthority3 = "CODETAX3";
        public const string TaxAuthority4 = "CODETAX4";
        public const string TaxAuthority5 = "CODETAX5";
        public const string TaxGroup = "CODETAXGRP";
        public const string PaymentDiscount = "AMTDISCAVL";
        public const string Payment = "AMTPPD";
        public const string DiscountDueDate = "DATEDISC";
        public const string ShipViaDesc = "SHPVIADESC";
        public const string NumberOfPayments = "AMTPAYMTOT";

        public const string LocationAddress1 = "SHPTOSTE1";
        public const string LocationAddress2 = "SHPTOSTE2";
        public const string LocationAddress3 = "SHPTOSTE3";
        public const string LocationAddress4 = "SHPTOSTE4";
        public const string LocationCity = "SHPTOCITY";
        public const string LocationStateProvince = "SHPTOSTTE";
        public const string LocationPostalCode = "SHPTOPOST";
        public const string LocationCountry = "SHPTOCTRY";
        public const string LocationPhone = "SHPTOPHON";
        public const string LocationEmail = "EMAIL";
        public const string LocationFax = "SHPTOFAX";
        public const string LocationContact = "SHPTOCTAC";
        public const string LocationContactEmail = "CTACEMAIL";
        public const string LocationContactPhoneNumber = "SHPTOPHON";
        public const string InvoiceType = "INVCTYPE";
        public const string InvoiceDescription = "INVCDESC";
        public const string ManualTaxCalculation = "SWMANTX";
        public const string Salesperson = "CODESLSP1";
        public const string InvoiceID = "IDINVC";
        public const string AMTPYMSCHD = "AMTPYMSCHD";
        public const string AMTPPD = "AMTPPD";
        public const string CTACPHONE = "CTACPHONE";
        public const string CTACFAX = "CTACFAX";
        public const string CTACEMAIL = "CTACEMAIL";
        public const string CODETAXGRP = "CODETAXGRP";
        public const string IDPPD = "IDPPD";
        public const string LASTLINE = "LASTLINE";

        public const string ProcessCommand = "PROCESSCMD";
        public enum Commands
        {
            CalculateTax = 0,
            DistributeTax = 1,
            InsertHeaderOptionalFields = 4
        }
        public enum DetailCommands
        {
            InsertDetailOptionalFields = 0
        }

        public const int InvoiceType_Item = 1;

        public const string FilterInvoices = "TEXTTRX = 1 AND CODECURN = \"{0}\" AND SWRTG = 0 AND SWJOB = 0 AND SWRTGINVC = 0 AND ERRENTRY = 0";

        #endregion

        #region A/R Invoice detail view
        public const string InvoiceDetailView = "AR0033";
        public const string LineItemID = "CNTLINE";
        public const string ItemNumber = "IDITEM";
        public const string DistributionCode = "IDDIST";
        public const string Description = "TEXTDESC";
        public const string UnitOfMeasure = "UNITMEAS";
        public const string Price = "AMTPRIC";
        public const string Total = "AMTTXBL";
        public const string Quantity = "QTYINVC";
        public const string Comment = "COMMENT";
        #endregion

        public const string DiscountSearch = "<<DiscountAmount=";
        public const string DisountCommentFormat = DiscountSearch + "{0}>>";
        public const string StartSymbol = "=";
        public const string EndSymbol = ">>";

        public const string AROBLFilter = "SWPAID = 0 AND TRXTYPETXT = 1 AND SWJOB = 0 AND SWRTG = 0 AND CODECURN = \"{0}\"";
        
        public const string ARInvoiceFilter = "IDINVC = \"{0}\" ";
        public const int Index_PaidSwitch = 6;
        
    }
}
