// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import SyntaxHighlighter from "react-syntax-highlighter";
import { vs } from "react-syntax-highlighter/dist/esm/styles/hljs";
import mytext from './SampleCode';
import FooterLinks from '../FooterLinks/FooterLinks';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, IconButton, FontIcon } from '@fluentui/react';
import { PanelStyles } from '../../helpers/PanelStyles';
import { getTheme } from '@fluentui/react';
import { GetInternalOffers } from './GetInternalOffers';
import { SharedColors } from '@uifabric/fluent-theme';
import { GetMarketplaceOffers } from './GetMarketplaceOffers';
import { useBoolean } from '@uifabric/react-hooks';
import { ISubscription } from '../../interfaces/ISubscription';

import './../AIServices/AIServices.css';
import { getTypeParameterOwner } from 'typescript';
import { PromptState } from 'msal/lib-commonjs/utils/Constants';
import { withRouter } from "react-router-dom";
import { useHistory, useLocation } from 'react-router';
import { GetMyApplication, GetMySubscriptionByApplication, GetinternalPublisherApplication, GetMarketPlaceApplication } from './GetMyApplication';
import { IApplication } from './IApplication';
import { IApplicationTags } from './IApplicationTags';


function generateUUID() { // Public Domain/MIT
  var d = new Date().getTime();//Timestamp
  var d2 = (performance && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
    var r = Math.random() * 16;//random number between 0 and 16
    if (d > 0) {//Use timestamp until depleted
      r = (d + r) % 16 | 0;
      d = Math.floor(d / 16);
    } else {//Use microseconds since page-load if supported
      r = (d2 + r) % 16 | 0;
      d2 = Math.floor(d2 / 16);
    }
    return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
  });
}

const theme = getTheme();

const AIServices = () => {
  const history = useHistory();
  const [offerData, setOfferData] = React.useState<any[]>();
  const [initOffferData, setInitOfferData] = React.useState<any[]>();
  const [isOpen, { setTrue: openPanel, setFalse: dismissPanel }] = useBoolean(false);
  const [isNewSubSuccessful, setIsNewSubSuccessful] = React.useState(false);
  const [isDataLoading, setIsDataLoading] = React.useState(true);
  const [planOptions, setPlanOptions] = React.useState<IDropdownOption[]>([]);
  const [hideNewSubDialog, setHideNewSubDialog] = React.useState(true);
  const [myApplication, setMyApplication] = React.useState<IApplication[]>();
  const [allApplication, setAllApplication] = React.useState<IApplication[]>();
  const [publisherApplication, setpublisherApplication] = React.useState<IApplication[]>();
  const [marketPlaceApplication, setMarketPlaceApplication] = React.useState<IApplication[]>();
  const [isMyAppLoading, setIsMyAppLoading] = React.useState<boolean>(true);
  const [isublisherAppLoading, setIsPublisherAppLoading] = React.useState<boolean>(true);
  const [isMarketPlaceAppLoading, setMarketPlaceAppLoading] = React.useState<boolean>(true);
  const [isListView, setIsListView] = React.useState<boolean>(false);
  const [designViewText, setDesignViewText] = React.useState<string>('Switch to List view');

  const [newSubscription, setNewSubscription] = React.useState<ISubscription>({
    OfferDisplayName: '',
    OfferName: '',
    PlanName: '',
    Name: '',
    SubscriptionId: '',
    Owner: ''
  });

  React.useEffect(() => {
    loadData();
  }, []);

  /**
   * Load data and merge all offer types into one object.
   */
  const loadData = async () => {
    let allapps: IApplication[] = [];

    /*Get MyApplication starts*/
    setIsMyAppLoading(true);
    let myapps: IApplication[] = [];
    let myApplications = await GetMyApplication();
    for (const key in myApplications) {
      if (Object.prototype.hasOwnProperty.call(myApplications, key)) {
        const element = myApplications[key];
        let mysubbyapp = await GetMySubscriptionByApplication(element.UniqueName);
        if (mysubbyapp.length > 0) {
          element.type = 'MyApplication';
          myapps.push(element);
          allapps.push(element);
        }
      }
    }
    setIsMyAppLoading(false);
    setMyApplication(myapps);

    /*Get MyApplication ends*/

    /*Get Publisher Application starts*/
    setIsPublisherAppLoading(true);
    let ipublisherapps: IApplication[] = [];
    let ipublisherApplications = await GetinternalPublisherApplication();
    for (const key in ipublisherApplications) {
      if (Object.prototype.hasOwnProperty.call(ipublisherApplications, key)) {
        const element = ipublisherApplications[key];
        element.type = 'PublisherApplication';
        if(myapps.filter(x=>x.UniqueName== element.UniqueName).length>0)
        {
          element.isSubScribed = true;
        }
        allapps.push(element);
        ipublisherapps.push(element);
      }
    }
    setIsPublisherAppLoading(false);
    setpublisherApplication(ipublisherapps);
    /*Get Publisher Application ends*/

    /*Get Market Application starts*/
    setMarketPlaceAppLoading(true)
    let marketPlaceapps: IApplication[] = [];
    let marketplaceApplications = await GetMarketPlaceApplication();
    for (const key in marketplaceApplications) {
      if (Object.prototype.hasOwnProperty.call(marketplaceApplications, key)) {
        const element = marketplaceApplications[key];
        element.type = 'MarketPlaceApplication';     
        if(myapps.filter(x=>x.UniqueName== element.UniqueName).length>0)
        {
          element.isSubScribed = true;
        }   
        allapps.push(element);
        marketPlaceapps.push(element);
      }
    }
    setMarketPlaceAppLoading(false)
    setMarketPlaceApplication(marketPlaceapps);

    /*Get Market Application ends*/

    setAllApplication(allapps);

  }

  const searchFilter = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {        
    if (newValue && newValue.length > 3) {      
      if (newValue) {
        let MyApplication = allApplication?.filter(x => x.type.includes("MyApplication") && x.DisplayName.includes(newValue));
        setMyApplication(MyApplication);

        let PublisherApplication = allApplication?.filter(x => x.type.includes("PublisherApplication") && x.DisplayName.includes(newValue));
        setpublisherApplication(PublisherApplication);

        let MarketPlaceApplication = allApplication?.filter(x => x.type.includes("MarketPlaceApplication") && x.DisplayName.includes(newValue));
        setMarketPlaceApplication(MarketPlaceApplication);
      }
      else {
        let MyApplication = allApplication?.filter(x => x.type.includes("MyApplication"));
        setMyApplication(MyApplication);

        let PublisherApplication = allApplication?.filter(x => x.type.includes("PublisherApplication"));
        setpublisherApplication(PublisherApplication);

        let MarketPlaceApplication = allApplication?.filter(x => x.type.includes("MarketPlaceApplication"));
        setMarketPlaceApplication(MarketPlaceApplication);
      }
    }
    else {
      let MyApplication = allApplication?.filter(x => x.type.includes("MyApplication"));
      setMyApplication(MyApplication);

      let PublisherApplication = allApplication?.filter(x => x.type.includes("PublisherApplication"));
      setpublisherApplication(PublisherApplication);

      let MarketPlaceApplication = allApplication?.filter(x => x.type.includes("MarketPlaceApplication"));
      setMarketPlaceApplication(MarketPlaceApplication);
    }
    // setOfferData(filterByValue(initOffferData!, newValue!));
  }

  const selectApplication = (selectedApp: string) => {
    sessionStorage.setItem('selectedApplication', selectedApp);
    history.push("servicedetails");
  }

  const ChangeView = () => {
    if (isListView) {
      setDesignViewText('Switch to List view');
      setIsListView(false);
    }
    else {
      setDesignViewText('Switch to Tile view');
      setIsListView(true);
    }
  }

  return (
    <div className="AIServices">
      <div style={PanelStyles}>
        <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
          <StackItem>
            <Text block variant={'xLargePlus'}>Machine Learning Gallery</Text>
          </StackItem>
          <StackItem>
            <TextField
              underlined
              autoComplete="false"
              autoCorrect="false"
              autoFocus={true}
              iconProps={{ iconName: 'Search' }}
              onChange={searchFilter}
              label="Search"
            ></TextField>
          </StackItem>
        </Stack>
        <Stack className="section">
          <hr style={{ width: "100%" }} />
          <Text block variant={'xLarge'} className="title">My Applications
            <a className="choosedesigntype" onClick={ChangeView}>{designViewText}</a>
          </Text>
          {
            isListView ?
              <React.Fragment>
                <table cellSpacing={0} cellPadding={0}>
                  <thead>
                    <th>
                      Application Name
                    </th>
                    <th>
                      Publisher
                    </th>
                    <th>
                      Description
                    </th>
                  </thead>
                  <tbody>
                    {
                      isMyAppLoading ? <tr><td colSpan={3}>loading data.....</td></tr>
                        :
                        myApplication && myApplication?.length > 0 ?
                          myApplication?.map((values: IApplication, idx: number) => {
                            return (
                              <tr>
                                <td>
                                  <a onClick={(event) => selectApplication(values.UniqueName)}>
                                    {values.DisplayName}
                                  </a>
                                </td>
                                <td>
                                  {values.Publisher}
                                </td>
                                <td>
                                  {values.Description}
                                </td>
                              </tr>
                            )
                          })
                          :
                          <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! You don’t own any application yet! Choose an application below to start with.</Text>
                    }
                  </tbody>
                </table>
              </React.Fragment>
              : <React.Fragment>

                <div>
                  {
                    isMyAppLoading ? 'loading data.....'
                      :
                      myApplication && myApplication?.length > 0 ?
                        myApplication?.map((values: IApplication, idx: number) => {
                          return (
                            <div className="appblock" key={idx} onClick={(event) => selectApplication(values.UniqueName)}>
                              <IconButton style={{ color: SharedColors.blue10 }}
                                iconProps={{ iconName: "TestBeakerSolid" }} size={30} />
                              <Text block variant={'xLarge'} className="heading">{values.DisplayName}</Text>
                              <p className="description">
                                {values.Description}
                              </p>
                              <p className="publisher">
                                <Text block variant={"small"}>Publisher: {values.Publisher}</Text>
                              </p>
                              <hr style={{ width: "100%", color: 'grey' }} />
                              <div className="tags">
                                {
                                  values.Tags.map((tagvalues: IApplicationTags, tagidx: any) => {
                                    let data = values.Tags.length > 1 ? tagvalues.name + ' | ' : tagvalues.name;
                                    return (
                                      data
                                    )
                                  })
                                }
                              </div>
                              <div className="subscribeddiv">
                                <FontIcon aria-label="Compass" iconName="CircleFill" style={{ paddingTop: '3%' }} />
                                <span className="subscribedtext"> Subcribed</span>
                              </div>
                            </div>
                          )
                        })
                        :
                        <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! You don’t own any application yet! Choose an application below to start with.</Text>
                  }
                </div>
              </React.Fragment>
          }
        </Stack>
        <Stack className="section">
          <Text block variant={'xLarge'} className="title">Applications from internal publishers
          </Text>
          {
            isListView ?
              <React.Fragment>
                <table cellSpacing={0} cellPadding={0}>
                  <thead>
                    <th>
                      Application Name
                    </th>
                    <th>
                      Publisher
                    </th>
                    <th>
                      Description
                    </th>
                  </thead>
                  <tbody>
                    {
                      isublisherAppLoading ? <tr><td colSpan={3}>loading data.....</td></tr>
                        :
                        publisherApplication && publisherApplication?.length > 0 ?
                          publisherApplication?.map((values: IApplication, idx: number) => {
                            return (
                              <tr>
                                <td>
                                  <a onClick={(event) => selectApplication(values.UniqueName)}>
                                    {values.DisplayName}
                                  </a>
                                </td>
                                <td>
                                  {values.Publisher}
                                </td>
                                <td>
                                  {values.Description}
                                </td>
                              </tr>
                            )
                          })
                          :
                          <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! You don’t own any application yet! Choose an application below to start with.</Text>
                    }
                  </tbody>
                </table>
              </React.Fragment>
              : <React.Fragment>

                <div>
                  {
                    isublisherAppLoading ? 'loading data.....'
                      :
                      publisherApplication && publisherApplication?.length > 0 ?
                        publisherApplication?.map((values: IApplication, idx: number) => {
                          return (
                            <div className="appblock" key={idx} onClick={(event) => selectApplication(values.UniqueName)}>
                              <IconButton style={{ color: SharedColors.blue10 }}
                                iconProps={{ iconName: "TestBeakerSolid" }} size={30} />
                              <Text block variant={'xLarge'} className="heading">{values.DisplayName}</Text>
                              <p className="description">
                                {values.Description}
                              </p>
                              <p className="publisher">
                                <Text block variant={"small"}>Publisher: {values.Publisher}</Text>
                              </p>
                              <hr style={{ width: "100%", color: 'grey' }} />
                              <div className="tags">
                                {
                                  values.Tags.map((tagvalues: IApplicationTags, tagidx: any) => {
                                    let data = values.Tags.length > 1 ? tagvalues.name + ' | ' : tagvalues.name;
                                    return (
                                      data
                                    )
                                  })
                                }
                              </div>
                              {
                                values.isSubScribed ?
                                <div className="subscribeddiv">
                                <FontIcon aria-label="Compass" iconName="CircleFill" style={{ paddingTop: '3%' }} />
                                <span className="subscribedtext"> Subcribed</span>
                              </div>
                                :null
                              }
                            </div>
                          )
                        })
                        :
                        <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! You don’t own any application yet! Choose an application below to start with.</Text>
                  }
                </div>
              </React.Fragment>
          }
        </Stack>
        <Stack className="section" style={{marginBottom:'5%'}}>
          <Text block variant={'xLarge'} className="title">Applications from Azure Marketplace
          </Text>
          {
            isListView ?
              <React.Fragment>
                <table cellSpacing={0} cellPadding={0}>
                  <thead>
                    <th>
                      Application Name
                    </th>
                    <th>
                      Publisher
                    </th>
                    <th>
                      Description
                    </th>
                  </thead>
                  <tbody>
                    {
                      isMarketPlaceAppLoading ? <tr><td colSpan={3}>loading data.....</td></tr>
                        :
                        marketPlaceApplication && marketPlaceApplication?.length > 0 ?
                          marketPlaceApplication?.map((values: IApplication, idx: number) => {
                            return (
                              <tr>
                                <td>
                                  <a onClick={(event) => selectApplication(values.UniqueName)}>
                                    {values.DisplayName}
                                  </a>
                                </td>
                                <td>
                                  {values.Publisher}
                                </td>
                                <td>
                                  {values.Description}
                                </td>
                              </tr>
                            )
                          })
                          :
                          <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! You don’t own any application yet! Choose an application below to start with.</Text>
                    }
                  </tbody>
                </table>
              </React.Fragment>
              : <React.Fragment>

                <div>
                  {
                    isMarketPlaceAppLoading ? 'loading data.....'
                      :
                      marketPlaceApplication && marketPlaceApplication?.length > 0 ?
                        marketPlaceApplication?.map((values: IApplication, idx: number) => {
                          return (
                            <div className="appblock" key={idx} onClick={(event) => selectApplication(values.UniqueName)}>
                              <IconButton style={{ color: SharedColors.blue10 }}
                                iconProps={{ iconName: "TestBeakerSolid" }} size={30} />
                              <Text block variant={'xLarge'} className="heading">{values.DisplayName}</Text>
                              <p className="description">
                                {values.Description}
                              </p>
                              <p className="publisher">
                                <Text block variant={"small"}>Publisher: {values.Publisher}</Text>
                              </p>
                              <hr style={{ width: "100%", color: 'grey' }} />
                              <div className="tags">
                                {
                                  values.Tags.map((tagvalues: IApplicationTags, tagidx: any) => {
                                    let data = values.Tags.length > 1 ? tagvalues.name + ' | ' : tagvalues.name;
                                    return (
                                      data
                                    )
                                  })
                                }
                              </div>
                              {
                                values.isSubScribed ?
                                <div className="subscribeddiv">
                                <FontIcon aria-label="Compass" iconName="CircleFill" style={{ paddingTop: '3%' }} />
                                <span className="subscribedtext"> Subcribed</span>
                              </div>
                                :null
                              }                              
                            </div>
                          )
                        })
                        :
                        <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! You don’t own any application yet! Choose an application below to start with.</Text>
                  }
                </div>
              </React.Fragment>
          }
        </Stack>
      </div>
      <br />
      <FooterLinks />
    </div>
  );
}

function filterByValue(array: any[], value: string) {
  return array.filter((data) => JSON.stringify(data).toLowerCase().indexOf(value.toLowerCase()) !== -1);
}

export interface IPlan {
  PlanName: string;
  PlanDisplayName: string;
  Description: string;
}
export default AIServices;