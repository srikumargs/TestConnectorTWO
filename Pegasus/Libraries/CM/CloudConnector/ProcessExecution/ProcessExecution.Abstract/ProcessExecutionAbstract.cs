using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sage.Connector.ProcessExecution.AddinView;

namespace ProcessExecution.Abstract
{
    public abstract class ProcessExecutionAbstract : IProcessRequest
    {
        public virtual void ProcessRequest(string request)
        {
            
        }
    }
}
