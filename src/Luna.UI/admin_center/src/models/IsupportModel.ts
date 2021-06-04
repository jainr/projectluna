import {IBaseModel} from "./IBaseModel";

export interface ISupportCasesModel extends IBaseModel {
  title: string;
  createdBy: string;  
  createdTime: string;
  lastUpdatedTime: string;
  icmTicket: string;  
}

export interface ISupportPermissionModel extends IBaseModel {
  customerName: string;
  activeSupportCase: string;  
  permissions: string;
  history: string;  
}