// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ISubscriptionAPIVersionOperation } from './ISubscriptionAPIVersionOperation';

export interface ISubscriptionAPIVersion {
  name: string;
  //description: string;
  operations: ISubscriptionAPIVersionOperation[];
}