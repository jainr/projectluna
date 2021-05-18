﻿using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Luna.Partner.Clients.PartnerServiceClients
{
    /// <summary>
    /// The client class for Azure ML
    /// </summary>
    public class AzureSynapseClient : IPartnerServiceClient
    {
        private AzureSynapseWorkspaceConfiguration _config;

        public AzureSynapseClient(BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureSynapseWorkspaceConfiguration)configuration;

        }

        /// <summary>
        /// Validate an registered partner service
        /// </summary>
        public bool TestConnection()
        {
            //TODO: implement test connection
            return true;
        }

        /// <summary>
        /// Update the configuration of the service client
        /// </summary>
        /// <param name="configuration">The configuration</param>
        public void UpdateConfiguration(BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureSynapseWorkspaceConfiguration)configuration;
        }
    }
}