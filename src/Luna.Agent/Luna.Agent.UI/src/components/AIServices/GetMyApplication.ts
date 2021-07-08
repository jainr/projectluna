// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * Get all internal focused offers.
 */

// const LunaUserId = sessionStorage.getItem("_userEmail");
const LunaUserId = 'test-admin';
export async function GetMyApplication() {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    return await fetch(`${window.BASE_URL}/gallery/applications`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Luna-User-Id': `${LunaUserId}`,
            'Host': 'lunatest-gateway.azurewebsites.net',
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
}

export async function GetinternalPublisherApplication() {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    return await fetch(`${window.BASE_URL}/gallery/applications`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Luna-User-Id': `${LunaUserId}`,
            'Host': 'lunatest-gateway.azurewebsites.net',
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
}

export async function GetMarketPlaceApplication() {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    return await fetch(`${window.BASE_URL}/gallery/applications`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Luna-User-Id': `${LunaUserId}`,
            'Host': 'lunatest-gateway.azurewebsites.net',
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
}

export async function GetMySubscriptionByApplication(applicationName: string) {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
    return await fetch(`${window.BASE_URL}/gallery/applications/${applicationName}/subscriptions`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Luna-User-Id': `${LunaUserId}`,
            'Host': 'lunatest-gateway.azurewebsites.net',
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
}

export async function GetRecommendedApplication(applicationName: any) {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    return await fetch(`${window.BASE_URL}/gallery/applications/${applicationName}/recommended`, {
        mode: "cors",
        method: "GET",
        headers: {
            'Luna-User-Id': `${LunaUserId}`,
            'Host': 'lunatest-gateway.azurewebsites.net',
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
}