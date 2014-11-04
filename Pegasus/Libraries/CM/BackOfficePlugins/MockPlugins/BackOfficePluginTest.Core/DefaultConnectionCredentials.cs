using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace BackOfficePluginTest.Core
{
    public static  class DefaultConnectionCredentials
    {
        /// <summary>
        /// 
        /// </summary>
        static DefaultConnectionCredentials()
        {
            dynamic connection = new ExpandoObject();
            connection.UserId = "User";
            connection.Password = "Password";
            connection.CompanyId = "Company1";

            _credentialsAsString = JsonConvert.SerializeObject(connection);

            dynamic credentials = JsonConvert.DeserializeObject(_credentialsAsString);
            _credentials = credentials.ToObject<IDictionary<string, string>>();
        }

        /// <summary>
        /// 
        /// </summary>
        public static  string AsString()
        {
           return _credentialsAsString; 
        }

        /// <summary>
        /// 
        /// </summary>
        public static IDictionary<string, string> Credentials { get { return _credentials; } }

        static private readonly IDictionary<string, string> _credentials;
        static private readonly string _credentialsAsString;
    }
}
