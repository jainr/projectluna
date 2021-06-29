// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import SyntaxHighlighter from "react-syntax-highlighter";
import { vs } from "react-syntax-highlighter/dist/esm/styles/hljs";
import mytext from './SampleCode';
import FooterLinks from '../FooterLinks/FooterLinks';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton, PanelType } from '@fluentui/react';
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
import MySubscriptionDetails from './MySubscriptionDetails';
import MySubscriptionOwnersDetails from './MySubscriptionOwnersDetails';
import SwaggerUI from "swagger-ui-react";
import "swagger-ui-react/swagger-ui.css";


const theme = getTheme();

const AIServiceDetails = () => {
  const history = useHistory();
  const [offerData, setOfferData] = React.useState<any[]>();
  const [initOffferData, setInitOfferData] = React.useState<any[]>();
  const [isOpen, { setTrue: openPanel, setFalse: dismissPanel }] = useBoolean(false);
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
  const [isOwnerPanelOpen, toggleOwnerPanel] = React.useState(false);
  const [subscriptionData, setSubscriptionData] = React.useState<IApplicationSubscription>();
  const [hideAddNewSub, setHideAddNewSub] = React.useState(true);
  const [subName,setSubName] = React.useState<string>('');

  const [applicationData, setApplicationData] = React.useState<IApplication>({
    UniqueName: "",
    DisplayName: "",
    Description: "",
    LogoImageUrl: "",
    DocumentationUrl: "",
    Publisher: "",
    Tags: [],
    Details: {},
    type:'',
    isSubScribed:false
  });

  // sessionStorage.setItem('selectedApplication','lunanlp');

  const [applicationSubscriptions, setApplicationSubscriptions] = React.useState<IApplicationSubscription[]>([]);
  const [swaggerUrl, setSwaggerUrl] = React.useState<string>();

  const [selectedApplication, setselectedApplication] = React.useState<string | null>(sessionStorage.getItem('selectedApplication'));
  
  const loadLanguagesList = () => {

    languageOptions.push({ "key": "Python", "text": "REST API - Python" });
    setLanguageOptions(languageOptions);
  }

  const loadAPIList = (applicationData: IApplication) => {
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

  const onRenderFooterContent = React.useCallback(
    () => (
      <Stack horizontal style={{display:'block',textAlign:'right'}}>
      {/* <PrimaryButton style={{ marginLeft:'100px' }} text="Save" ></PrimaryButton>       */}
      <div>        
        <DefaultButton onClick={() => { toggleOwnerPanel(false); openPanel() }}>Cancel</DefaultButton>     
      </div>
      </Stack>            
    ),
    [dismissPanel],
  );

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
  const addSubscription = () => {
    fetch(`${window.BASE_URL}/gallery/applications/${selectedApplication}/subscriptions/`+ subName, {
      mode: "cors",
      method: "PUT",
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Luna-User-Id': 'test-admin',
        'Host': 'lunatest-gateway.azurewebsites.net'
      },
    })
      .then(response => response.json())
      .then(_data => getApplicationSubscriptions());      

  }
  const loadTabData = (item: PivotItem) => {
    if (item.props.itemKey === 'My Subscriptions') {
      //  getApplicationSubscriptions();
    }
  }
  const togglePanel = (value: boolean) => {
    toggleOwnerPanel(value);
  }

  const closePanel=()=>
  {
    dismissPanel();
  }
  const setSelectedSubscription = (selectedSubscriptionName: string)=>
  {
    var subscriptionData = applicationSubscriptions.filter((e)=>e.SubscriptionName === selectedSubscriptionName);
    setSubscriptionData(subscriptionData[0]);
    openPanel();
  }

  const setSubNameValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
    setSubName(newValue || '');
}
  const getApplicationSubscriptions = () => {

    fetch(`${window.BASE_URL}/gallery/applications/${selectedApplication}/subscriptions`, {
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
      .then(_data => { setApplicationSubscriptions(_data) });
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
    .then(async _data => {setSwaggerUrl(_data);
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
                  <PivotItem headerText={"Sample Code"} itemKey={"SampleCode"}>
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
                        <tbody>
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
                        </tbody>
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
                  <PivotItem headerText={"My Subscriptions"} itemKey={"MySubscriptions"}>
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
                                      <Link onClick={()=>setSelectedSubscription(values.SubscriptionName)} >{values.SubscriptionName}</Link>
                                    </td>
                                    <td>
                                      {values.SubscriptionId}
                                    </td>
                                    <td>
                                      {values.CreatedTime.substr(0,10)}
                                    </td>
                                  </tr>
                                )
                              })
                            }
                          </tbody>
                        </table>
                        <Link onClick={()=> setHideAddNewSub(false)}>+ New</Link>
                        <div style={{ display : hideAddNewSub ? 'none' : 'block', width:'250px',border:'1px solid black',padding:'10px'}}>                          
                          <TextField label={"Subscription Name:"} value={subName} onChange={setSubNameValue}></TextField>
                          <PrimaryButton style={{marginTop:'5px'}} onClick={()=>{addSubscription(); setSubName('')}}>Submit</PrimaryButton>
                        </div>
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
      <br />
      <FooterLinks />
      <Panel
        headerText="My Subscription"
        isOpen={isOpen}        
        isFooterAtBottom={true}        
        hasCloseButton={false}
        type={PanelType.custom}
        customWidth={"400px"}
        isBlocking={true}
      >
        <MySubscriptionDetails toggle={togglePanel} closePanel={closePanel} subscription={subscriptionData} />
      </Panel>
      <Panel
        headerText="Subscription Owners"
        isOpen={isOwnerPanelOpen}
        onDismiss={() => toggleOwnerPanel(false)}
        onRenderFooterContent={onRenderFooterContent}                            
        closeButtonAriaLabel="Close"
        isFooterAtBottom={true}
        hasCloseButton={false}
      >
        <MySubscriptionOwnersDetails subscription={subscriptionData} />        
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
