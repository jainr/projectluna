// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ISubscriptionAPI } from './ISubscriptionAPI';

export interface ISubscriptionApplication {
  name: string;
  description: string;
  //type: string;
  apIs: ISubscriptionAPI[];
}