import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IDeploymentsModel, IDeploymentVersionModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { objectIdNameRegExp, versionNameRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );

export const getInitialDeployment = (): IDeploymentsModel => {
  return {
    aiServiceName: '',
    selecteddeploymentName:'',
    versionName: '',
    aiServicePlanName: '',
    aiServicePlanDisplayName: '',
    selectedPlanType: '',
    planType:'',
    description: '',
    isSaved: false,
    isModified: false,
    clientId: uuid()
  }
};

export const getInitialVersion = (): IDeploymentVersionModel => {
  return {
    productType: '',
    aiServiceName: '',
    aiServicePlanName: '',
    versionName: '',
    amlWorkspaceName: '',
    gitRepoName: '',
    endpointName: '',
    modelName: '',
    modelVersion: 'latest',
    modelDisplayName: '',
    gitVersion: '',
    linkedServiceType: '',
    isUseDefaultRunConfig: true,
    isRunProjectOnManagedCompute: true,
    linkedServiceComputeTarget: '',
    runConfigFile: '',
    selectedVersionName: '',
    deployModelId: '',
    advancedSettings: '',
  }
};

export const initialDeploymentList: IDeploymentsModel[] = [{
  aiServiceName: 'a1',
  selecteddeploymentName:'',
  aiServicePlanName: 'd1',
  aiServicePlanDisplayName: '',
  selectedPlanType: '',
  planType:'',
  versionName: '1.0',
  description: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  aiServiceName: 'b1',
  selecteddeploymentName:'',
  aiServicePlanName: 'd2',
  aiServicePlanDisplayName: '',
  selectedPlanType: '',
  planType:'',
  versionName: '2.0',
  description: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];

export interface IDeploymentFormValues {
  deployment: IDeploymentsModel;
}

export interface IDeploymentVersionFormValues {
  version: IDeploymentVersionModel;
}

export const initialDeploymentFormValues: IDeploymentFormValues = {
  deployment: getInitialDeployment()
}

const deploymentValidator: ObjectSchema<IDeploymentsModel> = yup.object().shape(
  {
    clientId: yup.string(),
    aiServiceName: yup.string(),
    aiServicePlanDisplayName: yup.string(),
    planType: yup.string(),
    selectedPlanType: yup.string(),
    selecteddeploymentName:yup.string(),
    versionName: yup.string(),
    aiServicePlanName: yup.string()
      .required("Id is a required field")
      .matches(objectIdNameRegExp,
        {
          message: ErrorMessage.deploymentName,
          excludeEmptyString: true
        }),
    description: yup.string(),
  }
);

export const deletedeploymentValidator: ObjectSchema<IDeploymentsModel> = yup.object().shape(
  {

    clientId: yup.string(),
    aiServiceName: yup.string(),
    aiServicePlanName: yup.string(),
    aiServicePlanDisplayName: yup.string(),
    planType: yup.string(),
    selectedPlanType: yup.string(),
    selecteddeploymentName:yup.string()
      .test('selecteddeploymentName', 'Deployment name does not match', function (value: string) {        
        const name: string = this.parent.deployment.deploymentName;
        if (!value)
          return true;

        return value.toLowerCase() === name.toLowerCase();
      }).required("Deployment Name is a required field"),
    versionName: yup.string(),
    description: yup.string(),

  }
);

const versionFormValidator: ObjectSchema<IDeploymentVersionModel> = yup.object().shape(
  {
    productType: yup.string(),
    aiServiceName: yup.string(),
    aiServicePlanName: yup.string(),
    versionName: yup.string(),
    amlWorkspaceName: yup.string(),
    gitRepoName: yup.string(),
    modelName: yup.string(),
    modelVersion: yup.string(),
    modelDisplayName: yup.string(),
    endpointName: yup.string(),
    gitVersion: yup.string(),
    linkedServiceType: yup.string(),
    isUseDefaultRunConfig: yup.boolean(),
    isRunProjectOnManagedCompute: yup.boolean(),
    linkedServiceComputeTarget: yup.string(),
    runConfigFile: yup.string(),
    selectedVersionName: yup.string(),
    deployModelId: yup.string(),
    advancedSettings: yup.string(),
  }
);

export const deleteVersionValidator: ObjectSchema<IDeploymentVersionModel> = yup.object().shape(
  {
    productType: yup.string(),
    aiServiceName: yup.string(),
    aiServicePlanName: yup.string(),
    versionName: yup.string(),
    amlWorkspaceName: yup.string(),
    gitRepoName: yup.string(),
    endpointName: yup.string(),
    modelName: yup.string(),
    modelVersion: yup.string(),
    modelDisplayName: yup.string(),
    gitVersion: yup.string(),
    linkedServiceType: yup.string(),
    isUseDefaultRunConfig: yup.boolean(),
    isRunProjectOnManagedCompute: yup.boolean(),
    linkedServiceComputeTarget: yup.string(),
    runConfigFile: yup.string(),
    deployModelId: yup.string(),
    advancedSettings: yup.string(),
    selectedVersionName:yup.string()
    .test('selectedVersionName', 'Version name does not match', function (value: string) {         
      const name: string = this.parent.versionName;
      if (!value)
        return true;

      return value.toLowerCase() === name.toLowerCase();
    }).required("versionName is a required field"),
    configFile:yup.string()
  }
);

export const deploymentFormValidationSchema: ObjectSchema<IDeploymentFormValues> =
  yup.object().shape({
    deployment: deploymentValidator
  });

export const versionFormValidationSchema: ObjectSchema<IDeploymentVersionFormValues> =
  yup.object().shape({
    version: versionFormValidator
  });

  // export const deleteVersionValidationSchema: ObjectSchema<IDeploymentVersionFormValues> =
  // yup.object().shape({
  //   version: deleteVersionValidator
  // });