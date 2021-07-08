import {IBaseModel} from "./IBaseModel";

export interface ISettingsModel extends IBaseModel {
  user: string;
  role: string;  
  createdDate: string;  
}