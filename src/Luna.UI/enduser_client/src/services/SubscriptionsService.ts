import {ServiceBase} from "../services/ServiceBase";
import {
    ICreateSubscriptionModel,
    IAccessTokenModel,
    IDeviceTokenModel,
    IOperationHistoryModel,
    ISubscriptionsModel,
    ISubscriptionWarningsModel, IUpdateSubscriptionModel,
    Result,
    ISubscriptionsV2RefreshKeyModel,
    ISubscriptionsV2Model
} from "../models";

import {IOfferParameterModel} from "../models/IOfferParameterModel";
import {v4 as uuid} from "uuid";

export default class SubscriptionsService extends ServiceBase {

    public static async getDeviceCode(): Promise<Result<IDeviceTokenModel>>{
        
        var result = await this.requestJson<IDeviceTokenModel>({
            url: `/manage/devicecode`,
            method: "GET"
        });

        return result;
    }


    public static async getAccessToken(code: string): Promise<Result<IAccessTokenModel>>{

        var result = await this.requestJson<IAccessTokenModel>({
            url: `/manage/accessToken?device_code=${code}`,
            method: "GET"
        });

        return result;
    }

    public static async listParameters(offerName: string, planName: string): Promise<Result<IOfferParameterModel[]>> {
    
        var result = await this.requestJson<IOfferParameterModel[]>({
          url: `/marketplace/offers/${offerName}/plans/${planName}/parameters`,
          method: "GET"
      });
    
        if (!result.hasErrors && result.value)
          result.value.map(u => u.clientId = uuid());
    
        return result;
      }

    public static async list(oid: string): Promise<Result<ISubscriptionsModel[]>> {

        var result = await this.requestJson<ISubscriptionsModel[]>({
            url: `/marketplace/subscriptiondetails`,
            method: "GET"
        });

        return result;
    }

    public static async get(subscriptionId: string): Promise<Result<ISubscriptionsModel>> {

        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/marketplace/subscriptions/${subscriptionId}`,
            method: "GET"
        });

        return result;
    }

    public static async getSubscriptionWarnings(): Promise<Result<ISubscriptionWarningsModel>> {

        let warnings = [
            "Offer blah is out of sync with the Azure Marketplace. Click<a href='https://www.bing.com' target='_blank'>here</a> for more details.",
            "Oh no...something happened with your deployment!  Click<a href='https://www.bing.com' target='_blank'>here</a> for more details."
        ];
        return new Result<ISubscriptionWarningsModel>({ warnings: warnings }, true);
        /*var result = await this.requestJson<IOfferWarningsModel>({
          url: `/offerwarnings`,
          method: "GET"
        });
        return result;*/
    }

    public static async create(model: ICreateSubscriptionModel): Promise<Result<ISubscriptionsModel>> {
        
        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/marketplace/subscriptions/${model.id}`,
            method: "PUT",
            data: model
        });

        return result;
    }

    public static async update(model: IUpdateSubscriptionModel): Promise<Result<ISubscriptionsModel>> {
        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/subscriptions/${model.SubscriptionId}`,
            method: "PUT",
            data: model
        });

        return result;
    }
/*
    public static async create_update(model: ISubscriptionsPostModel): Promise<Result<ISubscriptionsModel>> {        
        var result = await this.requestJson<ISubscriptionsModel>({
            url: `/subscriptions/${model.subscriptionId}`,
            method: "PUT",
            data: model
        });

        return result;
    }*/

    public static async delete(subscriptionId: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/subscriptions/${subscriptionId}`,
            method: "DELETE"
        });
        return result;
    }

    public static async getOperationHistory(subscriptionId: string): Promise<Result<IOperationHistoryModel[]>> {

        var result = await this.requestJson<IOperationHistoryModel[]>({
            url: `/subscriptions/${subscriptionId}/operations`,
            // url: `http://localhost:3002/Subscriptions/${email}`,
            method: "GET"
        });
                
        // return result;        
        return result;
    }

    //#region SubscriptionV2
    
    public static async listV2(): Promise<Result<ISubscriptionsV2Model[]>> {

        var result = await this.requestJson<ISubscriptionsV2Model[]>({
            url: `/apisubscriptions`,
            method: "GET"
        });

        return result;
    }
    
    public static async getV2(subscriptionId): Promise<Result<ISubscriptionsV2Model>> {

        var result = await this.requestJson<ISubscriptionsV2Model>({
            url: `/apisubscriptions/${subscriptionId}`,
            method: "GET"
        });
        return result;
    }

    public static async createV2(model: ISubscriptionsV2Model): Promise<Result<ISubscriptionsV2Model>> {
        var result = await this.requestJson<ISubscriptionsV2Model>({
            url: `/apisubscriptions/create`,
            method: "POST",
            data: model
        });
        
        return result;
    }
    public static async updateV2(model: ISubscriptionsV2Model): Promise<Result<ISubscriptionsV2Model>> {
        var result = await this.requestJson<ISubscriptionsV2Model>({
            url: `/apisubscriptions/${model.subscriptionId}`,
            method: "PUT",
            data: model
        });
        
        return result;
    }

    public static async RefreshKey(model: ISubscriptionsV2RefreshKeyModel): Promise<Result<ISubscriptionsV2Model>> {
        var result = await this.requestJson<ISubscriptionsV2Model>({
            url: `/apisubscriptions/${model.subscriptionId}/regenerateKey`,
            method: "POST",
            data: model
        });
        
        return result;
    }

    public static async deleteV2(subscriptionId: string): Promise<Result<{}>> {
        var result = await this.requestJson<Result<{}>>({
            url: `/subscriptions/${subscriptionId}`,
            method: "DELETE"
        });
        return result;
    }

    //#endregion
}