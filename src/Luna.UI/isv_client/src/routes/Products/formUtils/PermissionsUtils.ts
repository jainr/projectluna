import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IPermissionsModel } from "../../../models";
import { v4 as uuid } from "uuid";

export const initialPermissionsValues: IPermissionsModel = {  
  userId:'',
  role: '',
  clientId: uuid(),
  createdDate: new Date().toLocaleString(),
};
export interface IPermissionsFormValues {
    permissions: IPermissionsModel;
}
export const initialPermissionsFormValues: IPermissionsFormValues = {
    permissions: initialPermissionsValues
}
const PermissionsValidator: ObjectSchema<IPermissionsModel> = yup.object().shape(
  {
    userId: yup.string().required("UserId is required"),
    role:  yup.string().required("Role is required"),
    clientId: yup.string(),
    createdDate: yup.string(),
  }
);
export const permissionsFormValidationSchema: ObjectSchema<IPermissionsFormValues> =
  yup.object().shape({
    permissions: PermissionsValidator
  });