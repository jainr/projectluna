import { string } from "yup";
import { IBaseModel } from "./IBaseModel";

export interface IProductModel extends IBaseModel {
  displayName: string;
  applicationName: string;
  owner: string;
  description: string;
  saaSOfferName: string;
  saaSOfferPlanName: string;
  createdTime?: string;
  lastUpdatedTime?: string;
  Idlist?: string;
  selectedProductId?: string
  selectedProductindex?: number  
}

export interface ILookupType {
  id:string;
  displayName:string;
}

export interface IDeploymentsModel extends IBaseModel {
  applicationName:string;
  apiName: string;
  apiDisplayName:string;
  apiType:string;
  selectedPlanType:string;
  description:string;
  versionName: string;  
  selecteddeploymentName: string;  
}

export interface IDeploymentVersionModel {  
  applicationName: string;
  apiName: string;
  versionName: string;
  amlWorkspaceName:string;
  gitRepoName: string;
  endpointName: string;
  modelName: string;
  modelVersion: string;
  modelDisplayName: string;
  gitVersion: string;
  dataShareAccountname: string;
  dataShareName: string;
  linkedServiceType: string;
  isUseDefaultRunConfig: boolean;
  isRunProjectOnManagedCompute: boolean;
  linkedServiceComputeTarget: string;
  runConfigFile: string;
  selectedVersionName:string;
  deployModelId: string;
  advancedSettings: string;
  productType: string;
}

export interface IAMLWorkSpaceModel extends IBaseModel{  
  workspaceName:string;
  resourceId:string;
  aadTenantId:string;
  registeredTime:string;
  aadApplicationId:string;
  aadApplicationSecrets:string;
  selectedWorkspaceName:string;
}

export interface IMLModelArtifactModel extends IBaseModel{
  name:string;
  version: string;
}

export interface IMLEndpointArtifactModel extends IBaseModel{
  name: string;
}

export interface IAMLComputeClusterModel extends IBaseModel{
  name: string;
}

export interface IGitRepoModel extends IBaseModel{
  type: string;
  repoName: string;
  httpUrl: string;
  personalAccessToken: string;
}

export interface IPermissionsModel extends IBaseModel{
  userId: string;
  role: string;
  createdDate: string;
}

export interface ISourceModel {  
  displayName:string;
  id:string;
}

export interface IPipeLineModel {  
  displayName:string;
  id:string;
  lastUpdatedTime:string;
  description:string;
}