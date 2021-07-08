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
import Recommendation from './Recommendation'
import { IApplicationTags } from './IApplicationTags';


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
  const [subName, setSubName] = React.useState<string>('');
  const [hideAddSubscriptionDialog, setHideAddSubscriptionDialog] = React.useState(true);

  const [applicationData, setApplicationData] = React.useState<IApplication>({
    UniqueName: "",
    DisplayName: "",
    Description: "",
    LogoImageUrl: "",
    DocumentationUrl: "",
    Publisher: "",
    Tags: [],
    Details: {},
    type: '',
    isSubScribed: false
  });

  const labelId: string = 'dialogLabel';
  const subTextId: string = 'subTextLabel';

  const [applicationSubscriptions, setApplicationSubscriptions] = React.useState<IApplicationSubscription[]>([]);
  const [swaggerUrl, setSwaggerUrl] = React.useState<string>();

  const [selectedApplication, setselectedApplication] = React.useState<string | null>(sessionStorage.getItem('selectedApplication'));

  let sa = sessionStorage.getItem('selectedApplicationObject') != null ? sessionStorage.getItem('selectedApplicationObject') : null
  const [selectedApplicationObject, setselectedApplicationObject] = React.useState<IApplication>();
  const [selectedApplicationObjectloading, setselectedApplicationObjectLoading] = React.useState<boolean>();

  const [selectedTab, setSelectedTab] = React.useState<string>();

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
      <Stack horizontal style={{ display: 'block', textAlign: 'right' }}>
        {/* <PrimaryButton style={{ marginLeft:'100px' }} text="Save" ></PrimaryButton>       */}
        <div>
          <DefaultButton onClick={() => { toggleOwnerPanel(false); openPanel() }}>Close</DefaultButton>
        </div>
      </Stack>
    ),
    [dismissPanel],
  );

  const dialogAddSubContentProps: IDialogContentProps = {
    type: DialogType.normal,
    title: 'Add New Subscription',
    closeButtonAriaLabel: 'Cancel',
    isMultiline: true
};

const modalProps: IModalProps = {
  titleAriaId: labelId,
  subtitleAriaId: subTextId,
  isBlocking: true,
  isDarkOverlay: true,
  allowTouchBodyScroll: true,
}

const toggleAddSubDialog = () => {
  if (hideAddSubscriptionDialog) {
      setHideAddSubscriptionDialog(false);
  } else {
      setHideAddSubscriptionDialog(true);
  }
}
  const getApplicationDetails = (subscribed:boolean) => {
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
      .then(_data => {
        _data.isSubScribed = subscribed;
        setApplicationData(_data);
        loadAPIList(_data); 
      });

  }
  const addSubscription = () => {
    fetch(`${window.BASE_URL}/gallery/applications/${selectedApplication}/subscriptions/` + subName, {
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
    let selectedkey = item.props.itemKey ? item.props.itemKey.toString() : '';
    setSelectedTab(selectedkey);
  }
  const togglePanel = (value: boolean) => {
    toggleOwnerPanel(value);
  }

  const closePanel = () => {
    dismissPanel();
  }
  const setSelectedSubscription = (selectedSubscriptionName: string) => {
    var subscriptionData = applicationSubscriptions.filter((e) => e.SubscriptionName === selectedSubscriptionName);
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
      .then(_data => {
        debugger;
        let subscribed = false;
        if (_data != null && _data.length > 0) {
          subscribed = true;
        }
        getApplicationDetails(subscribed);
        setApplicationSubscriptions(_data) 
      });
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

  const reloadLeftSection = () => {    
    let appdata = sessionStorage.getItem('selectedApplicationObject') != null ? sessionStorage.getItem('selectedApplicationObject') : null
    if (appdata != null) {
      setselectedApplicationObjectLoading(true);
      let selectedApp = JSON.parse(appdata) as IApplication;
      setselectedApplicationObject(selectedApp);
      setselectedApplicationObjectLoading(false);
    }
  }

  React.useEffect(() => {
    if (sa != null) {
      setselectedApplicationObjectLoading(true);
      let selectedApp = JSON.parse(sa) as IApplication;
      setselectedApplicationObject(selectedApp);
      setselectedApplicationObjectLoading(false);
    }
    setSelectedTab('SampleCode');
    loadLanguagesList();
    getApplicationSubscriptions();
    loadSwaggerData();
  }, []);

  const viewMySubscription = () => {
    setSelectedTab('MySubscriptions');
  }

  const SelectSwaggerTab = () => {
    setSelectedTab('Swagger');
  }

  return (
    <div className="AIServices">
      <div style={PanelStyles}>
        <StackItem className="divWidth25 leftsection">
          <div className="contentsection">
            {
              selectedApplicationObjectloading ? 'loading...' :
                <React.Fragment>
                  <Text block variant={'xLargePlus'}>{selectedApplicationObject?.DisplayName}</Text>
                  <Text block variant={'medium'} style={{ marginTop: '10px', color: 'grey' }}>Publisher: {selectedApplicationObject?.Publisher}</Text>

                  <Text block variant={'medium'} style={{ marginTop: '20px', fontWeight: 'bold' }}>Description</Text>
                  <Text block variant={'medium'} style={{ marginTop: '5px' }}>{selectedApplicationObject?.Description}</Text>
                  <Text block variant={'medium'} style={{ marginTop: '20px', fontWeight: 'bold', marginBottom: '20px' }}>Tags</Text>
                  <div style={{ display: 'flex', 'flexDirection': 'row' }}>
                    {
                      selectedApplicationObject?.Tags.map((tagvalues: IApplicationTags, tagidx: any) => {
                        return (
                          <div key={tagidx} style={{ fontWeight: 400, border: '1px solid black', marginRight: '10px', padding: '0px 5px', borderRadius: '5px' }}>{tagvalues.name}</div>
                        )
                      })
                    }
                  </div>
                  {
                    applicationData?.isSubScribed ?
                      <React.Fragment>
                        <Text block variant={'medium'} style={{ marginTop: '20px', fontWeight: 'bold' }}>You own this application already.</Text>
                        <br />
                        <Text block variant={'medium'} style={{ marginTop: '5px', color: 'blue', borderBottom: '1px solid blue', width: 'fit-content', cursor: 'pointer' }}
                          onClick={viewMySubscription}>View my subscriptions</Text>
                      </React.Fragment>
                      : <React.Fragment>
                        <Text block variant={'medium'} style={{ marginTop: '20px', fontWeight: 'bold' }}>You haven't subscribed this application yet.</Text>
                        <Text block variant={'medium'} style={{ marginTop: '5px', color: 'blue', borderBottom: '1px solid blue', width: 'fit-content', cursor: 'pointer' }} onClick={()=> setHideAddSubscriptionDialog(false)}>+ Subscribe Now</Text>
                      </React.Fragment>
                  }
                </React.Fragment>
            }
          </div>
          <div className="leftfooter">
            <Text>Documentation</Text><IconButton text="Documentation" iconProps={{ iconName: "OpenInNewWindow" }} target="_blank" href={selectedApplicationObject?.DocumentationUrl} /><br />
            <Text>About ACE team</Text><IconButton text="About ACE team" iconProps={{ iconName: "OpenInNewWindow" }} target="" /> <br />
            <Text>About Luna.ai</Text><IconButton text="About Luna.ai" iconProps={{ iconName: "OpenInNewWindow" }} target="" /> <br />
          </div>
        </StackItem>
        <StackItem className="divWidth75 rightsection">
          <StackItem>
            <div style={{ width: '100%', height: '770px' }}>
              <Pivot onLinkClick={(item?: PivotItem) => loadTabData(item!)} selectedKey={selectedTab} className="pivottab">
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
                    <Text>Swagger</Text><IconButton text="Swagger" iconProps={{ iconName: "OpenInNewWindow" }} target="" onClick={SelectSwaggerTab} /> <br />
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
                                    <Link onClick={() => setSelectedSubscription(values.SubscriptionName)} >{values.SubscriptionName}</Link>
                                  </td>
                                  <td>
                                    {values.SubscriptionId}
                                  </td>
                                  <td>
                                    {values.CreatedTime.substr(0, 10)}
                                  </td>
                                </tr>
                              )
                            })
                          }
                        </tbody>
                      </table>
                      <Link onClick={()=> setHideAddSubscriptionDialog(false)}>+ New</Link>
                      <div style={{ display: hideAddNewSub ? 'none' : 'block', width: '250px', border: '1px solid black', padding: '10px' }}>
                        <TextField label={"Subscription Name:"} value={subName} onChange={setSubNameValue}></TextField>
                        <PrimaryButton style={{ marginTop: '5px' }} onClick={() => { addSubscription(); setSubName(''); setHideAddNewSub(true) }}>Submit</PrimaryButton>
                      </div>
                    </div>
                  </React.Fragment>
                </PivotItem>
                <PivotItem headerText={"Swagger"} itemKey={"Swagger"} >
                  <SwaggerUI spec={swaggerUrl} />
                </PivotItem>
                <PivotItem headerText={"Recommendations"} itemKey={"Recommendations"}>
                  <Recommendation reloadLeftSection = {reloadLeftSection}/>
                </PivotItem>
                <PivotItem headerText={"Reviews"} itemKey={"Reviews"}>

                </PivotItem>
              </Pivot>
            </div>
          </StackItem>
        </StackItem>

      </div>
      <br />
      <FooterLinks />
      <Panel
        headerText={subscriptionData?.SubscriptionName}
        isOpen={isOpen}
        isFooterAtBottom={true}
        hasCloseButton={false}
        type={PanelType.custom}
        customWidth={"400px"}
        isBlocking={true}
      >
        <MySubscriptionDetails deleteCallback={() => {closePanel();getApplicationSubscriptions();}} toggle={togglePanel} closePanel={closePanel} subscription={subscriptionData} />
      </Panel>
      <Panel
        headerText="Subscription Owners"
        isOpen={isOwnerPanelOpen}
        onDismiss={() => toggleOwnerPanel(false)}
        onRenderFooterContent={onRenderFooterContent}
        closeButtonAriaLabel="Close"
        isFooterAtBottom={true}
        hasCloseButton={false}
        type={PanelType.custom}
        customWidth={"400px"}
      >
        <MySubscriptionOwnersDetails subscription={subscriptionData} />
      </Panel>
      <Dialog
        hidden={hideAddSubscriptionDialog}
        onDismiss={toggleAddSubDialog}
        dialogContentProps={dialogAddSubContentProps}
        modalProps={modalProps}
            >
                 {/* <div style={{ width: '250px', border: '1px solid black', padding: '10px' }}> */}
                        <TextField label={"Subscription Name:"} value={subName} onChange={setSubNameValue}></TextField>                        
                      {/* </div> */}
                <DialogFooter>
                    <PrimaryButton
                        text="Submit"
                        onClick={() => { addSubscription(); setSubName(''); toggleAddSubDialog(); viewMySubscription(); reloadLeftSection(); }} />
                    <DefaultButton onClick={toggleAddSubDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
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
