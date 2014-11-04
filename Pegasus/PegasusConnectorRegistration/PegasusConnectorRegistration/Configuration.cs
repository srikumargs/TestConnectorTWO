using System;
using System.Runtime.Serialization;

namespace PegasusConnectorRegistration
{
    [DataContract]
    public class Configuration
    {
        [DataMember(Name = "configurationbaseuri")]
        public Uri ConfigurationBaseUri { get; set; }
        [DataMember(Name = "configurationresourcepath")]
        public String ConfigurationResourcePath { get; set; }
        [DataMember(Name = "requestbaseuri")]
        public Uri RequestBaseUri { get; set; }
        [DataMember(Name = "requestresourcepath")]
        public String RequestResourcePath { get; set; }
        [DataMember(Name = "responsebaseuri")]
        public Uri ResponseBaseUri { get; set; }
        [DataMember(Name = "responseresourcepath")]
        public String ResponseResourcePath { get; set; }
        [DataMember(Name = "requestuploadresourcepath")]
        public String RequestUploadResourcePath { get; set; }
        [DataMember(Name = "responseuploadresourcepath")]
        public String ResponseUploadResourcePath { get; set; }
        [DataMember(Name = "notificationresourceuri")]
        public Uri NotificationResourceUri { get; set; }
        [DataMember(Name = "minimumconnectorproductversion")]
        public String MinimumConnectorProductVersion { get; set; }
        [DataMember(Name = "upgradeconnectorproductversion")]
        public String UpgradeConnectorProductVersion { get; set; }
        [DataMember(Name = "upgradeconnectorpublicationdate")]
        public DateTime UpgradeConnectorPublicationDate { get; set; }
        [DataMember(Name = "upgradeconnectordescription")]
        public String UpgradeConnectorDescription { get; set; }
        [DataMember(Name = "upgradeconnectorlinkuri")]
        public Uri UpgradeConnectorLinkUri { get; set; }
        [DataMember(Name = "siteaddressbaseuri")]
        public Uri SiteAddressBaseUri { get; set; }
        [DataMember(Name = "tenantpublicuri")]
        public Uri TenantPublicUri { get; set; }
        [DataMember(Name = "tenantname")]
        public String TenantName { get; set; }
        [DataMember(Name = "maxblobsize")]
        public UInt32 MaxBlobSize { get; set; }
        [DataMember(Name = "largeresponsesizethreshold")]
        public UInt32 LargeResponseSizeThreshold { get; set; }
        [DataMember(Name = "suggestedmaxconnectoruptimeduration")]
        public TimeSpan SuggestedMaxConnectorUptimeDuration { get; set; }
        [DataMember(Name = "mincommunicationfailureretryinterval")]
        public TimeSpan MinCommunicationFailureRetryInterval { get; set; }
        [DataMember(Name = "maxcommunicationfailureretryinterval")]
        public TimeSpan MaxCommunicationFailureRetryInterval { get; set; }
    }
}
