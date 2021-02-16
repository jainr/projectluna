// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ISubscriptionAPIVersionOperationParameter } from './ISubscriptionAPIVersionOperationParameter';

export interface ISubscriptionAPIVersionOperation {
  displayName: string;
  name: string;
  description: string;
  parameters: ISubscriptionAPIVersionOperationParameter[];

}