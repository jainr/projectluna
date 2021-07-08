import { ServiceBase } from "./ServiceBase";
import { IPlanModel, Result, IARMTemplateModel,IRestrictedUsersModel } from "../models";
import { v4 as uuid } from "uuid";
import { IWizardModel } from "../models/IWizardModel";
import {IApplicationModel} from "../models/IApplicationModel";
import {IApiModel} from "../models/IApiModel";
import {IApiVersionModel, IEndpoint} from "../models/IApiVersionModel";
import { endpoint } from "../adalConfig";

// eslint-disable-next-line @typescript-eslint/no-unused-vars
let armTemplateModellist: IARMTemplateModel[];
export default class WizardService extends ServiceBase {

    public static async serviceTypesList(): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerServices/metadata/hostservicetypes`,
            method: "GET"            
        });

        return result;
    }

    public static async serviceList(option): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerServices?type=`+option,
            method: "GET"            
        });

        return result;
    }

    public static async componentTypeList(): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerservices/metadata/AzureML/mlcomponenttypes`,
            method: "GET"            
        });

        return result;
    }

    public static async componentList(option): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerservices/amlworkspace/mlcomponents/`+option,
            method: "GET"            
        });

        return result;
    }

    public static async computedServiceTypeList(): Promise<Result<any>> {
      var result = await this.requestJson<any>({
          url: `/manage/partnerServices/metadata/computeservicetypes`,
          method: "GET"            
      });

      return result;
  }

  public static async computeServiceList(type): Promise<Result<any>> {
    var result = await this.requestJson<any>({
        url: `/manage/partnerServices?type=${type}`,
        method: "GET"            
    });

    return result;
  }

  public static async createPartnerService(model: IWizardModel): Promise<Result<any>> {
    let applicationModel: IApplicationModel = {
        displayName: model.applicationDisplayName,
        applicationName: model.applicationName,
        description: model.applicationDescription,
        logoImageUrl: model.logoImageURL,
        documentationUrl: model.documentationURL,
        publisher: model.publisher,
        tags:[],
    };
    
    var result = await this.requestJson<any>({
      //url: `/manage/partnerServices/metadata/computeservicetypes`,
      url: `/manage/applications/${applicationModel.applicationName}`,
      method: "PUT",
      data: applicationModel
    });       

    return result;
  }

    public static async createApplication(model: IWizardModel): Promise<Result<any>> {
        let applicationModel: IApplicationModel = {
            displayName: model.applicationDisplayName,
            applicationName: model.applicationName,
            description: model.applicationDescription,
            logoImageUrl: model.logoImageURL,
            documentationUrl: model.documentationURL,
            publisher: model.publisher,
            tags:[],
        };
        
        var result = await this.requestJson<any>({
          url: `/manage/applications/${applicationModel.applicationName}`,
          method: "PUT",
          data: applicationModel
        });       
    
        return result;
      }

      public static async createAPI(model: IWizardModel): Promise<Result<any>> {
        let apiModel: IApiModel = {
            displayName: model.apiName,           
            description: model.applicationDescription,
            type: model.mLComponentType === ''? 'MLProject' : model.mLComponentType,
            advancedSettings: ""            
        };
        
        var result = await this.requestJson<IApplicationModel>({
          url: `/manage/applications/${model.applicationName}/apis/${model.apiName}`,
          method: "PUT",
          data: apiModel
        });       
    
        return result;
      }

      public static async createAPIVersion(model: IWizardModel): Promise<Result<any>> {
        let endpointModel : IEndpoint={
            endpointName: "",
            operationName: model.operationName
        }
        let endpointList: IEndpoint[] = [];
        endpointList.push(endpointModel);

        let apiVersionModel: IApiVersionModel = {
            AzureMLWorkspaceName: model.sourceService,           
            description: model.applicationDescription,
            type: model.sourceServiceType,
            advancedSettings: "",
            endpoints: endpointList
        };
                
        var result = await this.requestJson<IApplicationModel>({
          url: `/manage/applications/${model.applicationName}/apis/${model.apiName}/versions/${model.apiVersion}`,
          method: "PUT",
          data: apiVersionModel
        });       
    
        return result;
      }

      public static async publishApplication(appName: string): Promise<Result<any>> {        
        
        var result = await this.requestJson<any>({
          url: `/manage/applications/${appName}/publish`,
          method: "POST",
        });       
    
        return result;
      }
}

