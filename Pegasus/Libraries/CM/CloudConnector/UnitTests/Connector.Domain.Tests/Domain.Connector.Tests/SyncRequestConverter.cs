using System;
using Newtonsoft.Json.Converters;

namespace Connector.DomainMediator.Tests
{

    /// <summary>
    /// Sync Request converter for json
    /// </summary>
    internal class SyncRequestConverter : CustomCreationConverter<ISyncRequest>
    {
        /// <summary>
        /// Converts the Sync Request interface with and instance object of that type.
        /// </summary>
        /// <param name="objectType">Type of the object to convert</param>
        /// <returns><see cref="ISyncRequest"/></returns>
        public override ISyncRequest Create(Type objectType)
        {
            return new SyncRequest();
        }
    }
}
