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
import {IApplication} from './IApplication';

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

  const loadLanguagesList = () => {
   
    languageOptions.push({"key":"Python","text":"REST API - Python"});
    setLanguageOptions(languageOptions);
  }

  const loadAPIList = (applicationData:IApplication) => {
   
    // aPIOptions.push({"key":"French","text":"French"});
    applicationData.Details.apIs?.forEach((api: any )  => 
      aPIOptions.push({"key" : api.name, 'text': api.name }) 
    );    
    setAPIOptions(aPIOptions);
  }

  const loadAPIVersionList = (selectedAPI: string) => {
    var apiObj = applicationData.Details.apIs?.find(obj => {
      return obj.name === selectedAPI
    })
    let apiVersionOptions: IDropdownOption[] = [];
    apiObj?.versions?.forEach((version: any )  => 
    apiVersionOptions.push({"key" : version.name, 'text': version.name }) 
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
    versionObj?.operations?.forEach((operation: any )  => 
    apiVersionOperationOptions.push({"key" : operation, 'text': operation }) 
    );    
    setOperationOptions(apiVersionOperationOptions);
  }

  const ExpandCollapseClick = () => {
    setIsExpand(!isExpand);
  }

  const getApplicationDetails = () => 
  {
    fetch(`${window.BASE_URL}/gallery/applications/lunanlp`, {
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
  .then(_data => { setApplicationData(_data); loadAPIList(_data);});
    
  }
  React.useEffect(() => {
      getApplicationDetails();
      loadLanguagesList();          
  }, []);

  return (
    <div className="AIServices">
      <div style={PanelStyles}>
      <Text block variant={'xLargePlus'}>AI Service: Text Summarization</Text>
        <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
          <StackItem>
            <Text block variant={'medium'} style={{marginTop: '10px', color: 'grey' }}>Publisher: ACE Team</Text>
            <div style={{paddingRight: '10px', borderRight: '1px solid black'}}>
              <Text block variant={'medium'} style={{marginTop: '20px', fontWeight: 'bold' }}>Description</Text>
              <Text block variant={'medium'} style={{marginTop: '5px' }}>Summarize text documents and articles using machine learning. This model helps you create summary from meeting notes, news and other articles…</Text>
              <Text block variant={'medium'} style={{marginTop: '20px', fontWeight: 'bold', marginBottom: '20px' }}>Tags</Text>
              <div style={{display: 'flex', 'flexDirection': 'row'}}>
                <div  style={{fontWeight: 400, border: '1px solid black', marginRight: '10px', padding: '0px 5px', borderRadius: '5px',width:'15%' }}>SaaS</div>
                <div  style={{fontWeight: 400, border: '1px solid black', marginRight: '10px', padding: '0px 5px', borderRadius: '5px',width:'15%' }}>NLP</div>
                <div style={{fontWeight: 400, border: '1px solid black', marginRight: '10px', padding: '5px 5px', borderRadius: '5px',width:'140px' }}>Publisher: ACE Team</div>
              </div>
              <Text block variant={'medium'} style={{marginTop: '20px', fontWeight: 'bold' }}>You haven't subscribed this application yet.</Text>
              <Text block variant={'medium'} style={{marginTop: '5px', color: 'blue', borderBottom: '1px solid blue', width: 'fit-content', cursor: 'pointer'}}>+ Subscribe Now</Text>
            </div>
          </StackItem>
          <StackItem>
            <StackItem>
              <div style={{width:'700px', height:'370px'}}>
                <Pivot>
                  <PivotItem headerText={"Sample Code"} >
                    <div style={{display:'flex'}} >
                    <Label style={{margin:'10px'}}>Language:</Label>
                      <Dropdown options={languageOptions}
                      placeholder={"Select a Language"}
                      style={{margin:'10px'}}
                      />                                          
                    </div>                    
                    <div style={{ ...isExpand ? { display: 'none' } : { display: 'block' }, margin:'10px' }}>
                          Advance Settings <IconButton iconProps={{ iconName: 'ChevronUpMed' }} title="Collapse" ariaLabel="Collapse" onClick={ExpandCollapseClick} /> 
                          <table style={{margin:'10px' }}>
                            <tr>
                              <td>
                              <Label>API:</Label>
                              </td>
                              <td>
                              <Dropdown options={aPIOptions}
                                placeholder={"Select an API"}
                                style={{margin:'10px', width:'170px'}}
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
                                style={{margin:'10px', width:'170px'}}
                                onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                                  setSelectedValues({
                                    application: "",
                                    api: selectedValues?.api!,
                                    version: option?.text!,
                                    operation: "",
                                  });
                                  loadOperationList(selectedValues?.api!,option?.text!);                                  
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
                                style={{margin:'10px', width:'170px'}}
                              />                       
                              </td>
                            </tr>
                          </table>
                    </div>
                    <div style={{ ...isExpand ? { display: 'block' } : { display: 'none' }, margin:'10px'  }}> Advance Settings <IconButton iconProps={{ iconName: 'ChevronDownMed' }} title="Expand" ariaLabel="Expand" onClick={ExpandCollapseClick} />
                    </div>
                    <div style={{textAlign:'right',width:'80%'}}>
                      <a href="https://aka.ms/lunasynapsenotebook" target="new"><u>Open in Synapse Notebook</u></a>
                    </div>
                    <div style={{margin:'10px',boxShadow:'0px 0px 10px 4px #888888',width:'80%'}}>  
                  <SyntaxHighlighter language="typescript" style={vs}>
                      {mytext}
                  </SyntaxHighlighter>
                  </div>
                  <div style={{color:'blue',margin:'10px'}}>
                    <Text>More Sample Code</Text><IconButton text="More Sample Code" iconProps={{iconName:"OpenInNewWindow"}} target=""  /><br />
                    <Text>Swagger</Text><IconButton text="Swagger" iconProps={{iconName:"OpenInNewWindow"}} target=""  /> <br />
                  </div>
                  </PivotItem>
                  <PivotItem headerText={"My Subscriptions"} >

                </PivotItem>
                <PivotItem headerText={"Swagger"} >

                </PivotItem>
                <PivotItem headerText={"Recommendations"} >

                </PivotItem>
                <PivotItem headerText={"Reviews"} >

                </PivotItem>
                </Pivot>
              </div>
            {/* <Text block variant={'xLargePlus'}>AI Service: Text Summarization</Text>            */}
             </StackItem>           
          </StackItem>
        </Stack>
        </div>
        <br />
        <div style={{height:'500px'}}>
          <p>
      
           </p>
        </div>
      <FooterLinks />
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
