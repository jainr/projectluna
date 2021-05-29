import React from 'react';
import { Stack, Nav, INavLink, FontIcon } from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
import { WebRoute } from "../shared/constants/routes";


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
    <Stack
      horizontal={true}      
      className={"admincentercontent"}
      styles={{
        root: {
          height: 'calc(100% - 57px)'
        }
      }}
    >
      <Stack className={"abovenav"}>

      <div className={"collapsearrow"}>
      <FontIcon iconName="DoubleChevronLeft8" className="Arrowicon" />
      </div>
      <Nav
        selectedKey={selectedMenuItemKey}
        selectedAriaLabel="Selected"

        styles={{
          navItems: {
            margin: 0
          },
          root: {
            width: 207,
            height: '96%',
            boxSizing: 'border-box',
            border: '1px solid #eee',
            overflowY: 'auto',
          }
        }}
        groups={[
          {
            links: navLinks
          }
        ]}
      />
      </Stack>
      <Stack
        horizontal={true}
        className={"background"}
        styles={{
          root: {
            flexGrow: 1,
            height: '100%',
            padding: 32
          }
        }}
      >
        <Stack
          horizontal={true}
          className={"innerbackground"}
          styles={{
            root: {
              flexGrow: 1,
              height: '100%',
              padding: 32
            }
          }}
        >
          {children}
        </Stack>
      </Stack>
    </Stack>
  );
};

export default Content;