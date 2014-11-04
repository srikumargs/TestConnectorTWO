/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;

namespace Sage300Erp.Plugin.Model
{
    /// <summary>
    /// Class that contains the Sage 300 ERP-specific definitions for the AR Statement Option (and related entities)
    /// </summary>
    public static class ARStatementOption
    {
        public const string ViewID = "AR0004";

        #region View Field Indices
        public const Int32 IDX_IDR04 = 1;
        public const Int32 IDX_DATEMNTN = 2;
        public const Int32 IDX_AGINPERD1 = 3;
        public const Int32 IDX_AGINPERD2 = 4;
        public const Int32 IDX_AGINPERD3 = 5;
        public const Int32 IDX_AGECR = 6;
        public const Int32 IDX_AGEUAPL = 7;
        public const Int32 IDX_TEXTSTMT1 = 8;
        public const Int32 IDX_TEXTSTMT2 = 9;
        public const Int32 IDX_TEXTSTMT3 = 10;
        public const Int32 IDX_TEXTSTMT4 = 11;
        public const Int32 IDX_TEXTSTMT5 = 12;
        public const Int32 IDX_PRTZEROBAL = 13;
        #endregion

        public enum ValueIDX 
        { 
            AgePeriod1,
            AgePeriod2,
            AgePeriod3,
            CurrentDunningMsg,
            Period1DunningMsg,
            Period2DunningMsg, 
            Period3DunningMsg,
            PeriodOverDunningMsg,
            PrintZero,
            AgeBy,
            Days,
            Currency
        };
    }
}
