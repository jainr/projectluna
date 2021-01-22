// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import { Text, TextField, StackItem, Stack, DefaultButton, CommandButton, Link, Icon, MessageBar, MessageBarType, Panel, PrimaryButton, DetailsList, SelectionMode, IColumn, Dialog, DialogFooter, DialogType, IModalProps, IDialogContentProps, Persona, PersonaSize, PersonaPresence, IconButton } from '@fluentui/react';
import { useParams } from 'react-router-dom';
import { SharedColors } from '@uifabric/fluent-theme';
import './SubscriptionDetail.css';
import { ISubscriptionDetail } from './ISubscriptionDetail';
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
    secondaryKeySecretName: ""
  });

  const [isCopySuccess, setSuccess] = React.useState<boolean>(false);
  const [isCopySuccess2, setSuccess2] = React.useState<boolean>(false);
  const [isCopySuccess3, setSuccess3] = React.useState<boolean>(false);

  const [isOpen, setIsOpen] = React.useState(false);
  const [newUser, setNewUser] = React.useState("");

  const [userValidation, setUserValidation] = React.useState<IUserLookUp | undefined>();
  const [noUserFound, setNoUserFound] = React.useState<boolean>();
  const [userValidationPhoto, setUserValidationPhoto] = React.useState<string | ArrayBuffer | null>();

  const [userToBeDeleted, setUserToBeDeleted] = React.useState('');
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
      .then(_data => { setData(_data); });
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

        <p>
            <Text variant={'large'}></Text>
        </p>

        <Stack verticalAlign="space-between" style={{paddingTop:"20px"}}>
          <StackItem>
            <Text variant={'large'} block>Resources:</Text>
          </StackItem>
          <StackItem>
            <Link  href="https://aka.ms/lunaai" target="blank">Swagger</Link>
          </StackItem>
          <StackItem>
            <Link  href="https://aka.ms/lunaai" target="blank">Sample Notebook</Link>
          </StackItem>
          <StackItem>
            <Link  href="https://aka.ms/lunaai" target="blank">Documentation</Link>
          </StackItem>
        </Stack>
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

export default SubscriptionDetail;

