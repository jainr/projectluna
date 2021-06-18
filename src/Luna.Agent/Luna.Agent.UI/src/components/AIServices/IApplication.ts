// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { IApplicationDetails } from './IApplicationDetails';
import { IApplicationTags } from './IApplicationTags';

export interface IApplication {
  UniqueName: "",
  DisplayName: "",
  Description: "",
  LogoImageUrl: "",
  DocumentationUrl: "",
  Publisher: "",
  Tags: IApplicationTags[],
  Details: IApplicationDetails
}