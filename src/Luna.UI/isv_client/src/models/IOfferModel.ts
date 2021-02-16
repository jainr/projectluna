import {IBaseModel} from "./IBaseModel";

export interface IOfferModel extends IBaseModel {
  offerName: string;
  displayName: string;
  description: string;
  logoImageUrl: string;
  documentationUrl: string;
  tags: string;
  offerVersion: string;
  owners: string;
  hostSubscription: string;
  status: string;
  createdTime?: string;
  lastUpdatedTime?: string;
  Idlist?: string;
  selectedOfferName?:string
  selectedOfferindex?:number
}