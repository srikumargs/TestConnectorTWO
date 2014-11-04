using System.Threading;
using Sage.Connector.DomainContracts.BackOffice;

namespace BackOfficePluginTest.Core
{
    public class SessionContext : ISessionContext
    {
        public SessionContext(ILogging logger, CancellationToken cancellationToken)
        {
            Logger = logger;
            CancellationToken = cancellationToken;
        }

        public ILogging Logger { get; private set; }


        public CancellationToken CancellationToken { get; private set; }
    }
}
