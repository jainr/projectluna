// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * Get all internal focused offers.
 */
export async function GetInternalOffers() {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
    const userPrincipalId = sessionStorage.getItem('_userEmail');

    return await fetch(`${window.BASE_URL}/AIMarketplaceOffers?userId=${userPrincipalId}`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.json())
}