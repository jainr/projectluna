// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

export interface IUserPhotoData {
  arrayBuffer: ArrayBuffer | undefined,
  size: number | undefined,
  slice: Function | undefined,
  stream: Function | undefined,
  text: Function | undefined,
  type: string | undefined
}