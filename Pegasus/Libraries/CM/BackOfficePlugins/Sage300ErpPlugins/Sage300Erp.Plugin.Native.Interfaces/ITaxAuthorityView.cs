/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface ITaxAuthorityView : IBaseView
    {
        string Authority { get; }
        string Description { get; }
        decimal MinTax { get; }
        decimal MaxTax { get; }
    }
}
