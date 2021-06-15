using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Public.Client
{
    public enum PartnerServiceType
    {
        AzureML,
        AzureSynapse,
        GitHub
    }

    public class PartnerServiceTypeMetadata
    {
        public static ServiceType[] MLHostServiceTypes = new ServiceType[] 
        {
            new ServiceType(PartnerServiceType.AzureML.ToString(), "Azure Machine Learning workspace"),
            new ServiceType(PartnerServiceType.GitHub.ToString(), "GitHub repo")
        };

        public static ServiceType[] MLComputeServiceTypes = new ServiceType[]
        {
            new ServiceType(PartnerServiceType.AzureML.ToString(), "Azure Machine Learning workspace"),
            new ServiceType(PartnerServiceType.AzureSynapse.ToString(), "Azure Synapse workspace")
        };
    }

    public class PartnerServiceComponentTypeMetadata
    {
        public static ComponentType[] GetComponentTypes(string partnerServiceType)
        {
            object typeObj;

            if (Enum.TryParse(typeof(PartnerServiceType), partnerServiceType, out typeObj))
            {
                PartnerServiceType type = (PartnerServiceType)typeObj;
                switch(type)
                {
                    case PartnerServiceType.AzureML:
                        return new ComponentType[]
                        {
                            new ComponentType(LunaAPIType.Realtime.ToString(), "Realtime endpoints"),
                            new ComponentType(LunaAPIType.Pipeline.ToString(), "Pipeline endpoints")
                        };
                    default:
                        break;
                }
            }

            return new ComponentType[] { };
        }
        
    }
}
