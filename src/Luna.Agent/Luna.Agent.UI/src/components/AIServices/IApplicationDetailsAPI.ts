// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { IApplicationDetailsAPIVersions } from './IApplicationDetailsAPIVersions';

export interface IApplicationDetailsAPI {
  name:string,
  type: string,
  versions:IApplicationDetailsAPIVersions[] 
}