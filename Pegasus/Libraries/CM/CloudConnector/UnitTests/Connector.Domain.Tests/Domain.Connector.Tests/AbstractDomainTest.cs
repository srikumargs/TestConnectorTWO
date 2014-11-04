using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.ProcessExecution.Events;
using Sage.Connector.ProcessExecution.Interfaces;
using Sage.Connector.ProcessExecution.Interfaces.Events;
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Connector.DomainMediator.Tests
{
    public class AbstractDomainTest
    {
        protected static readonly string MockTestTenantId = "9B906D46-34E5-431B-A723-42A034379FEA".ToLower();
        protected string ProcessExecutionPath = Environment.CurrentDirectory + @"\Pipeline\AddIns\ProcessExecution\";

        protected void ExecuteProcessRequest(AddInProcess executeAddInProcess,
            EventHandler<ResponseEventArgs> responseProcessFunc, String backOfficeProductId, String feature,
            string payload)
        {
            IApp appObj = new AppObject();


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
                    AddInStore.FindAddIn(typeof (IProcessRequest), addInRoot,
                        addInRoot + @"\AddIns\ProcessExecution\Sage.Connector.ProcessExecution.Addin.dll",
                        "Sage.Connector.ProcessExecution.Addin.ProcessExecution");

                //Using same appdomain and process space for this test. 

                var processToken = processTokens.First();

                // Activate the selected AddInToken in a new application domain  
                //// with the Internet trust level.
                var processRequest = (executeAddInProcess == null)
                    ? processToken.Activate<IProcessRequest>(AddInSecurityLevel.FullTrust)
                    : processToken.Activate<IProcessRequest>(executeAddInProcess, AddInSecurityLevel.FullTrust);


                processRequest.Initialize(appObj);
                processRequest.ProcessResponse += responseProcessFunc;


                //TODO: we need to pass the entire domain mediation record over.  However, because we are out of process, we need to 
                //TODO: serialize the type into multiple parameters that can be passed. 
                dynamic connection = new ExpandoObject();
                connection.UserId = "admin";
                connection.Password = "1";
                connection.CompanyId = "AdminOnlyCompany";

                string credentialsAsString = JsonConvert.SerializeObject(connection);

                dynamic credentials = JsonConvert.DeserializeObject(credentialsAsString);
                credentials = credentials.ToObject<IDictionary<string, string>>();


                var dispatchTask =
                    Task.Factory.StartNew(() => processRequest.ProcessRequest(Guid.NewGuid(), MockTestTenantId,
                        new BackOfficeCompanyConfigurationObject()
                        {
                            BackOfficeId = backOfficeProductId,
                            ConnectionCredentials = credentialsAsString
                        },
                        feature, payload));
                Console.WriteLine("Task {0} executing", dispatchTask.Id);

                dispatchTask.Wait();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Assert.Fail(ex.Message);
            }
            finally
            {
                if (executeAddInProcess != null)
                {
                    executeAddInProcess.Shutdown();
                }
            }
        }

        protected void Request_ProcessResponse(object sender, ResponseEventArgs e)
        {
            Console.WriteLine("Host: Work progressing: {0}", e.Payload);

            //We're going to stop the add-in if it ever reports progress > 50%
            Debug.Print("Host: Request Response processing: {0}", e.Payload);
        }


        private class AppObject : IApp
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

        private class DMNotificationEventArgs : AppNotificationEventArgs
        {
            public override string Data
            {
                get { return "Cancel"; }
            }
        }

        private class BackOfficeCompanyConfigurationObject : IBackOfficeCompanyConfiguration
        {
            public string BackOfficeId { get; set; }
            public string ConnectionCredentials { get; set; }
        }
    }
}
