// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

export interface ISubscriptionDetail {
  AMLWorkspaceComputeClusterName: string;
  AMLWorkspaceDeploymentClusterName: string;
  AMLWorkspaceDeploymentTargetType: string;
  AMLWorkspaceId: number;
  AMLWorkspaceName: string;
  Admins: any[];
  AgentId: string;
  AvailablePlans: any[];
  BaseUrl: string;
  CreatedTime: string;
  DeploymentName: string;
  HostType: string;
  Id: number;
  OfferName?: any;
  PlanName?: any;
  PrimaryKey: string;
  PrimaryKeySecretName: string;
  ProductName: string;
  ProductType: string;
  PublisherId: string;
  SecondaryKey: string;
  SecondaryKeySecretName: string;
  Status: string;
  SubscriptionId: string;
  SubscriptionName: string;
  UserId: string;
  Users: any[];
  primaryKey: string;
  secondaryKey: string;
}