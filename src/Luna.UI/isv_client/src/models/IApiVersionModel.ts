import { IBaseModel } from "./IBaseModel";

export interface IApiVersionModel {
    AzureMLWorkspaceName: string;
    description: string;
    type: string;
    endpoints: IEndpoint[];
    advancedSettings: string;
}
export interface IEndpoint
{
    endpointName:string;
    operationName: string;
}