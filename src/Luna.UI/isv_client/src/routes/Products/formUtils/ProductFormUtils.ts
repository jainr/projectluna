import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IProductModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { objectIdNameRegExp } from "./RegExp";
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
export const initialProductValues: IProductModel = {
  owner: userName,
  aiServiceName: '',
  logoImageUrl: '',
  documentationUrl: '',
  description: '',
  displayName: '',
  tags: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
};

export const initialProductList: IProductModel[] = [{
  tags: '',
  owner: 'v-anirc@microsoft.com',
  aiServiceName: '1',
  logoImageUrl: 'logo',
  description: 'description',
  documentationUrl: 'documenation',
  createdTime: '',
  displayName: '',
  lastUpdatedTime: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  tags: 'bringyourowncompute',
  owner: 'zbates@affirma.com',
  aiServiceName: '2',
  logoImageUrl: 'logo',
  description: 'description',
  documentationUrl: 'documenation',
  createdTime: '',
  displayName: '',
  lastUpdatedTime: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
},
{
  tags: '',
  owner: 'zbates@affirma.com',
  aiServiceName: '3',
  logoImageUrl: 'logo',
  description: 'description',
  documentationUrl: 'documenation',
  displayName: '',
  createdTime: '',
  lastUpdatedTime: '',
  isDeleted: false,
  isSaved: false,
  isModified: false,
  clientId: uuid()
}];

export interface IProductInfoFormValues {
  product: IProductModel;
}

export const initialInfoFormValues: IProductInfoFormValues = {
  product: initialProductValues
}

const productValidator: ObjectSchema<IProductModel> = yup.object().shape(
  {
    clientId: yup.string(),
    tags: yup.string(),
    aiServiceName: yup.string()
      .matches(objectIdNameRegExp,
        {
          message: ErrorMessage.productName,
          excludeEmptyString: true
        }).required("Id is a required field"),

    owner: yup.string().required("Owners is a required field"),
    logoImageUrl: yup.string().url(ErrorMessage.Url),
    description: yup.string().max(120, "The description is too long. Must be no more than 120 characters"),
    createdTime: yup.string(),
    documentationUrl: yup.string().url(ErrorMessage.Url),
    displayName: yup.string().max(64, "The display name is too long. Must be no more than 64 characters"),
    lastUpdatedTime: yup.string()
  }
);

export const productInfoValidationSchema: ObjectSchema<IProductInfoFormValues> =
  yup.object().shape({
    product: productValidator
  });

export const deleteProductValidator: ObjectSchema<IProductModel> = yup.object().shape(
  {
    clientId: yup.string(),
    aiServiceName: yup.string(),
    selectedProductId: yup.string()
      .test('selectedProductid', 'Product name does not match', function (value: string) {
        const productName: string = this.parent.aiServiceName;
        if (!value)
          return true;

        return value.toLowerCase() === productName.toLowerCase();
      }).matches(objectIdNameRegExp,
        {
          message: ErrorMessage.productName,
          excludeEmptyString: true
        }).required("Product id is a required field"),

    owner: yup.string(),
    tags: yup.string(),
    logoImageUrl: yup.string(),
    description: yup.string(),
    saasOfferName: yup.string(),
    displayName: yup.string(),
    documentationUrl: yup.string(),
    createdTime: yup.string(),
    lastUpdatedTime: yup.string()
  }
);