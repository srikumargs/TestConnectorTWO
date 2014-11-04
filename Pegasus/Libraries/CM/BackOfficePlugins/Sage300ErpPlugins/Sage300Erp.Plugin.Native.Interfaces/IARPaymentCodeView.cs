﻿/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IARPaymentCodeView : IBaseView
    {
        void CheckPaymentTypeOnDefaultPaymentCode(dynamic type, string reference);
    }
}