// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * Get all internal focused offers.
 */
export async function GetApplicationDetails(applicationName : string) {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
    // const userPrincipalId = sessionStorage.getItem('_userEmail');

    return await fetch(`${window.BASE_URL}/gallery/applications/${applicationName}`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Luna-User-Id': 'test-admin',
            'Host': 'lunatest-gateway.azurewebsites.net',
            // 'Origin': 'http://localhost:3000',
            // 'Access-Control-Allow-Origin': 'http://localhost:3000',
            // 'Access-Control-Allow-Credentials': 'true',
            // 'Access-Control-Allow-Methods' : 'GET, POST, OPTIONS',
            // 'Access-Control-Allow-Headers' : 'Origin, Content-Type, Accept'
        },
    })
    // .then(response => {
    //     if (response.status === 0 ) {
    //         window.alert(`Error . - ${response.status}`);
    //     }})
    .then(response => response.json())
}