// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

export interface IUserLookupError {
  error: ErrorMessage;
}

interface ErrorMessage {
  code: string;
  message: string;
  innerError: InnerError;
}

interface InnerError {
  date: string;
  'request-id': string;
}