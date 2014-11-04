/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using System.Collections.Generic;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARAgedTrialBalanceView : IBaseView
    {
        bool ProcessStatement(DateTime statementDate, string customer, List<object> statememtOptions);
        int AgeSequence { get; }
    }
}
