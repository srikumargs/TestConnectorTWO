using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sage.Connector.DomainMediator.Core.JsonConverters;
using System.AddIn.Hosting;

namespace Connector.DomainMediator.Tests
{
    /// <summary>
    /// Summary description for SyncUnitTests
    /// </summary>
    [TestClass]
    public class SyncUnitTests : AbstractDomainTest
    {
        #region Mock

        /* -----------------------------------------------------------------------------------------------
         * Mock 
         * -----------------------------------------------------------------------------------------------
         */
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sync_TaxCodes_Mock()
        {
            //{"ResourceKindName":"TaxCodes","CloudTick":1}
            var syncRequest = new SyncRequest
            {
                CloudTick = 0,
                ResourceKindName = "TaxCodes"
            };
            var payload = JsonConvert.SerializeObject(syncRequest, new DomainMediatorJsonSerializerSettings());

            var executeAddInProcess = new AddInProcess(Platform.AnyCpu);
            ExecuteProcessRequest(executeAddInProcess, Request_ProcessResponse, "Mock", "SyncTaxCodes", payload);
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sync_TaxSchedules_Mock()
        {
            var syncRequest = new SyncRequest
            {
                CloudTick = 0,
                ResourceKindName = "TaxSchedules"
            };

            var payload = JsonConvert.SerializeObject(syncRequest, new DomainMediatorJsonSerializerSettings());
            var executeAddInProcess = new AddInProcess(Platform.AnyCpu);
            ExecuteProcessRequest(executeAddInProcess, Request_ProcessResponse, "Mock", "SyncTaxSchedules", payload);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sync_Customers_Mock()
        {
            var syncRequest = new SyncRequest
            {
                CloudTick = 0,
                ResourceKindName = "Customers"
            };

            var payload = JsonConvert.SerializeObject(syncRequest, new DomainMediatorJsonSerializerSettings());
            var executeAddInProcess = new AddInProcess(Platform.AnyCpu);
            ExecuteProcessRequest(executeAddInProcess, Request_ProcessResponse, "Mock", "SyncCustomers", payload);

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Sync_InventoryItems_Mock()
        {
            var syncRequest = new SyncRequest
            {
                CloudTick = 0,
                ResourceKindName = "InventoryItems"
            };

            var payload = JsonConvert.SerializeObject(syncRequest, new DomainMediatorJsonSerializerSettings());
            var executeAddInProcess = new AddInProcess(Platform.AnyCpu);
            ExecuteProcessRequest(executeAddInProcess, Request_ProcessResponse, "Mock", "SyncInventoryItems", payload);

        }
        #endregion


        #region Sage 300 ERP
        /* -----------------------------------------------------------------------------------------------
         * Sage 300 ERP tests
         * -----------------------------------------------------------------------------------------------
         */

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [Ignore]
        public void Sync_Customers_Sage300ERP()
        {
            var syncRequest = new SyncRequest
            {
                CloudTick = 1,
                ResourceKindName = "Customers"
            };
            var payload = JsonConvert.SerializeObject(syncRequest, new DomainMediatorJsonSerializerSettings());

            var executeAddInProcess = new AddInProcess(Platform.X86);
            ExecuteProcessRequest(executeAddInProcess, Request_ProcessResponse, "Sage300ERP", "SyncCustomers", payload);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [Ignore]
        public void Sync_Customers_Sage300ERP_SameProcess()
        {
            var syncRequest = new SyncRequest
            {
                CloudTick = 1,
                ResourceKindName = "SyncCustomers"
            };
            var payload = JsonConvert.SerializeObject(syncRequest, new DomainMediatorJsonSerializerSettings());
            ExecuteProcessRequest(null, Request_ProcessResponse, "Sage300ERP", "SyncCustomers", payload);
        }

        #endregion

    }
}
