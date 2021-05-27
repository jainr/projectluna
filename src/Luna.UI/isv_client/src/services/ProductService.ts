import {ServiceBase} from "./ServiceBase";
import {
  IAMLWorkSpaceModel,
  IDeploymentsModel,
  IDeploymentVersionModel,
  ILookupType,
  IProductModel,
  Result,
  ISourceModel,
  IPipeLineModel,
  IGitRepoModel,
  IMLModelArtifactModel,
  IMLEndpointArtifactModel,
  IAMLComputeClusterModel,
  IPermissionsModel
} from "../models";
import {v4 as uuid} from "uuid";

export default class ProductService extends ServiceBase {

  public static async list(): Promise<Result<IProductModel[]>> {

    var result = await this.requestJson<IProductModel[]>({
      url: `/aiservices`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async get(productName: string): Promise<Result<IProductModel>> {

    var result = await this.requestJson<IProductModel>({
      url: `/applications/${productName}`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async update(model: IProductModel): Promise<Result<IProductModel>> {
    var result = await this.requestJson<IProductModel>({
      url: `/applications/${model.applicationName}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async delete(productName: string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/applications/${productName}`,
      method: "DELETE"
    });
    return result;
  }

  public static async create(model: IProductModel): Promise<Result<IProductModel>> {
    var result = await this.requestJson<IProductModel>({
      url: `/applications/${model.applicationName}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }


  //#region Deployments 

  public static async getDeploymentListByProductName(productName: string): Promise<Result<IDeploymentsModel[]>> {

    var result = await this.requestJson<IDeploymentsModel[]>({
      url: `/applications/${productName}/apis`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getDeploymentByProductName(productName: string,deploymentName:string): Promise<Result<IDeploymentsModel>> {

    var result = await this.requestJson<IDeploymentsModel>({
      url: `/applications/${productName}/apis/${deploymentName}`,
      method: "GET"
    });

    return result;
  }

  public static async createOrUpdateDeployment(model: IDeploymentsModel): Promise<Result<IDeploymentsModel>> {
    var result = await this.requestJson<IDeploymentsModel>({
      url: `/applications/${model.applicationName}/apis/${model.apiName}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async deleteDeployment(productName: string,deploymentName:string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/applications/${productName}/apis/${deploymentName}`,
      method: "DELETE"
    });
    return result;
  }
  //#endregion

  //#region Version
  public static async getDeploymentVersionListByDeploymentName(productName: string,deploymentName:string): Promise<Result<IDeploymentVersionModel[]>> {

    var result = await this.requestJson<IDeploymentVersionModel[]>({
      url: `/applications/${productName}/apis/${deploymentName}/apiversions`,
      method: "GET"
    });

    return result;
  }

  public static async getDeploymentVersionById(productName: string,deploymentName:string,versionName:string): Promise<Result<IDeploymentVersionModel>> {

    var result = await this.requestJson<IDeploymentVersionModel>({
      url: `/applications/${productName}/apis/${deploymentName}/apiversions/${versionName}`,
      method: "GET"
    });

    return result;
  }

  public static async createOrUpdateDeploymentVersion(model: IDeploymentVersionModel): Promise<Result<IDeploymentVersionModel>> {
    var result = await this.requestJson<IDeploymentVersionModel>({
      url: `/applications/${model.applicationName}/apis/${model.apiName}/apiversions/${model.versionName}`,
      method: "PUT",
      data: model
    });

    return result;
  }

  public static async deleteDeploymentVersion(productName: string,deploymentName:string,versionName:string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/applications/${productName}/apis/${deploymentName}/apiversions/${versionName}`,
      method: "DELETE"
    });
    return result;
  }
  //#endregion

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

  public static async getAmlWorkSpaceList(): Promise<Result<IAMLWorkSpaceModel[]>> {

    var result = await this.requestJson<IAMLWorkSpaceModel[]>({
      url: `/amlworkspaces/`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getModelsFromAmlWorkspace(workspaceName:string): Promise<Result<IMLModelArtifactModel[]>> {

    var result = await this.requestJson<IMLModelArtifactModel[]>({
      url: `/amlworkspaces/${workspaceName}/models`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getEndpointsFromAmlWorkspace(workspaceName:string): Promise<Result<IMLEndpointArtifactModel[]>> {

    var result = await this.requestJson<IMLEndpointArtifactModel[]>({
      url: `/amlworkspaces/${workspaceName}/endpoints`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getComputeClustersFromAmlWorkspace(workspaceName:string): Promise<Result<IAMLComputeClusterModel[]>> {

    var result = await this.requestJson<IAMLComputeClusterModel[]>({
      url: `/amlworkspaces/${workspaceName}/computeclusters`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async getAmlWorkSpaceByName(workspaceName:string): Promise<Result<IAMLWorkSpaceModel>> {

    var result = await this.requestJson<IAMLWorkSpaceModel>({
      url: `/amlworkspaces/${workspaceName}`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async createOrUpdateWorkSpace(model: IAMLWorkSpaceModel): Promise<Result<IAMLWorkSpaceModel>> {
    var result = await this.requestJson<IAMLWorkSpaceModel>({
      url: `/amlworkspaces/${model.workspaceName}`,
      method: "PUT",
      data: model
    });

    if (!result.hasErrors && result.value)
      result.value.clientId = uuid();

    return result;
  }

  public static async deleteWorkSpace(workspaceName: string): Promise<Result<any>> {
    var result = await this.requestJson<Result<any>>({
      url: `/amlworkspaces/${workspaceName}/`,
      method: "DELETE"
    });
    return result;
  }

  public static async getPublishedPipeLineByAmlWorkSpaceList(amlWorkspace:string): Promise<Result<IPipeLineModel[]>> {

    var result = await this.requestJson<IPipeLineModel[]>({
      url: `/amlworkspaces/${amlWorkspace}/pipelines/`,
      method: "GET"
    });
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
  //#endregion
}