// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import { Text, TextField, StackItem, Stack, DefaultButton, CommandButton, Link, Icon, MessageBar, MessageBarType, Panel, PrimaryButton, DetailsList, SelectionMode, IColumn, Dialog, DialogFooter, DialogType, IModalProps, IDialogContentProps, Persona, PersonaSize, PersonaPresence, IconButton, IDropdownOption, Dropdown } from '@fluentui/react';
import { useParams } from 'react-router-dom';
import { SharedColors } from '@uifabric/fluent-theme';
import './SubscriptionDetail.css';
import { ISubscriptionDetail } from './ISubscriptionDetail';
import { ISubscriptionAPI } from './ISubscriptionAPI';
import FooterLinks from '../FooterLinks/FooterLinks';
import { getUserByEmail, getOtherUserPhoto } from '../../GraphService';
import { IUserLookUp } from '../../interfaces/IUserLookUp';
import { PanelStyles } from '../../helpers/PanelStyles';
import { CustomInfoAddUserLabel } from '../_subcomponents/CustomInfoAddUserLabel';
import { DeleteSubscription } from './SubscriptionDetail.DeleteSubscription';

/** @component Subscription Detail sub view. */
const SubscriptionDetail: React.FunctionComponent = () => {
  const params: AuthParams = useParams();
  const id = params.id;

  const [subscriptionData, setData] = React.useState<ISubscriptionDetail>({
    baseUrl: "",
    createdTime: "",
    offerName: "",
    planName: "",
    primaryKey: "",
    secondaryKey: "",
    status: "",
    subscriptionId: "",
    subscriptionName: "",
    owner: "",
    primaryKeySecretName: "",
    secondaryKeySecretName: "",
    applications: []
  });

  const [isCopySuccess, setSuccess] = React.useState<boolean>(false);
  const [isCopySuccess2, setSuccess2] = React.useState<boolean>(false);
  const [isCopySuccess3, setSuccess3] = React.useState<boolean>(false);
  const [isCopySuccessSampleCode, setSuccessSampleCode] = React.useState<boolean>(false);

  const [isOpen, setIsOpen] = React.useState(false);
  const [newUser, setNewUser] = React.useState("");
  const [applicationOptions, setApplicationOptions] = React.useState<IDropdownOption[]>([]);
  const [apiOptions, setApiOptions] = React.useState<IDropdownOption[]>([]);
  const [apiVersionOptions, setApiVersionOptions] = React.useState<IDropdownOption[]>([]);
  const [apiVersionOperationOptions, setApiVersionOperationOptions] = React.useState<IDropdownOption[]>([]);
  const [isApiDisabled, setIsApiDisabled] = React.useState<boolean>(true);
  const [isApiVersionDisabled, setIsApiVersionDisabled] = React.useState<boolean>(true);
  const [isApiVersionOperationDisabled, setIsApiVersionOperationDisabled] = React.useState<boolean>(true);
  const [selectedValues, setSelectedValues] = React.useState<ISelectedItems>();

  const [userValidation, setUserValidation] = React.useState<IUserLookUp | undefined>();
  const [noUserFound, setNoUserFound] = React.useState<boolean>();
  const [userValidationPhoto, setUserValidationPhoto] = React.useState<string | ArrayBuffer | null>();

  const [userToBeDeleted, setUserToBeDeleted] = React.useState('');
  const [apiType, setApiType] = React.useState('');
  const [hideDeleteDialog, setHideDeleteDialog] = React.useState(true);

  const openPanel = () => setIsOpen(true);
  const dismissPanel = () => setIsOpen(false);

  const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

  const loadData = () => {
    fetch(window.BASE_URL + '/subscriptions/' + params.id, {
      mode: "cors",
      method: "GET",
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.json())
      .then(_data => { setData(_data); setSubApplicationOptions(_data);});
  }

  const setSubApplicationOptions = (subscriptionData:ISubscriptionDetail) => {
    let subAppOptions: IDropdownOption[] = [];
    for (const key in subscriptionData.applications)
    {
      const app = subscriptionData.applications[key]
      subAppOptions.push({"key": app.name, "text": app.name})
    }
    setApplicationOptions(subAppOptions);
  }

  const setSubApiOptions = (selectedApp: string) => {

    if (selectedApp == ""){
      setIsApiDisabled(true);
      setIsApiVersionDisabled(true);
      setIsApiVersionOperationDisabled(true);
      return;
    }

    let subApiOptions: IDropdownOption[] = [];
    
    var appObj = subscriptionData.applications.find(obj => {
      return obj.name === selectedApp
    })

    if (appObj){
      for (const key in appObj.apIs)
      {
        const api = appObj.apIs[key]
        subApiOptions.push({"key": api.name, "text": api.name})
      }
      setApiOptions(subApiOptions);
      setIsApiDisabled(false);
    }
  }

  const setSubApiVersionOptions = (selectedApp: string, selectedApi:string) => {
    
    if (selectedApi == ""){
      setIsApiVersionDisabled(true);
      setIsApiVersionOperationDisabled(true);
      return;
    }

    var appObj = subscriptionData.applications.find(obj => {
      return obj.name === selectedApp
    })

    if (appObj){
      var apiObj = appObj.apIs.find(obj => {
        return obj.name === selectedApi
      })
  
      if (apiObj){
        setApiType(apiObj.type)
        let subApiVersionOptions: IDropdownOption[] = [];
    
        for (const key in apiObj.versions)
        {
          const version = apiObj.versions[key]
          subApiVersionOptions.push({"key": version?.name, "text": version?.name})
        }
        setApiVersionOptions(subApiVersionOptions);
        setIsApiVersionDisabled(false);
        setIsApiVersionOperationDisabled(true);
      }

    }
  }

  const setSubApiVersionOperationOptions = (selectedApp: string, selectedApi: string, selectedVersion: string) => {
    
    if (selectedApi == "" || selectedVersion == ""){
      setIsApiVersionOperationDisabled(true);
      return;
    }

    var appObj = subscriptionData.applications.find(obj => {
      return obj.name === selectedApp
    })

    if (appObj){
      var apiObj = appObj.apIs.find(obj => {
        return obj.name === selectedApi
      });
  
      if (apiObj){
  
        var versionObj = apiObj.versions.find(obj => {
          return obj.name === selectedVersion
        });
  
        if (versionObj){
          let subApiVersionOperationOptions: IDropdownOption[] = [];
      
          for (const key in versionObj.operations)
          {
            const operation = versionObj.operations[key]
            subApiVersionOperationOptions.push({"key": operation?.name, "text": operation?.displayName})
          }
          setApiVersionOperationOptions(subApiVersionOperationOptions);
          setIsApiVersionOperationDisabled(false);
        }
      }
    }
  }

  React.useEffect(() => {
    loadData();
  }, [id]);

  const renderLabel = (): JSX.Element => {
    const copyClick = () => {
      const urlValue = subscriptionData?.baseUrl;
      const selBox = document.createElement('textarea');
      selBox.style.position = 'fixed';
      selBox.style.left = '0';
      selBox.style.top = '0';
      selBox.style.opacity = '0';
      selBox.value = urlValue || "";
      document.body.appendChild(selBox);
      selBox.focus();
      selBox.select();
      document.execCommand('copy');
      document.body.removeChild(selBox);
      setSuccess(true);

      setTimeout(() => {
        setSuccess(false);
      }, 3000);
    }
    return (
      <Stack tokens={{ childrenGap: 10 }}>
        <Stack style={{ visibility: isCopySuccess ? "visible" : 'hidden' }}>
          <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
        </Stack>
        <Stack horizontal verticalAlign="baseline" horizontalAlign="space-between">
          <Text variant={'mediumPlus'}>Base URL</Text>

          <CommandButton onClick={copyClick} iconProps={{ iconName: 'Copy' }}>Copy</CommandButton>
        </Stack>
      </Stack>
    )
  }


  const renderLabel2 = (): JSX.Element => {
    const copyClick = () => {
      const urlValue = subscriptionData?.primaryKey;
      const selBox = document.createElement('textarea');
      selBox.style.position = 'fixed';
      selBox.style.left = '0';
      selBox.style.top = '0';
      selBox.style.opacity = '0';
      selBox.value = urlValue || "";
      document.body.appendChild(selBox);
      selBox.focus();
      selBox.select();
      document.execCommand('copy');
      document.body.removeChild(selBox);
      setSuccess2(true);

      setTimeout(() => {
        setSuccess2(false);
      }, 3000);
    }
    return (
      <Stack tokens={{ childrenGap: 10 }}>
        <Stack style={{ visibility: isCopySuccess2 ? "visible" : 'hidden' }}>
          <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
        </Stack>
        <Stack horizontal verticalAlign="baseline" horizontalAlign="space-between">
          <Text variant={'mediumPlus'}>Primary Key</Text>

          <CommandButton onClick={copyClick} iconProps={{ iconName: 'Copy' }}>Copy</CommandButton>
        </Stack>
      </Stack>
    )
  }


  const renderLabel3 = (): JSX.Element => {
    const copyClick = () => {
      const urlValue = subscriptionData?.secondaryKey;
      const selBox = document.createElement('textarea');
      selBox.style.position = 'fixed';
      selBox.style.left = '0';
      selBox.style.top = '0';
      selBox.style.opacity = '0';
      selBox.value = urlValue || "";
      document.body.appendChild(selBox);
      selBox.focus();
      selBox.select();
      document.execCommand('copy');
      document.body.removeChild(selBox);
      setSuccess3(true);

      setTimeout(() => {
        setSuccess3(false);
      }, 3000);
    }
    return (
      <Stack tokens={{ childrenGap: 10 }}>
        <Stack style={{ visibility: isCopySuccess3 ? "visible" : 'hidden' }}>
          <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
        </Stack>
        <Stack horizontal verticalAlign="baseline" horizontalAlign="space-between">
          <Text variant={'mediumPlus'}>Secondary Key</Text>

          <CommandButton onClick={copyClick} iconProps={{ iconName: 'Copy' }}>Copy</CommandButton>
        </Stack>
      </Stack>
    )
  }

  const sampleCode = "import os \n\
import requests \n\
import pandas as pd \n\
\n\
subscription_key = \"****************\" \n\
endpoint = \"<endpoint_url>\" \n\
df = pd.read_csv(<input_data_file>)\n\
url = endpoint + \"/apiv2/<app_name>/<api_name>/<operation_name>?api-version=<api_version>\"\n\
response = requests.post(url, headers={\"api-key\": subscription_key}, json=df.to_dict('split')) \n\
if response.status_code == 200: \n\
    print(response.json())";
  
  const [sampleCodeValue, setSampleCodeValue] = React.useState<string>(sampleCode);

  const renderLabelSampleCode = (): JSX.Element => {
    const copyClick = () => {
      const urlValue = sampleCodeValue.replace("****************", subscriptionData?.primaryKey)
        .replace("<endpoint_url>", subscriptionData?.baseUrl);
      const selBox = document.createElement('textarea');
      selBox.style.position = 'fixed';
      selBox.style.left = '0';
      selBox.style.top = '0';
      selBox.style.opacity = '0';
      selBox.value = urlValue || "";
      document.body.appendChild(selBox);
      selBox.focus();
      selBox.select();
      document.execCommand('copy');
      document.body.removeChild(selBox);
      setSuccessSampleCode(true);

      setTimeout(() => {
        setSuccessSampleCode(false);
      }, 3000);
    }
    return (
      <Stack tokens={{ childrenGap: 10 }}>
        <Stack style={{ visibility: isCopySuccessSampleCode ? "visible" : 'hidden' }}>
          <MessageBar messageBarType={MessageBarType.success}>Sample Code Copied!</MessageBar>
        </Stack>
        <Stack horizontal verticalAlign="baseline" horizontalAlign="space-between">
          <Text variant={'mediumPlus'}>Sample Code</Text>
          <CommandButton onClick={copyClick} iconProps={{ iconName: 'Copy' }}>Copy</CommandButton>
        </Stack>
      </Stack>
    )
  }

  const modalProps: IModalProps = {
    titleAriaId: 'titleId',
    subtitleAriaId: 'subtitleId',
    isBlocking: true,
    isDarkOverlay: true,
    allowTouchBodyScroll: true
  }

  const addUser = () => {
    return fetch(window.BASE_URL + '/subscriptions/' + id + '/users/' + userValidation?.userPrincipalName, {
      method: 'put',
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        "AADUserId": userValidation?.userPrincipalName,
        "Description": userValidation?.displayName,
        "Role": "User",
        "SubscriptionId": id,
        "ObjectId": userValidation?.id

      })
    })
      .then(response => {
        try {
          response.json();
          loadData();
          dismissPanel();
        } catch (error) {
          console.log(error);
          window.alert("Unable to add user.");
        }
      });
  };

  const verifyUser = async () => {
    try {
      const userInfo: IUserLookUp = await getUserByEmail(window.userToken, newUser);
      setUserValidation(userInfo);
      setNoUserFound(false);
      try {
        const lookupUserPhoto = await getOtherUserPhoto(window.userToken, userInfo.id);
        console.log(lookupUserPhoto);
        let fileReader = new FileReader();
        fileReader.onloadend = function (e) {
          if (e.target !== null) {
            let base64 = e.target.result;
            setUserValidationPhoto(base64);
          }
        }
        fileReader.readAsDataURL(lookupUserPhoto);

      } catch (errorPhoto) {
        console.info("No image found.");
      }
    } catch (error) {
      setUserValidation(undefined);
      setNoUserFound(true);
    }
  };

  const panelAddUserFooterUI = () => (
    <Stack horizontal tokens={{ childrenGap: 10 }}>
      <PrimaryButton
        disabled={userValidation?.userPrincipalName === undefined}
        onClick={addUser}>Add User</PrimaryButton>
      <DefaultButton onClick={dismissPanel}>Cancel</DefaultButton>
    </Stack>
  );



  const dialogDeleteUserContentProps: IDialogContentProps = {
    type: DialogType.normal,
    title: 'Remove User',
    closeButtonAriaLabel: 'Close',
    isMultiline: false,
    subText: `Are you sure you want remove ${userToBeDeleted}?`,
  };
  const deleteUser = async () => {
    await fetch(window.BASE_URL + '/subscriptions/' + id + '/users/' + userToBeDeleted, {
      mode: "cors",
      method: "DELETE",
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    })
      .then(response => {
        try {
          toggleHideDeleteDialog();
          setTimeout(() => {
            loadData();
          }, 1000);
        } catch (error) {
          console.log(error);
          window.alert("Unable to delete user.");
        }
      });
  };
  const toggleHideDeleteDialog = (aadId?: string) => {
    if (aadId !== undefined) {
      setUserToBeDeleted(aadId);
    }
    if (hideDeleteDialog) {
      setHideDeleteDialog(false);
    } else {
      setHideDeleteDialog(true);
    }
  }

  const confirmDeleteUser = async (user: any) => {
    // user: interface User
    const userId = user.AADUserId;
    toggleHideDeleteDialog(userId);
  }

  const columns: IColumn[] = [{
    key: "Column1",
    name: "Azure Active Directory ID (AAD ID)",
    fieldName: "AADUserId",
    ariaLabel: "Azure Active Directory ID (AAD ID)",
    className: "userlist",
    minWidth: 10
  },
  {
    key: "Column2",
    name: "Delete",
    ariaLabel: "Delete",
    className: "DeleteColumn",
    minWidth: 40,
    isResizable: false,
    isIconOnly: true,
    onRender: (item?: any, index?: number | undefined, column?: IColumn | undefined) => {
      return <IconButton

        onClick={() => confirmDeleteUser(item)}
        iconProps={{ iconName: 'Delete' }}
        style={{ color: SharedColors.red20 }} />
    }
  }];

  const adminColumns: IColumn[] = [{
    key: "Column1",
    name: "Azure Active Directory ID (AAD ID)",
    fieldName: "AADUserId",
    ariaLabel: "AAD User Id",
    className: "userlist",
    minWidth: 10
  }];
  const _updateUserName = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
    setNewUser(newValue || '');
  }
  return (
    <div className="Details">

      <div className="backArea">
        <Link href="/#/"><Icon iconName="ChromeBack" style={{ fontSize: 14, top: 2, position: "relative" }} /> Back</Link>
      </div>
      <div style={PanelStyles}>
        <Stack horizontal horizontalAlign="space-between">
          <StackItem>
            <Text variant={'small'} block>Subscription ID</Text>
            <Text variant={'xLarge'} block>{id}</Text>
          </StackItem>
          <StackItem>
            <DeleteSubscription SubscriptionId={id} />
          </StackItem>
        </Stack>
        
        <p style={{ display: subscriptionData?.baseUrl && subscriptionData?.baseUrl.length >= 1 ? "none" : "block" }}>
            <Text variant={'medium'}>Loading Subscription Details...</Text>
        </p>

        <div className="items">
          <TextField
            value={subscriptionData?.baseUrl}
            readOnly={true}
            borderless={true}
            label="Base URL"
            disabled={true}
            onRenderLabel={renderLabel}
          ></TextField>
          <TextField
            value={subscriptionData?.primaryKey}
            readOnly={true}
            borderless={true}
            label="Primary Key"
            type="password"
            disabled={true}
            onRenderLabel={renderLabel2}
          ></TextField>
          <TextField
            value={subscriptionData?.secondaryKey}
            readOnly={true}
            borderless={true}
            label="Secondary Key"
            type="password"
            disabled={true}
            onRenderLabel={renderLabel3}
          ></TextField>
        </div>

        <div className="items">
          
            <Stack verticalAlign="space-between" style={{paddingTop:"10px"}}>
            <Stack style={{ visibility: 'hidden' }}>
              <Text variant={'medium'}>Usage:</Text>
            </Stack>
            <StackItem>
              <Dropdown
                    label="Available Applications"
                    placeholder={"Select an Application"}
                    options={applicationOptions}
                    onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                      
                      setSelectedValues({
                        application: option?.text!,
                        api: "",
                        version: "",
                        operation: "",
                      });
                      setSubApiOptions(option?.text!);
                      setSampleCodeValue(sampleCode.replace("<app_name>", option?.text!));
                    }}
                  />
            </StackItem>
            <StackItem>
              <Dropdown
                    label="Available APIs"
                    placeholder={"Select an API"}
                    disabled={isApiDisabled}
                    options={apiOptions}
                    onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                      
                      setSelectedValues({
                        application: selectedValues?.application!,
                        api: option?.text!,
                        version: "",
                        operation: "",
                      });
                      setSubApiVersionOptions(selectedValues?.application!, option?.text!);
                      setSampleCodeValue(sampleCode.replace("<app_name>", selectedValues?.application!)
                        .replace("<api_name>", option?.text!));
                    }}
                  />
                  <p style={{ display: isApiVersionDisabled ? "none" : "block" }}>
                      <Text variant={'medium'}>API Type: {apiType}</Text>
                  </p>
            </StackItem>
            <StackItem>
              <Dropdown
                    label="API Versions"
                    placeholder={"Select a Version"}
                    disabled={isApiVersionDisabled}
                    options={apiVersionOptions}
                    onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                      
                      setSelectedValues({
                        application: selectedValues?.application!,
                        api: selectedValues?.api!,
                        version: option?.text!,
                        operation: "",
                      });
                      setSubApiVersionOperationOptions(selectedValues?.application!, selectedValues?.api!, option?.text!);
                      setSampleCodeValue(sampleCode.replace("<app_name>", selectedValues?.application!)
                        .replace("<api_name>", selectedValues?.api!)
                        .replace("<api_version>", option?.text!));
                    }}
                  />
            </StackItem>
            <StackItem>
              <Dropdown
                    label="Operations"
                    placeholder={"Select an operation"}
                    disabled={isApiVersionOperationDisabled}
                    options={apiVersionOperationOptions}
                    onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                      
                      setSelectedValues({
                        application: selectedValues?.application!,
                        api: selectedValues?.api!,
                        version: selectedValues?.version!,
                        operation: option?.key!+"",
                      });
                      setSampleCodeValue(sampleCode.replace("<app_name>", selectedValues?.application!)
                        .replace("<api_name>", selectedValues?.api!)
                        .replace("<api_version>", selectedValues?.version!)
                        .replace("<operation_name>", option?.key!+""));
                    }}
                  />
            </StackItem>
          </Stack>
            <Stack verticalAlign="space-between" style={{paddingTop:"10px"}}>
              <StackItem>
              <TextField
                value={subscriptionData?.baseUrl?sampleCodeValue
                  .replace("<endpoint_url>", subscriptionData?.baseUrl):"loading..."}
                readOnly={true}
                multiline={true}
                rows={(sampleCode.match(new RegExp("\n", "g")) || []).length + 1 > 10?
                  15:
                  (sampleCode.match(new RegExp("\n", "g")) || []).length + 1}
                borderless={true}
                label="Sample Code"
                disabled={true}
                onRenderLabel={renderLabelSampleCode}
              >
                
              </TextField>
              </StackItem>
              <StackItem>
                <Link href="https://ml.azure.com/fileexplorerAzNB?wsid=/subscriptions/a6c2a7cc-d67e-4a1a-b765-983f08c0423a/resourcegroups/lunaaitest-rg/workspaces/lunaaitest-aml&tid=72f988bf-86f1-41af-91ab-2d7cd011db47&activeFilePath=Users/xiwu/luna_demo/luna_demo.ipynb" 
                  target='blank'>Open Notebook</Link>
              </StackItem>
            </Stack>
            <Stack verticalAlign="space-between">
              <Stack style={{ visibility: 'hidden' }}>
                <Text variant={'medium'}>Usage:</Text>
              </Stack>
                <StackItem>
                  <Text variant={'large'} block>Other Resources:</Text><br/>
                  <Link  href="https://aka.ms/lunaai" target="blank">Swagger</Link><br/>
                  <Link  href="https://aka.ms/lunaai" target="blank">Sample Notebook</Link><br/>
                  <Link  href="https://aka.ms/lunaai" target="blank">Documentation</Link>
                </StackItem>
                <StackItem>
                  <br/>
                </StackItem>
                <StackItem>
                  <br/>
                </StackItem>
            </Stack>
          </div>
        
        

        
        
        <p>
            <Text variant={'large'}></Text>
        </p>

      </div>


      <Panel
        title="Add User"
        isFooterAtBottom={true}
        isHiddenOnDismiss={false}
        onRenderFooterContent={panelAddUserFooterUI}
        isOpen={isOpen}
        onDismiss={dismissPanel}
        closeButtonAriaLabel="Close"
      >
        <Stack tokens={{ childrenGap: 10 }}>
          <TextField
            value={newUser}
            onChange={_updateUserName}
            label="Azure Active Directory ID (AAD ID)"
            placeholder="username@microsoft.com"
            onRenderLabel={CustomInfoAddUserLabel}
          />
          <PrimaryButton
            disabled={newUser.length === 0}
            onClick={verifyUser}>Verify User</PrimaryButton>
          <MessageBar
            messageBarType={MessageBarType.success}
            styles={{
              root: {
                display: userValidation === undefined ? "none" : "block"
              }
            }}
          >
            User Valid!
              </MessageBar>
          <MessageBar
            messageBarType={MessageBarType.error}
            styles={{
              root: {
                display: noUserFound === true ? "block" : "none"
              }
            }}
          >
            User not found.
              </MessageBar>
          <Persona
            text={userValidation?.displayName}
            secondaryText={userValidation?.mail}
            tertiaryText={userValidation?.jobTitle}
            presence={PersonaPresence.none}
            optionalText={userValidation?.businessPhones[0]}
            showSecondaryText={true}
            imageUrl={userValidationPhoto?.toString()}

            size={PersonaSize.size72}
            styles={{
              root: {
                visibility: userValidation === undefined ? "collapse" : "visible"
              }
            }}
          />

        </Stack>
      </Panel>
      <Dialog
        hidden={hideDeleteDialog}
        onDismiss={() => toggleHideDeleteDialog(undefined)}
        dialogContentProps={dialogDeleteUserContentProps}
        modalProps={modalProps}
      >
        <DialogFooter>
          <PrimaryButton
            style={{
              borderColor: SharedColors.red20,
              backgroundColor: SharedColors.red20,
              color: "white"
            }}
            onClick={deleteUser} text="Delete" />
          <DefaultButton onClick={() => toggleHideDeleteDialog(undefined)} text="Cancel" />
        </DialogFooter>
      </Dialog>
      <FooterLinks />
    </div>
  )
}


interface AuthParams {
  id: string;
}

interface ISelectedItems {
  application: string;
  api: string;
  version: string;
  operation: string;
}

export default SubscriptionDetail;

