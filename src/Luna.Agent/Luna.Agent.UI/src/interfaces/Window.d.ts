// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

interface Window {
    userToken: string;

    /** @string Base API endpoint. */
    BASE_URL: string;
    /** @string Header background color. (default color is: #3376CD) */
    HEADER_HEX_COLOR: string;
    /** @string Global site title. */
    SITE_TITLE: string;
    
    /** AAD app configuration with options. */
    MSAL_CONFIG: IMSALConfig;
}

interface IMSALConfig {
    appId: string;
    redirectUri: string;
    scopes: string[];
};