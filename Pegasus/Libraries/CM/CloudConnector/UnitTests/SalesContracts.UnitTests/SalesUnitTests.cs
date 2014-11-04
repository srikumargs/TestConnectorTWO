using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sage.Connector.Sales.Contracts;

namespace SalesContracts.UnitTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class SalesUnitTests
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestJsonSerialization()
        {
            IQuote quote = new Quote()
            {
                Customer = "Customer 1",
                Description = "Description 1",
                DiscountPercent =  10,
                ExpiryDate = null,
                ExternalId = "1000",
                QuoteNumber = 1000,
                QuoteTotal = 1000,
                SandH = 25,
                Status="Active",
                SubTotal = 1000,
                SubmittedDate = DateTime.UtcNow,
                Tax = 90

            };

            string output = JsonConvert.SerializeObject(quote);
            Assert.IsNotNull(output, "serialization did not worked");

        }

        [TestMethod]
        public void TestObjectList()
        {
            IList < IQuote > quotes = new List<IQuote>();

            IList<object> objects = quotes.ToList<object>();
            Assert.AreNotSame(quotes, objects);



        }
    }
}
