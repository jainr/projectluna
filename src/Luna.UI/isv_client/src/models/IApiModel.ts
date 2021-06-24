import { string } from "yup";
import { IBaseModel } from "./IBaseModel";

export interface IApiModel {   
    description: string;   
    displayName: string;
    type: string;
    advancedSettings: string;
}