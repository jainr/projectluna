// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ISubscriptionOwner } from './ISubscriptionOwner';

export interface IApplicationSubscription
{
  baseUrl: string,
  createdTime: string,
  primaryKey: string,
  secondaryKey: string,
  notes: string,
  subscriptionId: string,
  subscriptionName: string,
  owner: ISubscriptionOwner[]
}