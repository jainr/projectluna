import { ServiceBase } from "./ServiceBase";
import { IPlanModel, Result, IARMTemplateModel,IRestrictedUsersModel } from "../models";
import { v4 as uuid } from "uuid";

// eslint-disable-next-line @typescript-eslint/no-unused-vars
let armTemplateModellist: IARMTemplateModel[];
export default class WizardService extends ServiceBase {

    public static async serviceTypesList(): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerServices/metadata/hostservicetypes`,
            method: "GET"            
        });

        return result;
    }

    public static async serviceList(option): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerServices?type=`+option,
            method: "GET"            
        });

        return result;
    }

    public static async componentTypeList(): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerservices/metadata/AzureML/mlcomponenttypes`,
            method: "GET"            
        });

        return result;
    }

    public static async componentList(option): Promise<Result<any>> {
        var result = await this.requestJson<any>({
            url: `/manage/partnerservices/amlworkspace/mlcomponents/`+option,
            method: "GET"            
        });

        return result;
    }
}

