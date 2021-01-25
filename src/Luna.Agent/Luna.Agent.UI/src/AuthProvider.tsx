// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import { UserAgentApplication } from 'msal';
import { getUserDetails, getUserPhotoDetails, getUserByEmail } from './GraphService';

interface AuthProviderState {
  error: any;
  isAuthenticated: boolean;
  user: any;
  userPhoto: string | ArrayBuffer | null | undefined;
}
export interface AuthComponentProps {
  error: any;
  isAuthenticated: boolean;
  user: any;
  login: Function;
  logout: Function;
  getAccessToken: Function;
  getRefreshAccessToken: Function;
  setError: Function;
  userPhoto: string | ArrayBuffer | null | undefined;
}

export default function withAuthProvider<T extends React.Component<AuthComponentProps>>
  (WrappedComponent: new (props: AuthComponentProps, context?: any) => T): React.ComponentClass {
  return class extends React.Component<any, AuthProviderState> {
    private userAgentApplication: UserAgentApplication;

    constructor(props: any) {
      super(props);
      this.state = {
        error: null,
        isAuthenticated: false,
        user: {},
        userPhoto: ""
      };

      // Initialize the MSAL application object
      this.userAgentApplication = new UserAgentApplication({
        auth: {
          clientId: window.MSAL_CONFIG.appId,
          redirectUri: window.MSAL_CONFIG.redirectUri
        },
        cache: {
          cacheLocation: "sessionStorage",
          storeAuthStateInCookie: true
        }
      });
    }

    componentDidMount() {
      // If MSAL already has an account, the user
      // is already logged in
      var account = this.userAgentApplication.getAccount();

      if (account) {
        // Enhance user object with data from Graph
        this.getUserProfile();
      }
    }

    render() {
      return <WrappedComponent
        userPhoto={this.state.userPhoto}
        login={() => this.login()}
        logout={() => this.logout()}
        getAccessToken={(scopes: string[]) => this.getAccessToken(scopes)}
        getRefreshAccessToken={(scopes: string[]) => this.getRefreshAccessToken(scopes)}
        setError={(message: string, debug: string) => this.setErrorMessage(message, debug)}
        {...this.props} {...this.state} />;
    }

    async login() {
      try {
        // Login via popup
        await this.userAgentApplication.loginRedirect(
          {
            scopes: window.MSAL_CONFIG.scopes,
            prompt: "select_account",
            forceRefresh: true
          });
        // After login, get the user's profile
        await this.getUserProfile();
      }
      catch (err) {
        this.setState({
          isAuthenticated: false,
          user: {},
          error: this.normalizeError(err)
        });
      }
    }

    logout() {
      this.userAgentApplication.logout();
    }

    async getAccessToken(scopes: string[]): Promise<string> {
      try {
        // Get the access token silently
        // If the cache contains a non-expired token, this function
        // will just return the cached token. Otherwise, it will
        // make a request to the Azure OAuth endpoint to get a token
        var silentResult = await this.userAgentApplication.acquireTokenSilent({
          scopes: scopes
        });

        //sessionStorage.setItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`, silentResult.accessToken);
        console.info('updated the session storage.')

        return silentResult.accessToken;
      } 
      catch (err) {
        // If a silent request fails, it may be because the user needs
        // to login or grant consent to one or more of the requested scopes
        if (this.isInteractionRequired(err)) {
          var interactiveResult = await this.userAgentApplication.acquireTokenPopup({
            scopes: scopes
          });
          return interactiveResult.accessToken;
        } else {
          throw err;
        }
      }
    }

    async getRefreshAccessToken(scopes: string[]): Promise<string> {
      
      //remove session storage keys
      const keys = await Object.keys(sessionStorage).filter(x => x.indexOf('authority') > 0)
      keys.forEach(x => sessionStorage.removeItem(x));
      try {
        // Get the access token silently
        // If the cache contains a non-expired token, this function
        // will just return the cached token. Otherwise, it will
        // make a request to the Azure OAuth endpoint to get a token
        var silentResult = await this.userAgentApplication.acquireTokenSilent({
          scopes: scopes
        });
        console.info('updated the session storage.')
        return silentResult.accessToken;
      } catch (err) {
        // If a silent request fails, it may be because the user needs
        // to login or grant consent to one or more of the requested scopes
        if (this.isInteractionRequired(err)) {
          var interactiveResult = await this.userAgentApplication.acquireTokenPopup({
            scopes: scopes
          });

          return interactiveResult.accessToken;
        } else {
          throw err;
        }
      }
    }

    async getUserPhoto() {
      try {
        var accessToken = await this.getAccessToken(window.MSAL_CONFIG.scopes);
        if (accessToken) {
          // Get the user's profile from Graph
          let _userPhoto = await getUserPhotoDetails(accessToken);
          let fileReader = new FileReader();
          let that = this;
          fileReader.onloadend = function (e) {
            if (e.target !== null) {
              let base64 = e.target.result;
              that.setState({
                userPhoto: base64
              });
            }
          }
          fileReader.readAsDataURL(_userPhoto);
        }
      }
      catch (err) {
        console.error("User photo not found.");
      }
    }

    async getUserProfile() {
      try {
        var accessToken = await this.getAccessToken(window.MSAL_CONFIG.scopes);
        if (accessToken) {

          // set global user token.
          window.userToken = accessToken;

          // Get the user's profile from Graph
          var user = await getUserDetails(accessToken);
          this.setState({
            isAuthenticated: true,
            user: {
              businessPhones: user.businessPhones,
              officeLocation: user.officeLocation,
              displayName: user.displayName,
              email: user.mail || user.userPrincipalName
            },
            error: null
          });
          this.getUserPhoto();
        }
      }
      catch (err) {
        this.setState({
          isAuthenticated: false,
          user: {},
          error: this.normalizeError(err)
        });
      }
    }

    setErrorMessage(message: string, debug: string) {
      this.setState({
        error: { message: message, debug: debug }
      });
    }

    normalizeError(error: string | Error): any {
      var normalizedError = {};
      if (typeof (error) === 'string') {
        var errParts = error.split('|');
        normalizedError = errParts.length > 1 ?
          { message: errParts[1], debug: errParts[0] } :
          { message: error };
      } else {
        normalizedError = {
          message: error.message,
          debug: JSON.stringify(error)
        };
      }
      return normalizedError;
    }

    isInteractionRequired(error: Error): boolean {
      if (!error.message || error.message.length <= 0) {
        return false;
      }

      return (
        error.message.indexOf('consent_required') > -1 ||
        error.message.indexOf('interaction_required') > -1 ||
        error.message.indexOf('login_required') > -1
      );
    }
  }
}