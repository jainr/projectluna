// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ISubscriptionOwner } from './ISubscriptionOwner';

export interface IApplicationSubscription
{
  BaseUrl: string,
  CreatedTime: string,
  PrimaryKey: string,
  SecondaryKey: string,
  Notes: string,
  SubscriptionId: string,
  SubscriptionName: string,
  Owners: ISubscriptionOwner[]
}