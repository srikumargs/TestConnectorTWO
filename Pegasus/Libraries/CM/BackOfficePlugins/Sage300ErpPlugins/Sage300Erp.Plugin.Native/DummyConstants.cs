using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage300Erp.Plugin.Native
{
    /// <summary>
    /// Stores constants that will be used for non-nullable fields.
    /// </summary>
    public static class DummyConstants
    {
        //TODO: this is a workaround because the Nephos.Model isn't permitting null values in the email field
        public const string Name = "<no name entered>";
        public static string Street1 = "<no street address entered>";
    }
}
