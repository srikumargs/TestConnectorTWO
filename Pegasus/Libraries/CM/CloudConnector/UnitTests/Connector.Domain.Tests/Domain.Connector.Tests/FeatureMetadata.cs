
using Sage.Connector.DomainMediator.Core;

namespace Connector.DomainMediator.Tests
{
    public class FeatureMetadata: IFeatureMetaData
    {
        public FeatureMetadata(string displayName, string interfaceName, string name)
        {
            Name = name;
            InterfaceName = interfaceName;
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }
        
        public string InterfaceName  { get; private set; }
       

        public string Name { get; private set; }
      
    }
}
