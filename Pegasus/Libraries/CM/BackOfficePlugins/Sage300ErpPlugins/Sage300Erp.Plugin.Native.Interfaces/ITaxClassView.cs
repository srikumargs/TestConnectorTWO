/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface ITaxClassView : IBaseView
    {
        string Authority { get; }
        int ClassAxis { get; }
        int Class { get; }
        string Description { get; }
        bool Exempt { get; }
    }
}
