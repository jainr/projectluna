import { string } from "yup";
import { IBaseModel } from "./IBaseModel";

export interface IWizardModel extends IBaseModel {
    sourceServiceType: string;
    sourceService: string;
    mLComponentType: string;
    mLComponent: string;
    operationName: string;
    applicationDisplayName: string;
    applicationName: string;
    apiName: string;
    apiVersion: string;
    applicationDescription: string;
    logoImageURL: string;
    documentationURL: string;
    publisher: string;
    branchOrCommitHash: string;
    executionConfigFile: string;
    computeServiceType: string;
    computeService: string;
}