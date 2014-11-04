using System.Collections.Generic;
using Sage.Connector.DomainContracts.BackOffice;

namespace BackOfficePluginTest.Core
{
    /// <summary>
    /// Class to help provide IBackOfficeCompanyData to the plugins
    /// </summary>
    /// <remarks>
    /// Consider if we want this internal and friended.
    /// </remarks>
    public class BackOfficeCompanyData : IBackOfficeCompanyData
    {
        /// <summary>
        /// BackOfficeId 
        /// </summary>
        public string BackOfficeId
        { get; set; }

        /// <summary>
        /// ConnectionCredentials
        /// </summary>
        public IDictionary<string, string> ConnectionCredentials
        { get; set; }


    }
}
