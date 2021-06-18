import React from 'react';
import { Stack, Nav, INavLink, FontIcon } from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
import { WebRoute } from "../shared/constants/routes";
import Navigation from './Navigation';

const Content: React.FunctionComponent = (props) => {

  const { children } = props;

  const history = useHistory();
  const location = useLocation();
  const v1Enabled = (window.Configs.ENABLE_V1.toLowerCase() === 'true' ? true : false);
  const v2Enabled = (window.Configs.ENABLE_V2.toLowerCase() === 'true' ? true : false);

  let offersTabActive = (location.pathname.toLowerCase().startsWith('/offers')
    || location.pathname.toLowerCase().startsWith('/modifyoffer'));
  let settingsActive = (location.pathname.toLowerCase().startsWith('/settings'));
  let productsTabActive = (location.pathname.toLowerCase().startsWith('/products'));
  let subscriptionTabActive = (location.pathname.toLowerCase().startsWith('/subscriptions'));
  let selectedMenuItemKey = '';
  if (offersTabActive) {
    selectedMenuItemKey = 'Offers';
  }
  if (productsTabActive) {
    selectedMenuItemKey = 'Products';
  }
  if (subscriptionTabActive) {
    selectedMenuItemKey = 'Subscriptions';
  }
  if (settingsActive) {
    selectedMenuItemKey = 'Settings';
  }

  let navLinks: INavLink[] = [];

  if (v1Enabled || v2Enabled) {
    navLinks.push({
      url: '',
      onClick: (ev, item) => { history.push(WebRoute.Home) },
      name: 'Home',
      key: 'Home',
      icon: 'Home',
    });

    navLinks.push({
      url: '',
      onClick: (ev, item) => { history.push(WebRoute.Reports) },
      name: 'Reports',
      key: 'Reports',
      icon: 'BarChartVertical'
    });

    navLinks.push({
      url: '',
      onClick: (ev, item) => { history.push(WebRoute.Supports) },
      name: 'Supports',
      key: 'Supports',
      icon: 'Settings'
    });

    navLinks.push({
      url: '',
      onClick: (ev, item) => { history.push(WebRoute.Settings) },
      name: 'Settings ',
      key: 'Settings',
      icon: 'Settings'
    });
  }

  return (
    <React.Fragment>
      <main>
        <Navigation />
        <div className="contentarea">
          {children}
        </div>
      </main>
    </React.Fragment>
  );
};

export default Content;