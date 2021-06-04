import uuid from "uuid";
import * as yup from "yup";
import { ObjectSchema } from "yup";
import { ISubscriptionsPostModel, ISubscriptionsModel, ISubscriptionsWarnings } from "../../../models";
import { IWizardModel } from "../../../models/IWizardModel";

export interface IWizardFormValues {
  wizard: IWizardModel;
}

const wizardFormValidator: ObjectSchema<IWizardModel> = yup.object().shape(
  {
  sourceServiceType: yup.string().required("Source Service Type required"),
  sourceService: yup.string().required("Source Service required"),
  mLComponentType: yup.string().required("ML Component Type required"),
  mLComponent: yup.string().required("ML Component required"),
  operationName: yup.string().required("Operation Name required"),
  applicationDisplayName: yup.string().required("Application Display Name required"),
  applicationName: yup.string().required("Application Name required"),
  apiName: yup.string().required("API Name required"),
  apiVersion: yup.string().required("API Version required"),
  applicationDescription: yup.string(),
  logoImageURL: yup.string(),
  documentationURL: yup.string(),
  publisher: yup.string(),
  branchOrCommitHash: yup.string(),
  executionConfigFile: yup.string(),
  computeServiceType: yup.string(),
  computeService: yup.string(),
  clientId: yup.string()
  }
);

export const wizardFormValidationSchema: ObjectSchema<IWizardFormValues> =
  yup.object().shape({
    wizard: wizardFormValidator
  });

  export const initialWizardValues: IWizardModel = {
    sourceServiceType: '',
    sourceService: '',
    mLComponentType: '',
    mLComponent: '',
    operationName: '',
    applicationDisplayName: '',
    applicationName: '',
    apiName: '',
    apiVersion: '',
    applicationDescription: '',
    logoImageURL: '',
    documentationURL: '',
    publisher: '',
    branchOrCommitHash: '',
    executionConfigFile: '',
    computeServiceType: '',
    computeService: '',
    clientId: uuid()
  };

  export const initialWizardFormValues: IWizardFormValues = {
    wizard: initialWizardValues
  }

  

  