import { ServiceBase } from "./ServiceBase";
import {ISupportCasesModel, ISupportPermissionModel, Result} from "../models";
import {v4 as uuid} from "uuid";

export default class SupportService extends ServiceBase {

  public static async supportCaseList(): Promise<Result<ISupportCasesModel[]>> {

    var result = await this.requestJson<ISupportCasesModel[]>({
      url: `/offers`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }

  public static async permissionList(): Promise<Result<ISupportPermissionModel[]>> {

    var result = await this.requestJson<ISupportPermissionModel[]>({
      url: `/offers`,
      method: "GET"
    });

    if (!result.hasErrors && result.value)
      result.value.map(u => u.clientId = uuid());

    return result;
  }
}