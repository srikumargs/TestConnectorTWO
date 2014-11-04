using System;

namespace Connector.DomainMediator.Tests
{
    /// <summary>
    /// Sync Request Payload
    /// </summary>
    internal class SyncRequest: ISyncRequest
    {
        /// <summary>
        /// Resource Kind Name to be used for this synchronization
        /// </summary>
        public String ResourceKindName { get; set; }

        /// <summary>
        /// Cloud Tick value used by sync to determine changed entities for internal sync's
        /// </summary>
        public int CloudTick { get; set; }
   
    }
}
