/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sage300ERP.Plugin.Native.Interfaces
{
    public interface IBrowseable
    {
        int Count { get; }

        int Order { get; set; }

        void Browse(string filter, bool ascending);
        bool GoTop();
        bool GoNext();

        bool Read(bool lockRecord);
    }
}
