using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sage.Connector.DomainContracts.Data.Metadata;

namespace BackOfficePluginTest.Core
{
    static public class TestHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backOfficeHandlers"></param>
        /// <param name="backOfficeId"></param>
        /// <returns></returns>
        static public T FindFeatureProcessor<T>(IEnumerable<Lazy<T, IBackOfficeData>> backOfficeHandlers, string backOfficeId)
        {
            // run the the back office handers that provide interface T.
            //find the one that has a matching back office id
            var featureProcessor = (from backOfficeHandler in backOfficeHandlers
                                               where backOfficeHandler.Metadata.BackOfficeId.Equals(backOfficeId, StringComparison.CurrentCultureIgnoreCase)
                                               select backOfficeHandler.Value).FirstOrDefault();

            return featureProcessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="featureProcessor"></param>
        /// <param name="backOfficeId"></param>
        public static void ValidateFeatureProcessor<T>(T featureProcessor, string backOfficeId)
            where T : class
        {
             string typename = typeof (T).ToString();
            Assert.IsNotNull(featureProcessor,
                String.Format("{0} back office to process the {1} was not found!", backOfficeId, typename));
        }
    }
}

