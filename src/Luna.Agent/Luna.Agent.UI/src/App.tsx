// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';
import SiteHeader from './components/SiteHeader/SiteHeader';
import Navigation from './components/Navigation/Navigation';
import withAuthProvider, { AuthComponentProps } from './AuthProvider';
import { BrowserRouter, Router, Route, Switch } from 'react-router-dom';
import Home from './components/Home/Home';
import AIServices from './components/AIServices/AIServices';
import AIServiceDetails from './components/AIServices/AIServiceDetails';
import DataSources from './components/DataSources/DataSources';
import Settings from './components/Settings/Settings';
import Tasks from './components/Tasks/Tasks';
import SubscriptionDetail from './components/SubscriptionDetail/SubscriptionDetail';
import { Spinner, SpinnerSize } from '@fluentui/react';

interface IAppState {
  isAuthenticated?: boolean;
  user?: IUser;
  error?: string;
}

interface IUser {
  displayName?: string;
  email?: string;
  businessPhones?: string[];
  officeLocation?: string;
}

/** @component Top level App element for all UI. */
class App extends React.Component<AuthComponentProps, IAppState> {

  constructor(props: AuthComponentProps) {
    super(props);

    this.state = {
      user: {
        displayName: "",
        email: "",
        businessPhones: [],
        officeLocation: ""
      }
    }
  }

  componentDidMount() {
    setTimeout(() => {
      if (!this.props.isAuthenticated) {
        this.props.login();
      } else {
        sessionStorage.setItem('_userEmail', this.props.user?.email);
      
        setTimeout(() => {
          // Refresh token every 60s.
          var refreshToken = setInterval(()=> {
            this.props.getRefreshAccessToken(window.MSAL_CONFIG.scopes);
          }, 600000);
        }, 5000);
      }
    }, 4500);
  }

  render() {
    return (
      <div className="App">
        <SiteHeader
          userLocation={this.props.user?.officeLocation}
          userDisplayName={this.props.user?.displayName}
          userPhotoDataStr={this.props.userPhoto?.toString()}
        />
        <main>
          {
            this.props.isAuthenticated &&
            <Navigation />
          }
          <div className="contentarea">
            {
              this.props.isAuthenticated &&
              
                <BrowserRouter >
                <Route path="/aiservices" component={AIServices} />
                <Route path="/" exact component={Home} />
                <Route path="/datasources" component={DataSources} />
                <Route path="/tasks" component={Tasks} />
                <Route path="/settings" component={Settings} />
                <Route path="/details/:id?" component={SubscriptionDetail} />
                <Route path="/servicedetails" component={AIServiceDetails} />                   
          </BrowserRouter>
            }
          </div>
        </main>
        {
            !this.props.isAuthenticated &&
            <div style={{ marginTop: '40px' }}>
              <Spinner size={SpinnerSize.large} label="Logging in and loading data..."  />
            </div>
          }
      </div>
    );
  }
}

export default withAuthProvider(App);
