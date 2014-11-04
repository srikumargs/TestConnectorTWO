/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/


namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the A/R Statements (and related entities)
    /// </summary
    public static class ARStatement
    {
        #region Aged Document Super View and Fields
        public const string AgedTrialViewID = "AR0055";
        public const string RUNDATE = "RUNDATE";
        public const string CUTOFFDATE = "CUTOFFDATE";
        public const string CMNDCODE = "CMNDCODE";
        public const string INDEX1 = "INDEX1";
        public const string FIELDNAME1 = "FIELDNAME1";
        public const string FIELDNAME2 = "FIELDNAME2";
        public const string SELFIELD1 = "SELFIELD1";
        public const string SELFROM1 = "IDFROM1";
        public const string SELTO1 = "IDTO1";
        public const string SELFROM2 = "IDFROM2";
        public const string INDEX2 = "INDEX2";
        public const string SELTO2 = "IDTO2";
        public const string AGEINVDTSW = "AGEINVDTSW";
        public const string AGEPERIOD2 ="AGEPERIOD2";
        public const string AGEPERIOD3 = "AGEPERIOD3";
        public const string AGEPERIOD4 = "AGEPERIOD4";
        public const string SWMATCHING = "SWMATCHING";
        public const string ZEROBALSW = "ZEROBALSW";
        public const string INCLPAIDSW = "INCLPAIDSW";
        public const string AGESEQ = "AGESEQ";
        public const string FROMDATE = "FROMDATE";
        public const int CommandCode_GenerateUnsortedWorkFile = 52;
        public const int AgeByDueDate = 0;
        #endregion
 
        #region Aged Document View
        public const string AgedDocViewID = "AR0125";
        public const string Filter = "AGESEQ = {0} AND (RECTYPE != 1 OR TRXTYPETXT != 20)";
        public const string RECORDNO = "RECORDNO";
        public const string IDCUST = "IDCUST";
        public const string IDRMIT = "IDRMIT";
        public const string IDMEMOXREF= "IDMEMOXREF";
        public const string IDINVC = "IDINVC";
        public const string ADJNO = "ADJNO";
        public const string RECTYPE = "RECTYPE";
        public const string CNTSEQ = "CNTSEQ";
        public const string AMTINVCTC = "AMTINVCTC";
        public const string DATEDUE = "DATEDUE";
        public const string DATEBUS = "DATEBUS";
        public const string DATEINVC = "DATEINVC";
        public const string TOTBKWDTC = "TOTBKWDTC";
        public const string AMTBALDUET = "AMTBALDUET";
        public const string TOTFWDTC = "TOTFWDTC";
        public const string AMTDUE1TC = "AMTDUE1TC";
        public const string AMTDUE2TC = "AMTDUE2TC";
        public const string AMTDUE3TC = "AMTDUE3TC";
        public const string AMTDUE4TC = "AMTDUE4TC";
        public const string AMTDUE5TC = "AMTDUE5TC";
        public const string AMTDUE6TC = "AMTDUE6TC";
        public const string AMTDUE7TC = "AMTDUE7TC";
        public const string AMTDUE8TC = "AMTDUE8TC";
        public const string AMTDUE9TC = "AMTDUE9TC";
        public const string TRXTYPETXT = "TRXTYPETXT";
        public const int DocType_Receipt = 11;
        public const int DocType_CreditNote = 3;
        public const int DocType_Invoice = 1;
        public const int DocType_Prepayment = 10;
        public const int DocType_Adjustment = 14;
        public const int DocType_DebitNote = 2;
        #endregion

        public const string BalanceForwardDescription = "Balance Forward";
    }
}
