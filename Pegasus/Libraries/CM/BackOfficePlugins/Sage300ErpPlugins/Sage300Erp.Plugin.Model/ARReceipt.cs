/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the A/R receipt batch
    /// </summary>
    /// 

    public static class ARReceipt
    {
        #region Receipt View IDs
        public const string ARBTAViewID = "AR0041";
        public const string ARTCRViewID = "AR0042";
        public const string ARTCNViewID = "AR0043";
        public const string ARTCPViewID = "AR0044";
        public const string ARTCUViewID = "AR0045";
        public const string ARTCROViewID = "AR0406";
        public const string ARTCCViewID = "AR0170";
        public const string ARPOOPViewID = "AR0061";
        #endregion     
   
        #region Receipt Batch view
        public const string filter = "CODEPYMTYP= \"CA\" AND BATCHSTAT=1 AND BATCHDESC = \"{0}\" AND SRCEAPPL = \"{1}\" ";
        public const string CODEPYMTYP = "CODEPYMTYP";
        public const string CNTBTCH = "CNTBTCH";
        public const string CNTITEM = "CNTITEM";
        public const string IDBANK = "IDBANK";
        public const string CODECURN = "CODECURN"; 
        public const string DATERATE = "DATERATE";
        public const string SRCEAPPL = "SRCEAPPL"; 
        public const string RATEEXCHHC = "RATEEXCHHC";
        public const string RATETYPE = "RATETYPE";
        public const string DATEBTCH = "DATEBTCH"; 
        public const string BATCHDESC = "BATCHDESC"; 
        public const string BATCHTYPE = "BATCHTYPE"; 
        public const string IDINVCMTCH = "IDINVCMTCH"; 
        public const int BatchType_External = 4;
        public const string SourceApp_OE = "OE"; 
        public const string SourceApp_AR = "AR"; 
        public const string CA = "CA";
        public const string ACH = "ACH";
        #endregion
        
        #region Receipt Entry view
        public const string RMITTYPE = "RMITTYPE";
        public const string DOCTYPE = "DOCTYPE";
        public const string AMTRMIT = "AMTRMIT";
        public const string TEXTRMIT = "TEXTRMIT";
        public const string IDRMIT = "IDRMIT";
        public const string CODEPAYM = "CODEPAYM"; 
        public const string DATERMIT = "DATERMIT";
        public const string PAYMTYPE = "PAYMTYPE";
        public const string IDCUST = "IDCUST";  
        public const string DOCNBR = "DOCNBR";  
        public const string AMTRMITTC = "AMTRMITTC";
        public const string TXTRMITREF = "TXTRMITREF";
        public const string ProcessCommand = "PROCESSCMD";
        public const int Receipt_Type = 1;   
        public const int Prepayment_Type = 2;  
        public const int Document_Type_Prepayment = 5;    
        public const int InsertOptionalFields = 0;      
        #endregion    
        
        #region Open Document List View  
        public const string ARPOOPfilter = "IDINVC = \"{0}\" ";   
        public const string APPLY = "APPLY";
        public const string PENDNGBAL = "PENDNGBAL";
        public const string OBSDISC = "OBSDISC";
        public const string PNDDSCTOT = "PNDDSCTOT";
        public const string PAYMAMT = "PAYMAMT";
        public const string DISCAMT = "DISCAMT";
        public const string DATEDISC = "DATEDISC";
        public const string Applied = "Y";
        public const string DATEDUE = "DATEDUE";
        public const string PROTYPE = "PROTYPE";
        #endregion

        #region ARTCP prepayment view
        public const string AMTPAYM = "AMTPAYM";
        #endregion
    }
}
