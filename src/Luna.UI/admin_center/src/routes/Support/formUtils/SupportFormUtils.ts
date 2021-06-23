import * as yup from "yup";
import { ObjectSchema } from "yup";
import {ISupportCasesModel,ISupportPermissionModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { ErrorMessage } from "./ErrorMessage";
import adalContext from "../../../adalConfig";

let userName = ''
var response = adalContext.AuthContext.getCachedUser();
if (response && response.profile && response.profile.upn)
  userName = response.profile.upn;

export const shallowCompare = (obj1, obj2) =>
  Object.keys(obj1).length === Object.keys(obj2).length &&
  Object.keys(obj1).every(key =>
    obj2.hasOwnProperty(key) && obj1[key] === obj2[key]
  );
/*
export const ProductType: IDropdownOption[] = [
  { key: '', text: "Select" },
  { key: 'RTP', text: "Real-Time Prediction" },
  { key: 'BI', text: "Batch Inference" },
  { key: 'TYOM', text: "Train Your Own Model" }]

export const HostType: IDropdownOption[] = [
  { key: '', text: "Select" },
  { key: 'SAAS', text: "SaaS" },
  { key: 'BYOC', text: "Bring Your Own Compute" }]
*/

export const initialSupportCaseList: ISupportCasesModel[] = [{
  title: 'Unable to delete an application',
  createdBy: 'allenwux',
  createdTime: '04/22/2021',
  lastUpdatedTime: '04/22/2021',
  icmTicket: '23128497',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];

export const initialPermissionList: ISupportPermissionModel[] = [{
  customerName: 'ToolsGroup',
  activeSupportCase: '2',
  permissions: 'Request Access',
  history: 'view History',  
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];