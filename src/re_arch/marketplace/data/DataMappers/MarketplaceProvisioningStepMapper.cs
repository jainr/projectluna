using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Marketplace.Data
{

    public class MarketplaceProvisioningStepMapper :
        IDataMapper<BaseProvisioningStepRequest, BaseProvisioningStepResponse, BaseProvisioningStepProp>
    {

        public BaseProvisioningStepProp Map(BaseProvisioningStepRequest request)
        {
            BaseProvisioningStepProp prop = null;

            if (request is ARMTemplateProvisioningStepRequest)
            {
                prop = new ARMTemplateProvisioningStepProp
                {
                    IsSynchronized = request.IsSynchronized,
                    Description = request.Description,
                    TemplateUrl = ((ARMTemplateProvisioningStepRequest)request).TemplateUrl,
                    IsRunInCompleteMode = ((ARMTemplateProvisioningStepRequest)request).IsRunInCompleteMode,
                    AzureSubscriptionIdParameterName = ((ARMTemplateProvisioningStepRequest)request).AzureSubscriptionIdParameterName,
                    AzureLocationParameterName = ((ARMTemplateProvisioningStepRequest)request).AzureLocationParameterName,
                    ResourceGroupNameParameterName = ((ARMTemplateProvisioningStepRequest)request).ResourceGroupNameParameterName,
                    AccessTokenParameterName = ((ARMTemplateProvisioningStepRequest)request).AccessTokenParameterName,
                    InputParameterNames = ((ARMTemplateProvisioningStepRequest)request).InputParameterNames
                };
            }
            else if (request is WebhookProvisioningStepRequest)
            {
                prop = new WebhookProvisioningStepProp
                {
                    IsSynchronized = request.IsSynchronized,
                    Description = request.Description,
                    WebhookUrl = ((WebhookProvisioningStepRequest)request).WebhookUrl,
                    WebhookAuthKey = ((WebhookProvisioningStepRequest)request).WebhookAuthKey,
                    WebhookAuthType = ((WebhookProvisioningStepRequest)request).WebhookAuthType,
                    WebhookAuthValue = ((WebhookProvisioningStepRequest)request).WebhookAuthValue,
                    TimeoutInSeconds = ((WebhookProvisioningStepRequest)request).TimeoutInSeconds,
                    InputParameterNames = ((WebhookProvisioningStepRequest)request).InputParameterNames,
                    OutputParameterNames = ((WebhookProvisioningStepRequest)request).OutputParameterNames,
                };
            }
            else if (request is ScriptProvisioningStepRequest)
            {
                prop = new ScriptProvisioningStepProp
                {
                    IsSynchronized = request.IsSynchronized,
                    Description = request.Description,
                    ScriptPackageUrl = ((ScriptProvisioningStepRequest)request).ScriptPackageUrl,
                    EntryScriptFileName = ((ScriptProvisioningStepRequest)request).EntryScriptFileName,
                    TimeoutInSeconds = ((ScriptProvisioningStepRequest)request).TimeoutInSeconds,
                    InputArguments = new List<ScriptArgument>(),
                };

                foreach (var arg in ((ScriptProvisioningStepRequest)request).InputArguments)
                {
                    ((ScriptProvisioningStepProp)prop).InputArguments.Add(new ScriptArgument
                    {
                        Name = arg.Name,
                        Option = arg.Option
                    });
                }
            }
            else
            {
                throw new LunaServerException($"Unknown provisioning step type {request.GetType().FullName}");
            }

            return prop;
        }

        public BaseProvisioningStepResponse Map(BaseProvisioningStepProp prop)
        {
            BaseProvisioningStepResponse response = null;

            if (prop is ARMTemplateProvisioningStepProp)
            {
                response = new ARMTemplateProvisioningStepResponse
                {
                    IsSynchronized = prop.IsSynchronized,
                    Description = prop.Description,
                    TemplateUrl = ((ARMTemplateProvisioningStepProp)prop).TemplateUrl,
                    IsRunInCompleteMode = ((ARMTemplateProvisioningStepProp)prop).IsRunInCompleteMode,
                    AzureSubscriptionIdParameterName = ((ARMTemplateProvisioningStepProp)prop).AzureSubscriptionIdParameterName,
                    AzureLocationParameterName = ((ARMTemplateProvisioningStepProp)prop).AzureLocationParameterName,
                    ResourceGroupNameParameterName = ((ARMTemplateProvisioningStepProp)prop).ResourceGroupNameParameterName,
                    AccessTokenParameterName = ((ARMTemplateProvisioningStepProp)prop).AccessTokenParameterName,
                    InputParameterNames = ((ARMTemplateProvisioningStepProp)prop).InputParameterNames
                };
            }
            else if (prop is WebhookProvisioningStepProp)
            {
                response = new WebhookProvisioningStepResponse
                {
                    IsSynchronized = prop.IsSynchronized,
                    Description = prop.Description,
                    WebhookUrl = ((WebhookProvisioningStepProp)prop).WebhookUrl,
                    WebhookAuthKey = ((WebhookProvisioningStepProp)prop).WebhookAuthKey,
                    WebhookAuthType = ((WebhookProvisioningStepProp)prop).WebhookAuthType,
                    WebhookAuthValue = ((WebhookProvisioningStepProp)prop).WebhookAuthValue,
                    TimeoutInSeconds = ((WebhookProvisioningStepProp)prop).TimeoutInSeconds,
                    InputParameterNames = ((WebhookProvisioningStepProp)prop).InputParameterNames,
                    OutputParameterNames = ((WebhookProvisioningStepProp)prop).OutputParameterNames,
                };
            }
            else if (prop is ScriptProvisioningStepProp)
            {
                response = new ScriptProvisioningStepResponse
                {
                    IsSynchronized = prop.IsSynchronized,
                    Description = prop.Description,
                    ScriptPackageUrl = ((ScriptProvisioningStepProp)prop).ScriptPackageUrl,
                    EntryScriptFileName = ((ScriptProvisioningStepProp)prop).EntryScriptFileName,
                    TimeoutInSeconds = ((ScriptProvisioningStepProp)prop).TimeoutInSeconds,
                    InputArguments = new List<ScriptArgumentResponse>(),
                };

                foreach (var arg in ((ScriptProvisioningStepProp)prop).InputArguments)
                {
                    ((ScriptProvisioningStepResponse)response).InputArguments.Add(new ScriptArgumentResponse
                    {
                        Name = arg.Name,
                        Option = arg.Option
                    });
                }
            }
            else
            {
                throw new LunaServerException($"Unknown provisioning step type {prop.GetType().FullName}");
            }

            return response;
        }
    }
}
