import { IBaseModel } from "./IBaseModel";

export interface IApplicationModel {
    displayName: string;
    applicationName: string;
    description: string;
    logoImageUrl: string;
    documentationUrl: string;
    publisher: string;
    tags:ITags[];
}
export interface ITags
{
    key: string;
    value: string;
}