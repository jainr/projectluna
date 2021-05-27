import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IPartnerServiceModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { objectIdNameRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";
import { guidRegExp } from "../../Offers/formUtils/RegExp";

export const initialPartnerServiceValues: IPartnerServiceModel = {  
  type:'',
  resourceId:'',
  tenantId:'',
  partnerServiceName: '',
  clinetSecrets:'',
  isSaved: false,
  isModified: false,
  clientId: uuid(),
  createdDate: new Date().toLocaleString(),
};

export interface IPartnerServiceFormValues {
  partnerService: IPartnerServiceModel;
}

export const initialPartnerServiceFormValues: IPartnerServiceFormValues = {
  partnerService: initialPartnerServiceValues
}

const PartnerServiceValidator: ObjectSchema<IPartnerServiceModel> = yup.object().shape(
  {
    clientId: yup.string(),
      tenantId: yup.string().matches(guidRegExp,
      {
        message: ErrorMessage.tenantId,
        excludeEmptyString: true
      })
      .required('Tenant Id is required'),
      clinetSecrets: yup.string().required('Clinet Secrets is required'),
    resourceId: yup.string().required('Resource id is required'),
    registeredTime: yup.string(),
    type:  yup.string(),
    partnerServiceName: yup.string()
      .matches(objectIdNameRegExp,
        {
          message: ErrorMessage.workSpaceName,
          excludeEmptyString: true
        }).required("Partner Service Name is required"),
      createdDate: yup.string(),
    // selectedWorkspaceName: yup.string()
  }
);

export const partnerServiceFormValidationSchema: ObjectSchema<IPartnerServiceFormValues> =
  yup.object().shape({
    partnerService: PartnerServiceValidator
  });
