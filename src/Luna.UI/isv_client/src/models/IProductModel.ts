import { string } from "yup";
import { IBaseModel } from "./IBaseModel";

export interface IProductModel extends IBaseModel {
  displayName: string;
  aiServiceName: string;
  owner: string;
  logoImageUrl: string;
  description: string;
  documentationUrl: string;
  tags: string;
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
  aiServiceName:string;
  aiServicePlanName: string;
  aiServicePlanDisplayName:string;
  planType:string;
  selectedPlanType:string;
  description:string;
  versionName: string;  
  selecteddeploymentName: string;  
}

export interface IDeploymentVersionModel {  
  aiServiceName: string;
  aiServicePlanName: string;
  versionName: string;
  amlWorkspaceName:string;
  gitRepoName: string;
  endpointName: string;
  gitVersion: string;
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
