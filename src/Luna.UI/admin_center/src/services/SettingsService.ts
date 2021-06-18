import { ServiceBase } from "./ServiceBase";
import {ISettingsModel, Result} from "../models";
import {v4 as uuid} from "uuid";

export default class SettingsService extends ServiceBase {

  public static async userDataList(): Promise<Result<ISettingsModel[]>> {

    var result = await this.requestJson<ISettingsModel[]>({
      url: `/offers`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }  
}