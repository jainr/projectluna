import * as yup from "yup";
import { ObjectSchema } from "yup";
import { ISettingsModel } from "../../../models";
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

export const initialUserList: ISettingsModel[] = [
  {
    user: 'Xiaochen Wu',
    role: 'Admin',
    createdDate: '04/22/2021',
    isNew: false,
    isDeleted: false,
    isSaved: false,
    isModified: false,
    clientId: uuid()
  },
  {
    user: 'Lindsey Allen',
    role: 'Admin',
    createdDate: '04/22/2021',
    isNew: false,
    isDeleted: false,
    isSaved: false,
    isModified: false,
    clientId: uuid()
  },
  {
    user: 'Firstname Lastname',
    role: 'Supporter',
    createdDate: '04/22/2021',
    isNew: false,
    isDeleted: false,
    isSaved: false,
    isModified: false,
    clientId: uuid()
  }
];