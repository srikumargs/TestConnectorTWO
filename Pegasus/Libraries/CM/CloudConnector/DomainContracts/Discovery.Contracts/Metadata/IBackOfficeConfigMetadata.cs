using Sage.Connector.DomainContracts.BackOffice.Metadata;
using System;

namespace Sage.Connector.Discovery.Contracts.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBackOfficeConfigMetadata : IBackOfficeData
    {
        /// <summary>
        /// The nice name of the back office plugin
        /// </summary>
        String BackOfficeName { get; }
        /// <summary>
        /// Valid Values {"X86", "AnyCPU", X64"}
        /// </summary>
        String Platform { get; }
    }
}
