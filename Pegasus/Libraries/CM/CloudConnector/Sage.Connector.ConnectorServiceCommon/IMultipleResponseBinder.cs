using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Sage.CRE.CloudConnector.ConnectorServiceCommon;

namespace Sage.CRE.CloudConnector.Binding
{
    public interface IMultipleResponseBinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestWrapper"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        [OperationContract]
        void InvokeWork(RequestWrapper requestWrapper, CancellationTokenSource cancellationTokenSource);
    }
}
