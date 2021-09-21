export interface IParamModel {
  name: string,
  type: string,
  value: any,
  isSystemParameter: boolean,
}

export interface IDeviceTokenModel{
  user_code: string,
  device_code: string,
  verification_url: string,
  expires_in: number,
  interval: number,
  message: string
}

export interface IAccessTokenModel{
  token_type: string,
  scope: string,
  expires_in: string,
  ext_expires_in: string,
  expires_on: string,
  access_token: string
}

export interface ISubscriptionsModel {

  id: string,
  name: string,
  offerId: string,
  planId: string,
  owner: string,
  ownerId: string,
  quantity: number,
  beneficiaryTenantId: string,
  purchaserTenantId: string,
  subscribeWebhookName: string,
  unsubscribeWebhookName: string,
  suspendWebhookName: string,
  deleteDataWebhookName: string,
  priceModel: string,
  monthlyBase: number,
  privatePlan: boolean,
  inputParameters: IParamModel[],
  provisioningStatus: string,
  entryPointUrl: string,
  primaryKey: string,
  baseUrl: string,
  secondaryKey: string,

  publisherId: string,
  saaSSubscriptionStatus: string,
  isTest: boolean,
  allowedCustomerOperationsMask: number,
  sessionMode: string,
  sandboxType: string,
  isFreeTrial: boolean,
  createdTime: string,
  activatedTime: string,
  lastUpdatedTime: string,
  lastSuspendedTime: string,
  unsubscribedTime: string,
  dataDeletedTime: string,
  operationId: string,
  deploymentName: string,
  deploymentId: string,
  resourceGroup: string,
  activatedBy: string,
  parameters: IParamModel[]
}

export interface ICreateSubscriptionModel {
  id: string
  name: string
  offerId: string
  planId: string
  publisherId: string
  token: string
  ownerId: string
  inputParameters: IParamModel[];
}

export interface ISubscriptionFormModel {
  subscription: IUpdateSubscriptionModel
}

export interface IUpdateSubscriptionModel {
  SubscriptionId: string,
  SubscriptionName: string,
  SubscriptionVerifiedName: string,
  OfferName: string,
  CurrentPlanName: string,
  PlanName: string,
  isUpdatePlan: boolean
}

export interface IOperationHistoryModel {
  timeStamp: string,
  status: number,
  action: string
  activityId: string,
  id: string,
  offerId: string,
  operationRequestSource: string,
  planId: string,
  publisherId: string,
  quantity?: number,
  resourceLocation: string,
  subscriptionId: string,  
  requestId: string,
  statusCode: number,
  success: boolean  
}

export interface ISubscriptionsV2Model {
  subscriptionId: string,
  name: string,
  userId: string,
  productName: string,
  deploymentName: string,
  status: string,
  baseUrl: string,
  primaryKey: string,
  secondaryKey: string,
  parameters: IParamModel[]
}

export interface ISubscriptionsV2RefreshKeyModel {  
  subscriptionId: string,
  keyName:string;  
}