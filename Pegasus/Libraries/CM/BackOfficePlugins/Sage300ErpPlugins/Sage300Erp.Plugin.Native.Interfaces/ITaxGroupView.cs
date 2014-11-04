/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using Sage300ERP.Plugin.Native.Types;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface ITaxGroupView : IBaseView
    {
        string GroupID { get; }
        int TType { get; }
        string Description { get; }
        string[] Authorities { get; }  // up to 5 authorities
    }
}
