// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

export interface IUserLookUp {
  '@odata.context': string;
  businessPhones: string[];
  displayName: string;
  givenName: string;
  jobTitle: string;
  mail: string;
  mobilePhone?: any;
  officeLocation: string;
  preferredLanguage?: any;
  surname: string;
  userPrincipalName: string;
  id: string;
}