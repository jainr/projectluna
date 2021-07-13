import {IBaseModel} from "./IBaseModel";

export interface IOfferParameterModel extends IBaseModel {
  ParameterName: string;
  DisplayName: string;
  Description: string;
  ValueType: string;
  FromList: boolean;
  ValueList: string;
  Maximum: number | null;
  Minimum: number | null;
}