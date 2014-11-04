using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;

namespace PegasusConnectorRegistration
{
    public partial class PegasusConnectorRegistrationForm : Form
    {
        public PegasusConnectorRegistrationForm()
        {
            InitializeComponent();
        }

        private void RegisterBtn_Click(object sender, EventArgs e)
        {
            ResultsTextBox.Text = "Registering tenant...";
            var results = InjectTenantConfiguration(APIUriTextBox.Text, NotificationUriTextBox.Text, TenantIDTextBox.Text);
            ResultsTextBox.Text = results;
        }

        static string InjectTenantConfiguration(string apiAddress, string notificationAddress, string tenantId)
        {
            Configuration config = new Configuration()
            {
                ConfigurationBaseUri = new Uri(apiAddress),
                ConfigurationResourcePath = @"api/configuration",
                RequestBaseUri = new Uri(apiAddress),
                RequestResourcePath = @"api/messages/requests",
                ResponseBaseUri = new Uri(apiAddress),
                ResponseResourcePath = @"api/messages/responses",
                RequestUploadResourcePath = @"api/messages/requests/startuploadrequest",
                ResponseUploadResourcePath = @"api/messages/responses/enduploadrequest",
                NotificationResourceUri = new Uri(notificationAddress),
                MinimumConnectorProductVersion = "1.0.0.0",
                UpgradeConnectorProductVersion = "3.0.0.0",
                UpgradeConnectorPublicationDate = DateTime.UtcNow.Date,
                UpgradeConnectorDescription = "Improved performance.",
                UpgradeConnectorLinkUri = new Uri(@"http://www.sage.com/connector_download"),
                SiteAddressBaseUri = new Uri(apiAddress),
                TenantPublicUri =  new Uri(apiAddress),
                TenantName = "DemoTenant",
                MaxBlobSize = 16384000,
                LargeResponseSizeThreshold = 10000000,
                SuggestedMaxConnectorUptimeDuration = new TimeSpan(7, 0, 0, 0),
                MinCommunicationFailureRetryInterval = TimeSpan.FromSeconds(1),
                MaxCommunicationFailureRetryInterval = TimeSpan.FromMinutes(1),
            };

            var serialiedConfig = JsonSerialize(config);

            var message = new Message()
            {
                Id = Guid.NewGuid(),
                Type = 1,
                TimeStamp = DateTime.UtcNow,
                Version = 1,
                BodyType = config.GetType().FullName,
                Body = serialiedConfig,
                BodyHash = "",
                UploadSessionInfo = null,
                CorrelationId = Guid.Empty
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("TenantId", tenantId);

                try
                {
                    var result = client.PostAsJsonAsync(@"api/configuration", message).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        return ConnectorKey(apiAddress, "Test", tenantId);
                    }
                    return result.ReasonPhrase;
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        static string ConnectorKey(string siteAddress, string premisekey, string tenantId)
        {
            var encodedSiteAddress = ToBase64(siteAddress);
            return string.Concat(tenantId, ":", premisekey, ":", encodedSiteAddress);
        }

        static string JsonSerialize(object item)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(item);
        }

        static string ToBase64(string value)
        {
            try
            {
                Byte[] baseUriBytes = Encoding.UTF8.GetBytes(value);
                string base64String = System.Convert.ToBase64String(baseUriBytes, Base64FormattingOptions.None);
                return base64String;
            }
            catch
            {
                // Supplied nothing
                return null;
            }
        }

    }
}
