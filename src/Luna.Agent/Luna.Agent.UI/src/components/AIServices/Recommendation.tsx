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
import { GetMySubscriptionByApplication, GetRecommendedApplication } from './GetMyApplication';
import { IApplication } from './IApplication';
import { IApplicationTags } from './IApplicationTags';

const Recommendation = (props: any) => {
  const history = useHistory();
  const [recommendedApplication, setRecommendedApplication] = React.useState<IApplication[]>();
  const [isrecommendedAppLoading, setRecommendedAppLoading] = React.useState<boolean>(true);

  const selectedApplication = sessionStorage.getItem("selectedApplication")?.toString();
  React.useEffect(() => {
    loadData();
  }, []);

  /**
   * Load data and merge all offer types into one object.
   */
  const loadData = async () => {

    /*Get MyApplication starts*/
    setRecommendedAppLoading(true);
    let appslist: IApplication[] = [];
    let recommendedApplications = await GetRecommendedApplication(selectedApplication);
    for (const key in recommendedApplications) {
      if (Object.prototype.hasOwnProperty.call(recommendedApplications, key)) {
        const element = recommendedApplications[key];
        element.type = 'MyApplication';
        let mysubbyapp = await GetMySubscriptionByApplication(element.UniqueName);
        if (mysubbyapp.length > 0) {          
          element.isSubScribed = true;         
        }
        else{          
          element.isSubScribed = false;         
        }
        appslist.push(element);
      }
    }
    setRecommendedAppLoading(false);
    setRecommendedApplication(appslist);

    /*Get MyApplication ends*/

  }

  const selectApplication = (selectedApp: IApplication) => {    
    sessionStorage.setItem('selectedApplication', selectedApp.UniqueName);
    sessionStorage.setItem('selectedApplicationObject', JSON.stringify(selectedApp));
    props.reloadLeftSection();
  }

  return (
    <div className="AIServices Recommended">
      <div style={PanelStyles} className="PanelStyles">
        <Stack className="section">
          <Text block variant={'xLarge'} className="title">You might be interested in these applications:</Text>
          {
            <React.Fragment>
              <div>
                {
                  isrecommendedAppLoading ? 'loading data.....'
                    :
                    recommendedApplication && recommendedApplication?.length > 0 ?
                      recommendedApplication?.map((values: IApplication, idx: number) => {
                        return (
                          <div className="appblock" key={idx} onClick={(event) => selectApplication(values)}>
                            <IconButton style={{ color: SharedColors.blue10 }} iconProps={{ iconName: "TestBeakerSolid" }} size={30} className="TestBeakericon" />
                            <Text block variant={'xLarge'} className="heading">{values.DisplayName}</Text>
                            <p className="description">
                              {values.Description}
                            </p>
                            <p className="publisher">
                              <Text block variant={"small"}>Publisher: {values.Publisher}</Text>
                            </p>
                            <hr className="seperator" />
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
    </div>
  );
}
export default Recommendation;