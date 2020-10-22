// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import { Stack, Text, CommandButton, SelectionMode, IColumn, PrimaryButton, DefaultButton, MessageBar, MessageBarType, IconButton, IModalProps, IDialogContentProps, DialogType, Panel, TextField, Persona, PersonaPresence, PersonaSize, Dialog, DialogFooter, ShimmeredDetailsList, IListProps } from '@fluentui/react';
import { PanelStyles } from '../../helpers/PanelStyles';
import { IUserLookUp } from '../../interfaces/IUserLookUp';
import { getUserByEmail, getOtherUserPhoto } from '../../GraphService';
import { IUserDetail } from '../SubscriptionDetail/IUserDetail';
import { SharedColors } from '@uifabric/fluent-theme';
import { CustomInfoAddUserLabel } from '../_subcomponents/CustomInfoAddUserLabel';

/** @return {JSX.Element} Administration view and code. */
export const Administration = () => {
    var initAdminList: never[] = [];
    const [dataIsLoaded, setDataIsLoaded] = React.useState(true);
    const [admins, setAdmins] = React.useState([]);
    const [newAdmin, setNewAdmin] = React.useState("");
    const [adminValidation, setAdminValidation] = React.useState<IUserLookUp | undefined>();
    const [noAdminFound, setNoAdminFound] = React.useState<boolean>();
    const [adminValidationPhoto, setAdminValidationPhoto] = React.useState<string | ArrayBuffer | null>();
    const [adminToBeDeleted, setAdminToBeDeleted] = React.useState('');

    //#region Admin Management
    const [adminColumns, setAdminColumns] = React.useState<IColumn[]>([
        {
            key: "aadId",
            name: "Azure Active Directory User ID",
            fieldName: "AADUserId",
            isResizable: true,
            isMultiline: false,
            minWidth: 200,
            onColumnClick: _onColumnClick
        },
        {
            key: "descCol",
            name: "Description",
            fieldName: "Description",
            isResizable: true,
            isMultiline: false,
            minWidth: 150,
            onColumnClick: _onColumnClick
        },
        {
            key: "roleCol",
            name: "Role",
            fieldName: "Role",
            isResizable: true,
            isMultiline: false,
            minWidth: 170,
            onColumnClick: _onColumnClick
        },
        {
            key: "deleteCol",
            name: "Delete",
            isResizable: false,
            minWidth: 80,
            maxWidth: 80,
            isMultiline: false,
            isIconOnly: true,
            onColumnClick: _onColumnClick,
            onRender: (item: IUserDetail) => {
                return (<Stack horizontal verticalAlign="center">
                    <Text variant={'small'} 
                    styles={{ root: { color: SharedColors.red20 }}}
                    onClick={() => deleteAdminWarning(item.AADUserId)}>Remove</Text>
                    <IconButton onClick={() => deleteAdminWarning(item.AADUserId)}
                    style={{ color: SharedColors.red20 }}
                    iconProps={{ iconName: "Delete" }}
                    ariaLabel={"Delete "} />
                </Stack>)
            }
        }
    ]);
    //#endregion

    //#region Dialog
    const [hideDialog, setHideDialog] = React.useState(true);
    const [hideDeleteDialog, setHideDeleteDialog] = React.useState(true);
    const [isAdminPanelOpen, setAddAdminIsOpen] = React.useState(false);
    const openAddAdminPanel = () => setAddAdminIsOpen(true);
    const dismissPanel = () => setAddAdminIsOpen(false);

    const labelId: string = 'dialogLabel';
    const subTextId: string = 'subTextLabel';
    const toggleHideDialog = () => {
        if (hideDialog) {
            setHideDialog(false);
        } else {
            setHideDialog(true);
        }
    }
    const toggleHideDeleteDialog = (aadId?: string) => {
        if (aadId !== undefined) {
            setAdminToBeDeleted(aadId);
        }
        if (hideDeleteDialog) {
            setHideDeleteDialog(false);
        } else {
            setHideDeleteDialog(true);
        }
    }

    const modalProps: IModalProps = {
        titleAriaId: labelId,
        subtitleAriaId: subTextId,
        isBlocking: true,
        isDarkOverlay: true,
        allowTouchBodyScroll: true
    }

    const dialogContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Cancel Subscription',
        closeButtonAriaLabel: 'Close',
        isMultiline: true,
        subText: 'Are you sure you want to add as an admin',
    };

    const dialogDeleteAdminContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Remove Administrator',
        closeButtonAriaLabel: 'Close',
        isMultiline: false,
        subText: `Are you sure you want remove ${adminToBeDeleted}?`,
    };
    //#endregion

    React.useEffect(() => {
        loadData();
    }, []);
    
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
        
    const loadData = () => {
        fetch(window.BASE_URL + '/admins', {
            mode: "cors",
            method: "GET",
            headers: {
                'Authorization': bearerToken,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(adminData => {
                initAdminList = adminData;
                setAdmins(adminData);
                setDataIsLoaded(false);
            });
    }

    //#region Admin Logic
    const addAdmin = () => {
        return fetch(window.BASE_URL + '/admins/' + adminValidation?.userPrincipalName, {
            method: 'put',
            headers: { 
                'Authorization': bearerToken,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
             },
            body: JSON.stringify({
                "AADUserId": adminValidation?.userPrincipalName,
                "Description": adminValidation?.displayName,
                "Role": "Admin",
                "ObjectId": adminValidation?.id
            })
        })
            .then(response => {
                try {
                    loadData();
                    dismissPanel();
                } catch (error) {
                    console.log(error);
                    window.alert("Unable to add admin.");
                }
            });
    };

    const deleteAdminWarning = (adminAADId: string) => {
        toggleHideDeleteDialog(adminAADId);
    };

    const deleteAdmin = () => {
        return fetch(window.BASE_URL + '/admins/' + adminToBeDeleted, {
            method: 'delete',
            headers: { 
                'Authorization': bearerToken,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
             },
        })
            .then(response => {
                if (response.status === 409) {
                    window.alert("Admin cannot remove themselves from the admin list.");
                    toggleHideDeleteDialog();
                    return;
                } 
                else if (response.status === 404) {
                    window.alert("User doesn't seem to exist to be deleted.");
                    toggleHideDeleteDialog();
                    return;
                }
                else {
                    toggleHideDeleteDialog();
                    loadData();
                }
            });
    };

    const verifyAdmin = async () => {
        try {
            const userInfo: IUserLookUp = await getUserByEmail(window.userToken, newAdmin);
            setAdminValidation(userInfo);
            setNoAdminFound(false);
            try {
                const lookupUserPhoto = await getOtherUserPhoto(window.userToken, userInfo.id);
                console.log(lookupUserPhoto);
                let fileReader = new FileReader();
                fileReader.onloadend = function (e) {
                    if (e.target !== null) {
                        let base64 = e.target.result;
                        setAdminValidationPhoto(base64);
                    }
                }
                fileReader.readAsDataURL(lookupUserPhoto);

            } catch (errorPhoto) {
                console.info("No image found.");
            }
        } catch (error) {
            setAdminValidation(undefined);
            setNoAdminFound(true);
        }
    };
    const _updateAdminName = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setNewAdmin(newValue || '');
    }
    //#endregion


    const panelAddAdminFooterUI = () => (
        <Stack horizontal tokens={{ childrenGap: 10 }}>
            <PrimaryButton
                disabled={adminValidation?.userPrincipalName === undefined}
                onClick={addAdmin}>Add Admin</PrimaryButton>
            <DefaultButton onClick={dismissPanel}>Cancel</DefaultButton>
        </Stack>
    );
    function _onColumnClick(ev: React.MouseEvent<HTMLElement, MouseEvent>, column: IColumn): void {
        const items = initAdminList;
        const columns = adminColumns;
        const newColumns: IColumn[] = columns.slice();
        const currColumn: IColumn = newColumns.filter(currCol => column.key === currCol.key)[0];
        newColumns.forEach((newCol: IColumn) => {
          if (newCol === currColumn) {
            currColumn.isSortedDescending = !currColumn.isSortedDescending;
            currColumn.isSorted = true;
          } else {
            newCol.isSorted = false;
            newCol.isSortedDescending = true;
          }
        });
        const newItems = _copyAndSort(items, currColumn.fieldName!, currColumn.isSortedDescending);
        setAdmins(newItems);
        setAdminColumns(newColumns);
    };
  
    function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
      const key = columnKey as keyof T;
      return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
    }


    return (
        <div>
            <div style={PanelStyles}>
                <Stack horizontal horizontalAlign="space-between">
                    <Text variant={'xLargePlus'}>Administrators</Text>
                    <CommandButton
                        onClick={openAddAdminPanel}
                        iconProps={{ iconName: 'Add' }}>Add New</CommandButton>
                </Stack>
                <div>
                    <MessageBar
                        styles={{
                            root: {
                                display: dataIsLoaded === false && admins.length !== 0 ? "none" : "block"
                            }
                        }}
                        messageBarType={MessageBarType.info}
                    >
                        No administrators assigned.
                </MessageBar>
                    <ShimmeredDetailsList
                        items={admins || []}
                        columns={adminColumns}
                        selectionMode={SelectionMode.none}
                        useReducedRowRenderer={true}
                        selectionPreservedOnEmptyClick={true}
                        enableShimmer={dataIsLoaded}
                        ariaLabelForShimmer="Content is being fetched"
                        ariaLabelForGrid="Item details"
                        listProps={shimmeredDetailsListProps}
                    />
                </div>
            </div>
            <Panel
                title="Add User"
                isFooterAtBottom={true}
                isHiddenOnDismiss={false}
                onRenderFooterContent={panelAddAdminFooterUI}
                isOpen={isAdminPanelOpen}
                onDismiss={dismissPanel}
                closeButtonAriaLabel="Close"
            >
                <Stack tokens={{ childrenGap: 10 }}>
                    <TextField
                        value={newAdmin}
                        onChange={_updateAdminName}
                        label="Azure Active Directory ID (AAD ID)"
                        placeholder="username@microsoft.com"
                        onRenderLabel={CustomInfoAddUserLabel}
                    />
                    <PrimaryButton
                        disabled={newAdmin.length === 0}
                        onClick={verifyAdmin}>Verify User</PrimaryButton>
                    <MessageBar
                        messageBarType={MessageBarType.success}
                        styles={{
                            root: {
                                display: adminValidation === undefined ? "none" : "block"
                            }
                        }}
                    >
                        User Valid!
              </MessageBar>
                    <MessageBar
                        messageBarType={MessageBarType.error}
                        styles={{
                            root: {
                                display: noAdminFound === true ? "block" : "none"
                            }
                        }}
                    >
                        User not found.
              </MessageBar>
                    <Persona
                        text={adminValidation?.displayName}
                        secondaryText={adminValidation?.mail}
                        tertiaryText={adminValidation?.jobTitle}
                        presence={PersonaPresence.none}
                        optionalText={adminValidation?.businessPhones[0]}
                        showSecondaryText={true}
                        imageUrl={adminValidationPhoto?.toString()}
                        size={PersonaSize.size72}
                        styles={{
                            root: {
                                visibility: adminValidation === undefined ? "collapse" : "visible"
                            }
                        }}
                    />

                </Stack>
            </Panel>
            <Dialog
                hidden={hideDialog}
                onDismiss={toggleHideDialog}
                dialogContentProps={dialogContentProps}
                modalProps={modalProps}
            >
                <DialogFooter>
                    <PrimaryButton
                        onClick={addAdmin} text="Add Admin" />
                    <DefaultButton onClick={toggleHideDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>

            <Dialog
                hidden={hideDeleteDialog}
                onDismiss={() => toggleHideDeleteDialog(undefined)}
                dialogContentProps={dialogDeleteAdminContentProps}
                modalProps={modalProps}
            >
                <DialogFooter>
                    <PrimaryButton
                        style={{
                            borderColor: SharedColors.red20,
                            backgroundColor: SharedColors.red20,
                            color: "white"
                        }}
                        onClick={deleteAdmin} text="Delete" />
                    <DefaultButton onClick={() => toggleHideDeleteDialog(undefined)} text="Cancel" />
                </DialogFooter>
            </Dialog>
        </div>

    )
}

const shimmeredDetailsListProps: IListProps = {
    renderedWindowsAhead: 0,
    renderedWindowsBehind: 0,
};