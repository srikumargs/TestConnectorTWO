using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BackOfficePlugins.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.AddIn.Hosting;
using Sage.Connector.ProcessExecution;
using Sage.Connector.ProcessExecution.Contract;
using Sage.Connector.ProcessExecution.Events;
using System.ComponentModel.Composition.Hosting;
using Sage.Connector.Sales.Contracts;

namespace ProcessExecution.UnitTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class DomainMediatorUnitTests
    {


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sales_ProcessQuotes()
        {
            IApp appObj = new AppObject();

            var executeAddInProcess = new AddInProcess(Platform.AnyCpu);
            try
            {


                // Assume that the current directory is the application folder,  
                // and that it contains the pipeline folder structure.
                String addInRoot = Environment.CurrentDirectory + "\\Pipeline";

                // Update the cache files of the pipeline segments and add-ins. 
                string[] warnings = AddInStore.Update(addInRoot);
                foreach (string warning in warnings)
                {
                    Console.WriteLine(warning);
                }

                // Search for the specific add-in of the Process Execution.  
                // Although the method returns a collection, there should be only 1 Addin returned. 

                Collection<AddInToken> processTokens =
                    AddInStore.FindAddIn(typeof(IProcessRequest), addInRoot,
                    addInRoot + @"\AddIns\ProcessExecution\Sage.Connector.ProcessExecution.Addin.dll",
                    "Sage.Connector.ProcessExecution.Addin.ProcessExecution");

                //Using same appdomain and process space for this test. 

                var processToken = processTokens.First();

                // Activate the selected AddInToken in a new application domain  
                //// with the Internet trust level.
                var processRequest = processToken.Activate<IProcessRequest>(AddInSecurityLevel.FullTrust);

                string quoteRequest = "{\"ExternalId\":\"1000\",\"Description\":\"Description 1\",\"QuoteNumber\":1000,\"Tax\":90.0,\"SandH\":25.0,\"QuoteTotal\":1000.0,\"SubTotal\":1000.0,\"Status\":\"Active\",\"ExpiryDate\":null,\"SubmittedDate\":\"2013-12-06T00:43:00.6311289Z\",\"Customer\":\"Customer 1\",\"DiscountPercent\":10.0}";

                processRequest.Initialize(appObj);
                processRequest.ProcessResponse += new EventHandler<ResponseEventArgs>(Request_ProcessResponse);


                //TODO: we need to pass the entire domain mediation record over.  However, because we are out of process, we need to 
                //TODO: serialize the type into multiple parameters that can be passed. 
                var dispatchTask = Task.Factory.StartNew(() => processRequest.ProcessRequest(Guid.NewGuid(), "Mock Back Office Product", "admin", "1", "ProcessQuote", quoteRequest));
                Console.WriteLine("Task {0} executing", dispatchTask.Id);

                dispatchTask.Wait();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);

                throw;
            }
            finally
            {
                executeAddInProcess.Shutdown();
            }
        }

        void Request_ProcessResponse(object sender, ResponseEventArgs e)
        {
            Console.WriteLine("Host: Work progressing: {0}", e.Payload);

            //We're going to stop the add-in if it ever reports progress > 50%
            Debug.Print("Host: Request Reponse processing: {0}", e.Payload);
        }

        class AppObject : IApp
        {
            public event EventHandler<AppNotificationEventArgs> AppNotification;

            public void FireEvent()
            {
                if (AppNotification != null)
                {
                    AppNotification.Invoke(this, new DMNotificationEventArgs());
                }
            }
        }

        class DMNotificationEventArgs : AppNotificationEventArgs
        {
            public override string Data
            {
                get { return "Cancel"; }
            }
        }
    }


}
