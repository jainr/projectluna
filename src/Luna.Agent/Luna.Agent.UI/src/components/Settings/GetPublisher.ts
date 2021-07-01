// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { IMarketPlacePublisher } from "../AIServices/ISettings";

/**
 * Get all internal focused offers.
 */

// const LunaUserId = sessionStorage.getItem("_userEmail");
const LunaUserId = 'test-admin';
export async function GetInternalPublisher() {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    return await fetch(`${window.BASE_URL}/gallery/applicationpublishers?type=Internal`, {
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

export async function GetMarketPlacePublisher() {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    return await fetch(`${window.BASE_URL}/gallery/applicationpublishers?type=Marketplace`, {
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

export async function UpdateMarketPlacePublisher(values:IMarketPlacePublisher) {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    return await fetch(`${window.BASE_URL}/gallery/applicationpublishers/${values.Name}`, {
        mode: "cors",
        method: "PUT",
        headers: {
            'Luna-User-Id': `${LunaUserId}`,
            'Host': 'lunatest-gateway.azurewebsites.net',
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body:JSON.stringify(values)
    })
        .then(response => response.json())
}

