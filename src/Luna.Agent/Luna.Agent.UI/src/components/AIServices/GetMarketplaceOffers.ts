// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * Get all marketplace offers.
 */
export async function GetMarketplaceOffers() {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
    const userPrincipalId = sessionStorage.getItem('_userEmail');

    return await fetch(`${window.BASE_URL}/marketplaceOffers?userId=${userPrincipalId}`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.json());
}