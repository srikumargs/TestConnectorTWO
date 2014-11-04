using System;
using System.Runtime.Serialization;

namespace PegasusConnectorRegistration
{
    [DataContract(Name = "Message")]
    public class Message
    {
        /// <summary>
        /// Unique identifier for this message
        /// </summary>
        [DataMember(Name = "id")]
        public Guid Id { get; set; }
        /// <summary>
        /// Message type identifier
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }
        /// <summary>
        /// Date and time of request
        /// </summary>
        [DataMember(Name = "timestamp")]
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// Version of the message
        /// </summary>
        [DataMember(Name = "version")]
        public int Version { get; set; }
        /// <summary>
        /// A string descriptor of the type of the enclosed body
        /// </summary>
        [DataMember(Name = "bodytype")]
        public String BodyType { get; set; }
        /// <summary>
        /// Serialized value of the cloud/back office entities
        /// </summary>
        [DataMember(Name = "body")]
        public String Body { get; set; }
        /// <summary>
        /// Computed hash of the body content
        /// </summary>
        [DataMember(Name = "bodyhash")]
        public String BodyHash { get; set; }
        /// <summary>
        /// Encapsulates information indicating the location of blob storage used for large data uploads
        /// </summary>
        [DataMember(Name = "uploadsessioninfo")]
        public UploadSessionInfo UploadSessionInfo { get; set; }
        /// <summary>
        /// Identifier of the original message / request
        /// </summary>
        /// <remarks>
        /// Used to track the original request
        /// </remarks>
        [DataMember(Name = "correlationid")]
        public Guid CorrelationId { get; set; }
    }

    [DataContract(Name = "UploadSessionInfoContract")]
    public class UploadSessionInfo
    {
        /// <summary>
        /// The key used to identify an upload session
        /// </summary>
        [DataMember(Name = "SessionKey", IsRequired = true, Order = 0)]
        public string SessionKey { get; protected set; }

        /// <summary>
        /// The name of the destination for this upload
        /// This is how both the client and service identify the upload
        /// </summary>
        [DataMember(Name = "DestinationName", IsRequired = true, Order = 1)]
        public string DestinationName { get; protected set; }

        /// <summary>
        /// The Uri of the container for the tenant
        /// Which is where this new blob will be going
        /// </summary>
        [DataMember(Name = "ContainerUri", IsRequired = true, Order = 2)]
        public Uri ContainerUri { get; protected set; }

        /// <summary>
        /// The directory path, if any, for the resulting blob
        /// </summary>
        [DataMember(Name = "DirectoryPath", IsRequired = true, Order = 3)]
        public string DirectoryPath { get; protected set; }

        /// <summary>
        /// The size in bytes that uploads should be 'chunked'
        /// </summary>
        [DataMember(Name = "ChunkSizeInBytes", IsRequired = true, Order = 4)]
        public Int32 ChunkSizeInBytes { get; protected set; }
    }
}
