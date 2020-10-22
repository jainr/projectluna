// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

var graph = require('@microsoft/microsoft-graph-client');

function getAuthenticatedClient(accessToken: string) {
  // Initialize Graph client
  const client = graph.Client.init({
    // Use the provided access token to authenticate
    // requests
    authProvider: (done: any) => {
      done(null, accessToken);
    }
  });

  return client;
}

export async function getUserDetails(accessToken: string) {
  const client = getAuthenticatedClient(accessToken);

  const user = await client.api('/me').get();
  return user;
}

export async function getUserByEmail(accessToken: string, userEmail: string) {
  const client = getAuthenticatedClient(accessToken);

  const user = await client.api('/users/' + userEmail).get();
  return user;
}

export async function getUserPhotoDetails(accessToken: string) {
  const client = getAuthenticatedClient(accessToken);

  const user = await client.api('/me/photo/$value').get();
  return user;
}

export async function getOtherUserPhoto(accessToken: string, userId: string) {
  const client = getAuthenticatedClient(accessToken);

  const user = await client.api(`/users/${userId}/photo/$value`).get();
  return user;
}