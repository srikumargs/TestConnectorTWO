using System;

namespace Connector.DomainMediator.Tests
{
    /// <summary>
    /// Sync Request
    /// </summary>
    internal interface ISyncRequest
    {
        /// <summary>
        /// Resource Kind Name to be used for this synchronization
        /// </summary>
        String ResourceKindName { get; set; }

        /// <summary>
        /// Cloud Tick for the requested resource
        /// </summary>
        int CloudTick { get; set; }
    }
}
