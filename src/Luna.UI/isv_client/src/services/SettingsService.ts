import { ServiceBase } from "./ServiceBase";
import {
  IAMLWorkSpaceModel,  
  Result,
  ISourceModel,  
  IGitRepoModel,  
  IPartnerServiceModel,
  IAutomationWebhookModel,
  IPermissionsModel
} from "../models";
import { v4 as uuid } from "uuid";

export default class SettingService extends ServiceBase {

  //#region git repo

  public static async getGitRepoList(): Promise<Result<IGitRepoModel[]>> {

    var result = await this.requestJson<IGitRepoModel[]>({
      url: `/gitrepos/`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  //
  //#region AMLWorkSpace

  public static async getPartnerServicesList(): Promise<Result<IPartnerServiceModel[]>> {

    var result = await this.requestJson<IPartnerServiceModel[]>({
      url: `/amlworkspaces/`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getAutomationWebhooksList(): Promise<Result<IAutomationWebhookModel[]>> {

    var result = await this.requestJson<IAutomationWebhookModel[]>({
      url: `/amlworkspaces/`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }  

  public static async getPartnerServiceByName(partnerServiceName: string): Promise<Result<IPartnerServiceModel>> {

    var result = await this.requestJson<IPartnerServiceModel>({
      url: `/amlworkspaces/${partnerServiceName}`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async getAutomationWebhookByName(partnerServiceName: string): Promise<Result<IAutomationWebhookModel>> {

    var result = await this.requestJson<IAutomationWebhookModel>({
      url: `/amlworkspaces/${partnerServiceName}`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async getbatchInferenceList(): Promise<Result<IAMLWorkSpaceModel[]>> {

    var result = await this.requestJson<IAMLWorkSpaceModel[]>({
      url: `/amlworkspaces/`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async gettrainModelList(): Promise<Result<IAMLWorkSpaceModel[]>> {

    var result = await this.requestJson<IAMLWorkSpaceModel[]>({
      url: `/amlworkspaces/`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getSourceModelList(): Promise<Result<ISourceModel[]>> {

    var result = await this.requestJson<ISourceModel[]>({
      url: `/apiVersions/sourceTypes/`,
      method: "GET"
    });

    return result;
  }

  public static async getPermissions(): Promise<Result<IPermissionsModel[]>> {
    var result = await this.requestJson<IPermissionsModel[]>({
      url: `/permissions/`,
      method: "GET"
    });

    return result;
  }

  public static async createPermissions(model: IPermissionsModel): Promise<Result<IPermissionsModel>> {
    var result = await this.requestJson<IPermissionsModel>({
      url: `/permissions/`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }  

  public static async deletePermission(userId: string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/permissions/${userId}`,
      method: "DELETE"
    });
    return result;
  }
  //#endregion
}