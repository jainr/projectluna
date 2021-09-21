import {ServiceBase} from "../services/ServiceBase";
import {IResolveTokenModel, Result} from "../models";

export default class EndUserLandingService extends ServiceBase {

    public static async resolveToken(token: string): Promise<Result<IResolveTokenModel>> {

        var result = await this.sendRequest<IResolveTokenModel>(window.Configs.MARKETPLACE_API_ENDPOINT, null, {
            url: `subscriptions/resolveToken`,
            method: "POST",
            data: token
        });

        return result;
    }
}