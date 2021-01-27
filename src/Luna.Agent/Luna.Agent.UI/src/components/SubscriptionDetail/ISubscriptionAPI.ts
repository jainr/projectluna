// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ISubscriptionAPIVersion } from './ISubscriptionAPIVersion';

export interface ISubscriptionAPI {
  name: string;
  description: string;
  //type: string;
  versions: ISubscriptionAPIVersion[];
}