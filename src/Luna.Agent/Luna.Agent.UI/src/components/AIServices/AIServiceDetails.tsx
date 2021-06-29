// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import SyntaxHighlighter from "react-syntax-highlighter";
import { vs } from "react-syntax-highlighter/dist/esm/styles/hljs";
import mytext from './SampleCode';
import FooterLinks from '../FooterLinks/FooterLinks';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton } from '@fluentui/react';
import { PanelStyles } from '../../helpers/PanelStyles';
import { getTheme } from '@fluentui/react';
import { SharedColors } from '@uifabric/fluent-theme';
import { GetMarketplaceOffers } from './GetMarketplaceOffers';
import { useBoolean } from '@uifabric/react-hooks';
import { ISubscription } from '../../interfaces/ISubscription';

import './../AIServices/AIServices.css';
import { getTypeParameterOwner } from 'typescript';
import { PromptState } from 'msal/lib-commonjs/utils/Constants';
import { withRouter } from "react-router-dom";
import { useHistory, useLocation } from 'react-router';
import { GetApplicationDetails } from './GetApplicationDetails';
import { IApplication } from './IApplication';
import { IApplicationSubscription } from './IApplicationSubscription';
import SwaggerUI from "swagger-ui-react"
import "swagger-ui-react/swagger-ui.css"

const theme = getTheme();

const AIServiceDetails = () => {
  const history = useHistory();
  const [offerData, setOfferData] = React.useState<any[]>();
  const [initOffferData, setInitOfferData] = React.useState<any[]>();
  const [isOpen, { setTrue: openPanel, setFalse: dismissPanel }] = useBoolean(false);
  const [isNewOpen, { setTrue: openNewPanel, setFalse: dismissNewPanel }] = useBoolean(false);
  const [isNewSubSuccessful, setIsNewSubSuccessful] = React.useState(false);
  const [isDataLoading, setIsDataLoading] = React.useState(true);
  const [planOptions, setPlanOptions] = React.useState<IDropdownOption[]>([]);
  const [hideNewSubDialog, setHideNewSubDialog] = React.useState(true);
  const [languageOptions, setLanguageOptions] = React.useState<IDropdownOption[]>([]);
  const [aPIOptions, setAPIOptions] = React.useState<IDropdownOption[]>([]);
  const [aPIVersionOptions, setAPIVersionOptions] = React.useState<IDropdownOption[]>([]);
  const [operationOptions, setOperationOptions] = React.useState<IDropdownOption[]>([]);
  const [isExpand, setIsExpand] = React.useState(true);
  const [applicationDetail, setApplicationDetail] = React.useState<{}>();
  const [selectedValues, setSelectedValues] = React.useState<ISelectedItems>();
  const [loadingSubscription, setLoadingSubscription] = React.useState<boolean>(true);

  const [applicationData, setApplicationData] = React.useState<IApplication>({
    UniqueName: "",
    DisplayName: "",
    Description: "",
    LogoImageUrl: "",
    DocumentationUrl: "",
    Publisher: "",
    Tags: [],
    Details: {}
  });

  sessionStorage.setItem('selectedApplication','lunanlp');

  const [applicationSubscriptions, setApplicationSubscriptions] = React.useState<IApplicationSubscription[]>([]);
  const [swaggerUrl, setSwaggerUrl] = React.useState<string>();

  const [selectedApplication, setselectedApplication] = React.useState<string | null>(sessionStorage.getItem('selectedApplication'));
  
  const loadLanguagesList = () => {

    languageOptions.push({ "key": "Python", "text": "REST API - Python" });
    setLanguageOptions(languageOptions);
  }

  const loadAPIList = (applicationData: IApplication) => {

    // aPIOptions.push({"key":"French","text":"French"});
    applicationData.Details.apIs?.forEach((api: any) =>
      aPIOptions.push({ "key": api.name, 'text': api.name })
    );
    setAPIOptions(aPIOptions);
  }

  const loadAPIVersionList = (selectedAPI: string) => {
    var apiObj = applicationData.Details.apIs?.find(obj => {
      return obj.name === selectedAPI
    })
    let apiVersionOptions: IDropdownOption[] = [];
    apiObj?.versions?.forEach((version: any) =>
      apiVersionOptions.push({ "key": version.name, 'text': version.name })
    );
    setAPIVersionOptions(apiVersionOptions);
  }

  const loadOperationList = (selectedAPI: string, selectedVersion: string) => {
    var apiObj = applicationData.Details.apIs?.find(obj => {
      return obj.name === selectedAPI
    })
    var versionObj = apiObj?.versions.find(obj => {
      return obj.name === selectedVersion
    });

    let apiVersionOperationOptions: IDropdownOption[] = [];
    versionObj?.operations?.forEach((operation: any) =>
      apiVersionOperationOptions.push({ "key": operation, 'text': operation })
    );
    setOperationOptions(apiVersionOperationOptions);
  }

  const ExpandCollapseClick = () => {
    setIsExpand(!isExpand);
  }

  const getApplicationDetails = () => {
    fetch(`${window.BASE_URL}/gallery/applications/${selectedApplication}`, {
      mode: "cors",
      method: "GET",
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Luna-User-Id': 'test-admin',
        'Host': 'lunatest-gateway.azurewebsites.net'
      },
    })
      .then(response => response.json())
      .then(_data => { setApplicationData(_data); loadAPIList(_data); });

  }
  const loadTabData = (item: PivotItem) => {
    if (item.props.itemKey === 'My Subscriptions') {
      // getApplicationSubscriptions();
    }
  }
  const getApplicationSubscriptions = () => {

    //   fetch(`${window.BASE_URL}/gallery/applications/lunanlp/subscriptions`, {
    //     mode: "cors",
    //     method: "GET",
    //     headers: {         
    //         'Accept': 'application/json',
    //         'Content-Type': 'application/json',
    //         'Luna-User-Id': 'test-admin',
    //         'Host': 'lunatest-gateway.azurewebsites.net'         
    //     },
    // })
    // .then(response => response.json())
    // .then(_data => { setApplicationSubscriptions(_data)});
    const data = {
      subscriptionId: '123',
      baseUrl: "lcjnadlcnadljc",
      createdTime: "22-06-2021",
      primaryKey: "kackbcleleakc",
      secondaryKey: "acbkcdicbdkbc",
      notes: "notes",
      subscriptionName: "mysub",
      owner: [{
        userId: '1',
        userName: 'User'
      }]
    }
    // setApplicationSubscriptions(data);
    applicationSubscriptions.push(data);
  }

  const loadSwaggerData = () => {
    fetch(`${window.BASE_URL}/gallery/applications/${selectedApplication}/swagger`, {
      mode: "cors",
      method: "GET",
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Luna-User-Id': 'test-admin',
        'Host': 'lunatest-gateway.azurewebsites.net'
      },
    })
      .then(response => response.json())
      .then(async _data => {
        setSwaggerUrl(_data);
      });
  }

  React.useEffect(() => {    
    getApplicationDetails();
    loadLanguagesList();
    getApplicationSubscriptions();
    loadSwaggerData();
  }, []);

  return (
    <div className="AIServices">
      <div style={PanelStyles}>
        <Text block variant={'xLargePlus'}>AI Service: Text Summarization</Text>
        <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
          <StackItem className="divWidth25">
            <Text block variant={'medium'} style={{ marginTop: '10px', color: 'grey' }}>Publisher: ACE Team</Text>
            <div style={{ paddingRight: '10px', borderRight: '1px solid black' }}>
              <Text block variant={'medium'} style={{ marginTop: '20px', fontWeight: 'bold' }}>Description</Text>
              <Text block variant={'medium'} style={{ marginTop: '5px' }}>Summarize text documents and articles using machine learning. This model helps you create summary from meeting notes, news and other articles…</Text>
              <Text block variant={'medium'} style={{ marginTop: '20px', fontWeight: 'bold', marginBottom: '20px' }}>Tags</Text>
              <div style={{ display: 'flex', 'flexDirection': 'row' }}>
                <div style={{ fontWeight: 400, border: '1px solid black', marginRight: '10px', padding: '0px 5px', borderRadius: '5px', width: '15%' }}>SaaS</div>
                <div style={{ fontWeight: 400, border: '1px solid black', marginRight: '10px', padding: '0px 5px', borderRadius: '5px', width: '15%' }}>NLP</div>
                <div style={{ fontWeight: 400, border: '1px solid black', marginRight: '10px', padding: '5px 5px', borderRadius: '5px', width: '140px' }}>Publisher: ACE Team</div>
              </div>
              <Text block variant={'medium'} style={{ marginTop: '20px', fontWeight: 'bold' }}>You haven't subscribed this application yet.</Text>
              <Text block variant={'medium'} style={{ marginTop: '5px', color: 'blue', borderBottom: '1px solid blue', width: 'fit-content', cursor: 'pointer' }}>+ Subscribe Now</Text>
            </div>
          </StackItem>
          <StackItem className="divWidth75">
            <StackItem>
              <div style={{ width: '700px', height: '370px' }}>
                <Pivot onLinkClick={(item?: PivotItem) => loadTabData(item!)}>
                  <PivotItem headerText={"Sample Code"} itemKey={"Sample Code"}>
                    <div style={{ display: 'flex' }} >
                      <Label style={{ margin: '10px' }}>Language:</Label>
                      <Dropdown options={languageOptions}
                        placeholder={"Select a Language"}
                        style={{ margin: '10px' }}
                      />
                    </div>
                    <div style={{ ...isExpand ? { display: 'none' } : { display: 'block' }, margin: '10px' }}>
                      Advance Settings <IconButton iconProps={{ iconName: 'ChevronUpMed' }} title="Collapse" ariaLabel="Collapse" onClick={ExpandCollapseClick} />
                      <table style={{ margin: '10px' }}>
                        <tr>
                          <td>
                            <Label>API:</Label>
                          </td>
                          <td>
                            <Dropdown options={aPIOptions}
                              placeholder={"Select an API"}
                              style={{ margin: '10px', width: '170px' }}
                              onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                                setSelectedValues({
                                  application: "",
                                  api: option?.text!,
                                  version: "",
                                  operation: "",
                                });
                                loadAPIVersionList(option?.text!);
                              }}
                            />
                          </td>
                          <td>
                            <Label>API Version:</Label>
                          </td>
                          <td>
                            <Dropdown options={aPIVersionOptions}
                              placeholder={"Select an API Version"}
                              style={{ margin: '10px', width: '170px' }}
                              selectedKey={selectedValues?.version}
                              onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                                setSelectedValues({
                                  application: "",
                                  api: selectedValues?.api!,
                                  version: option?.text!,
                                  operation: "",
                                });
                                loadOperationList(selectedValues?.api!, option?.text!);
                              }}
                            />
                          </td>
                        </tr>
                        <tr>
                          <td>
                            <Label>Operation:</Label>
                          </td>
                          <td>
                            <Dropdown options={operationOptions}
                              placeholder={"Select an Operation"}
                              style={{ margin: '10px', width: '170px' }}
                              selectedKey={selectedValues?.operation}
                              onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                                setSelectedValues({
                                  application: "",
                                  api: selectedValues?.api!,
                                  version: selectedValues?.version!,
                                  operation: option?.text!,
                                });
                              }}
                            />
                          </td>
                        </tr>
                      </table>
                    </div>
                    <div style={{ ...isExpand ? { display: 'block' } : { display: 'none' }, margin: '10px' }}> Advance Settings <IconButton iconProps={{ iconName: 'ChevronDownMed' }} title="Expand" ariaLabel="Expand" onClick={ExpandCollapseClick} />
                    </div>
                    <div style={{ textAlign: 'right', width: '80%' }}>
                      <a href="https://aka.ms/lunasynapsenotebook" target="new"><u>Open in Synapse Notebook</u></a>
                    </div>
                    <div style={{ margin: '10px', boxShadow: '0px 0px 10px 4px #888888', width: '80%' }}>
                      <SyntaxHighlighter language="typescript" style={vs}>
                        {mytext}
                      </SyntaxHighlighter>
                    </div>
                    <div style={{ color: 'blue', margin: '10px' }}>
                      <Text>More Sample Code</Text><IconButton text="More Sample Code" iconProps={{ iconName: "OpenInNewWindow" }} target="" /><br />
                      <Text>Swagger</Text><IconButton text="Swagger" iconProps={{ iconName: "OpenInNewWindow" }} target="" /> <br />
                    </div>
                  </PivotItem>
                  <PivotItem headerText={"My Subscriptions"} itemKey={"My Subscriptions"}>
                    <React.Fragment>
                      <div style={{ margin: '10px' }}>
                        <table style={{ width: '90%', borderCollapse: 'collapse' }}>
                          <thead>
                            <tr style={{ borderBottom: '1px solid black', borderTop: '1px solid black' }}>
                              <th style={{ width: '30%', padding: 0 }}>
                                <Label title={"Subscription Name"} > Subscription Name</Label>
                              </th>
                              <th style={{ width: '50%', padding: 0 }}>
                                <Label title={"Subscription Id"} >Subscription Id</Label>
                              </th>
                              <th style={{ width: '20%%', padding: 0 }}>
                                <Label title={"Created Date"} >Created Date</Label>
                              </th>
                            </tr>
                          </thead>
                          <tbody>
                            {
                              applicationSubscriptions?.map((values, idx) => {
                                return (
                                  <tr key={idx} style={{ lineHeight: '30px', textAlign: 'center' }}>
                                    <td>
                                      <Link onClick={openPanel} >{values.subscriptionName}</Link>
                                    </td>
                                    <td>
                                      {values.subscriptionId}
                                    </td>
                                    <td>
                                      {values.createdTime}
                                    </td>
                                  </tr>
                                )
                              })
                            }
                          </tbody>
                        </table>
                      </div>
                    </React.Fragment>
                  </PivotItem>
                  <PivotItem headerText={"Swagger"} itemKey={"Swagger"} >
                    <SwaggerUI spec={swaggerUrl} />                    
                  </PivotItem>
                  <PivotItem headerText={"Recommendations"} itemKey={"Recommendations"}>

                  </PivotItem>
                  <PivotItem headerText={"Reviews"} itemKey={"Reviews"}>

                  </PivotItem>
                </Pivot>
              </div>
            </StackItem>
          </StackItem>
        </Stack>
      </div>            
      <FooterLinks />
      <Panel
        headerText="Sample panel"
        isOpen={isOpen}
        onDismiss={dismissPanel}
        // You MUST provide this prop! Otherwise screen readers will just say "button" with no label.
        closeButtonAriaLabel="Close"
      >
        <p>Content goes here.</p>
        <Link >Open more panel</Link>
      </Panel>
      <Panel
        headerText="Sample panel"
        isOpen={isNewOpen}
        onDismiss={dismissNewPanel}
        // You MUST provide this prop! Otherwise screen readers will just say "button" with no label.
        closeButtonAriaLabel="Close"
      >
        <p>Content goes here.</p>
        <Link onClick={openNewPanel}>Open more panel</Link>
      </Panel>
    </div>
  );
}

interface ISelectedItems {
  application: string;
  api: string;
  version: string;
  operation: string;
}
export default AIServiceDetails;
