using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sage.Connector.DomainContracts.BackOffice;
using Sage.Connector.DomainContracts.Data;
using Sage.Connector.DomainContracts.Data.Metadata;
using Sage.Connector.DomainContracts.Responses;
using Sage.Connector.DomainMediator.Core;
using Sage.Connector.DomainMediator.Core.JsonConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Sage.Connector.Configuration.Mediator
{
    /// <summary>
    /// Validates the Back Office Feature Configuration
    /// </summary>
    [Export(typeof(IDomainFeatureRequest))]
    [FeatureMetadataExport(FeatureMessageTypes.ValidateFeatureConfigurationPropertyValue, typeof(FeatureDescriptions), "IManageFeatureConfiguration")]
    public class ValidateFeatureConfigurationPropertyValue : AbstractDomainMediator
    {

        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<IManageFeatureConfiguration, IBackOfficeData>> _backOfficeHandlers;
#pragma warning restore 649


        /// <summary>
        /// Validate the back office feature configuration
        /// </summary>
        /// <param name="handler">The <see cref="IResponseHandler"/></param>
        /// <param name="requestId">The Connector Request Id</param>
        /// <param name="tenantId">The tenant Id making the request.</param>
        /// <param name="backOfficeConfiguration">The back office company configuration <see cref="IBackOfficeCompanyConfiguration"/></param>
        /// <param name="requestPayload">String representing the payload to process.</param>
        public override void FeatureRequest(IResponseHandler handler, Guid requestId, String tenantId,
            IBackOfficeCompanyConfiguration backOfficeConfiguration, string requestPayload)
        {

            var validatePropertyValueResponse = new Response();

            var processors = (from backOfficeHandler in _backOfficeHandlers
                              where backOfficeHandler.Metadata.BackOfficeId.Equals(backOfficeConfiguration.BackOfficeId)
                              select backOfficeHandler.Value).DefaultIfEmpty();

            var cfg = new DomainMediatorJsonSerializerSettings();
            cfg.Converters.Add(new KeyValuePairConverter());

            KeyValuePair<PropertyDefinition, Object> featurePropertyValuePair = (String.IsNullOrWhiteSpace(requestPayload))
                ? new KeyValuePair<PropertyDefinition, object>() 
                : JsonConvert.DeserializeObject<KeyValuePair<PropertyDefinition, Object>>(requestPayload, cfg);
            PropertyDefinition propertyDefinition = featurePropertyValuePair.Key;


            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (processors != null )
            {

                foreach (IManageFeatureConfiguration processor in processors)
                {
                    String featureName = processor.GetType().GetInterfaces()[0].Name;

                    if (!featurePropertyValuePair.ContainsKey(featureName))
                    {
                        //TODO KMS: ask if the feature has any, and if so, we are missing them in the call. 
                        continue;
                    }

                    var propertyValuePairs = featurePropertyValuePairs[featureName];

                    //TODO KMS: Figure out how to tell the backoffice to keep the session alive for more incoming calls
                    //TODO KMS: on this session. Problem is we don't know how the back offices will implement. 
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    var backOfficeSessionHandler = processor as IBackOfficeSessionHandler;
                    if (backOfficeSessionHandler != null)
                    {
                        var data = BackOfficeCompanyData.FromConfiguration(backOfficeConfiguration);
                        backOfficeSessionHandler.BeginSession(data);
                    }

                    ValidationResponse validation = processor.ValidateFeatureConfigurationValues(propertyValuePairs);
                    featureValidationResponses.Add(featureName, validation);


                    if (backOfficeSessionHandler != null)
                    {
                        backOfficeSessionHandler.EndSession();
                    }
                }
            }

            //If there aren't any then the response is empty
            String responsePayload = JsonConvert.SerializeObject(validatePropertyValueResponse, cfg);
            handler.HandleResponse(requestId, responsePayload);
        }
    }
}
