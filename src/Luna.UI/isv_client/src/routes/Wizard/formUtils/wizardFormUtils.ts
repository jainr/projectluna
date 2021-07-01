import uuid from "uuid";
import * as yup from "yup";
import { ObjectSchema } from "yup";
import { ISubscriptionsPostModel, ISubscriptionsModel, ISubscriptionsWarnings } from "../../../models";
import { IWizardModel } from "../../../models/IWizardModel";
import { ErrorMessage } from "../../Wizard/formUtils/ErrorMessage";
import { httpURLRegExp, opetarionNameRegExp, versionNameRegExp } from "../../Products/formUtils/RegExp";

export interface IWizardFormValues {
  wizard: IWizardModel;
}

const wizardFormValidator: ObjectSchema<IWizardModel> = yup.object().shape(
  {    
  sourceServiceType: yup.string().required("Source Service Type required"),
  sourceService: yup.string().required("Source Service required"),  
  mLComponentType: yup.string(),
  mLComponent: yup.string(),
  operationName: yup.string(),
  applicationDisplayName: yup.string().max(128,"Application Display Name is too long. Must be no more than 128 characters").required("Application Display Name is required")
  ,
  applicationName: yup.string().matches(opetarionNameRegExp,
    {
      message: ErrorMessage.applicationName,
      excludeEmptyString: true
    }).required("Application Name is required"),
  apiName: yup.string().matches(opetarionNameRegExp,
    {
      message: ErrorMessage.apiName,
      excludeEmptyString: true
    }).required("API Name is required"),
  apiVersion: yup.string().matches(versionNameRegExp,
    {
      message: ErrorMessage.apiVersion,
      excludeEmptyString: true
    }).required("Version is required"),
  applicationDescription: yup.string(),
  logoImageURL: yup.string().matches(httpURLRegExp,
    {
      message: ErrorMessage.Url,
      excludeEmptyString: true
    }),
  documentationURL: yup.string().matches(httpURLRegExp,
    {
      message: ErrorMessage.Url,
      excludeEmptyString: true
    }),  
  publisher: yup.string(),
  branchOrCommitHash: yup.string().min(1,"Branch or Commit Hash is required"),
  executionConfigFile: yup.string().min(1,"Config file is required"),
  computeServiceType: yup.string().min(1,"Compute Service Type is required"),
  computeService: yup.string().min(1,"Compute Service is required"),
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
    branchOrCommitHash: 'main',
    executionConfigFile: 'config.json',
    computeServiceType: '',
    computeService: '',
    clientId: uuid()
  };

  export const initialWizardFormValues: IWizardFormValues = {
    wizard: initialWizardValues
  }

  

  