import * as yup from "yup";
import { ObjectSchema } from "yup";
import { IAutomationWebhookModel } from "../../../models";
import { v4 as uuid } from "uuid";
import { objectIdNameRegExp } from "./RegExp";
import { ErrorMessage } from "./ErrorMessage";
import { guidRegExp } from "../../Offers/formUtils/RegExp";

export const initialAutomationWebhookValues: IAutomationWebhookModel = {  
  name:'',
  webhookURL: '',
  enabled:false,
  isSaved: false,
  isModified: false,
  clientId: uuid(),
  createdDate: new Date().toLocaleString(),
};

export interface IAutomationWebhookFormValues {
  automationWebhook: IAutomationWebhookModel;
}

export const initialAutomationWebhookFormValues: IAutomationWebhookFormValues = {
  automationWebhook: initialAutomationWebhookValues
}

const AutomationWebhookValidator: ObjectSchema<IAutomationWebhookModel> = yup.object().shape(
  {
    clientId: yup.string(),      
    webhookURL: yup.string().required('Webhook URL is required'),
    name: yup.string()
      .matches(objectIdNameRegExp,
        {
          message: ErrorMessage.workSpaceName,
          excludeEmptyString: true
        }).required("Name is required"),
    createdDate: yup.string(),    
    enabled: yup.boolean()
  }
);

export const automationWebhookFormValidationSchema: ObjectSchema<IAutomationWebhookFormValues> =
  yup.object().shape({
    automationWebhook: AutomationWebhookValidator
  });
