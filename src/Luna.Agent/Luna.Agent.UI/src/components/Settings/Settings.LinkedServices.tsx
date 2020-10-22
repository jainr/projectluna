// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import { Stack, Text, CommandButton, SelectionMode, IColumn, ShimmeredDetailsList, MessageBar, MessageBarType, Panel, TextField, DefaultButton, PrimaryButton, Separator, PanelType } from '@fluentui/react';
import { PanelStyles } from '../../helpers/PanelStyles';
import { useBoolean } from '@uifabric/react-hooks';
import { ILinkedService } from '../../interfaces/ILinkedService';

/** @component Linked Services view and code. */
export const LinkedServices = () => {
    var initWorkspaceList: never[] = [];
    const [isOpen, { setTrue: openPanel, setFalse: dismissPanel }] = useBoolean(false);
    const [isEditPanelOpen, toggleEditPanel] = React.useState(false);
    const [amlworkspaces, setAmlworkspaces] = React.useState([]);
    const [isDataLoading, setIsDataLoading] = React.useState(true);
    const [isNewWSSuccessful, setIsNewWSSuccessful] = React.useState(false);
    const [lagDelay, setLagDelay] = React.useState<boolean>(true);

    const [newWorkspace, setNewWorkspace] = React.useState<ILinkedService>({
        AADApplicationId: '',
        AADApplicationSecret: '',
        AADTenantId: '',
        ResourceId: '',
        WorkspaceName: ''
    });

    const [EditWorkspace, SetEditWorkspace] = React.useState<ILinkedService>({
        AADApplicationId: '',
        AADApplicationSecret: '',
        AADTenantId: '',
        ResourceId: '',
        WorkspaceName: ''
    });

    const buttonStyles = { root: { marginRight: 8 } };

    React.useEffect(() => {
        loadData();
    }, []);

    const onRenderFooterContent = React.useCallback(
        () => (
            <div>
                <PrimaryButton
                    onClick={CreateLinkedService} styles={buttonStyles}>
                    Save
          </PrimaryButton>
                <DefaultButton onClick={dismissPanel}>Cancel</DefaultButton>
            </div>
        ),
        [dismissPanel],
    );

    const onRenderEditFooterContent = React.useCallback(
        () => (
            <div>
                <PrimaryButton
                    onClick={UpdateLinkedService} styles={buttonStyles}>
                    Update
                </PrimaryButton>
                <DefaultButton onClick={() => toggleEditPanel(false)}>Cancel</DefaultButton>
            </div>
        ),
        [],
    );

    React.useEffect(() => {
        return () => {
            // componentwillunmount in functional component.
            // Anything in here is fired on component unmount.
            sessionStorage.removeItem('newWS');
            sessionStorage.removeItem('editWS');
        }
    }, [])

    const loadData = () => {
        const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
        fetch(window.BASE_URL + '/amlworkspaces', {
            mode: "cors",
            method: "GET",
            headers: {
                'Authorization': bearerToken,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        })
            .then(response2 => response2.json())
            .then(workspaceData => {
                initWorkspaceList = workspaceData;
                setAmlworkspaces(workspaceData);
                setIsDataLoading(false);
                if (workspaceData.length > 0){
                    SetEditWorkspace({
                        AADApplicationId: workspaceData[0].AADApplicationId,
                        AADApplicationSecret: workspaceData[0].AADApplicationSecret,
                        AADTenantId: workspaceData[0].AADTenantId,
                        ResourceId: workspaceData[0].ResourceId,
                        WorkspaceName: workspaceData[0].WorkspaceName
                    });
                }
            }).finally(() => setLagDelay(false));
    }

    //#region Admin Management
    const [amlWorkspaceColumns, setamlWorkspaceColumns] = React.useState<IColumn[]>([
        {
            key: "WorkspaceName",
            name: "Workspace Name",
            fieldName: "WorkspaceName",
            isResizable: true,
            isMultiline: false,
            minWidth: 100,
            maxWidth: 200,
            onColumnClick: _onColumnClick
        },
        {
            key: "ResourceId",
            name: "Resource ID",
            fieldName: "ResourceId",
            isResizable: true,
            isMultiline: true,
            minWidth: 100,
            maxWidth: 200,
            onColumnClick: _onColumnClick
        }
    ]);
    //#endregion
    function _onColumnClick(ev: React.MouseEvent<HTMLElement, MouseEvent>, column: IColumn): void {
        const items = initWorkspaceList;
        const columns = amlWorkspaceColumns;
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
        setAmlworkspaces(newItems);
        setamlWorkspaceColumns(newColumns);
    };

    function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
        const key = columnKey as keyof T;
        return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
    }

    const CreateLinkedService = () => {
        setIsDataLoading(true);
        const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
        const newWS = JSON.parse(sessionStorage.getItem('newWS')!);
        fetch(`${window.BASE_URL}/amlworkspaces/${newWS.WorkspaceName}`, {
            mode: "cors",
            method: "PUT",
            headers: {
                'Authorization': bearerToken,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(newWS)
        })
            .then(response => {
                if (response.status === 200 || 202) {
                    setIsNewWSSuccessful(true);
                    loadData();
                    setTimeout(() => {
                        setIsNewWSSuccessful(false);
                    }, 7000);
                    dismissPanel();
                } else {
                    window.alert(`Error uploading new workspace. - ${response.status}`);
                }
                return response.json();
            }).finally(() => setIsDataLoading(true));
    }

    const UpdateLinkedService = () => {
        setIsDataLoading(true);
        const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
        const newWS = JSON.parse(sessionStorage.getItem('editWS')!);
        fetch(`${window.BASE_URL}/amlworkspaces/${newWS.WorkspaceName}`, {
            mode: "cors",
            method: "PUT",
            headers: {
                'Authorization': bearerToken,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(newWS)
        })
            .then(response => {
                if (response.status === 200 || 202) {
                    setIsNewWSSuccessful(true);
                    loadData();
                    setTimeout(() => {
                        setIsNewWSSuccessful(false);
                    }, 7000);
                    toggleEditPanel(false);
                } else {
                    window.alert(`Error uploading new workspace. - ${response.status}`);
                }
                return response.json();
            }).finally(() => setIsDataLoading(true));
    }

    return (
        <>
            <div>
                <Stack tokens={{ childrenGap: 30 }}>
                    <div style={PanelStyles}>
                        <Stack horizontal horizontalAlign="space-between">
                            <Text variant={'xLargePlus'}>Azure Machine Learning Workspaces</Text>
                            <CommandButton
                                onClick={openPanel}
                                iconProps={{ iconName: 'Add' }}>Register Workspace</CommandButton>
                        </Stack>
                        <MessageBar
                            styles={{
                                root: {
                                    display: isDataLoading === false && amlworkspaces.length !== 0 && !lagDelay ? "none" : "block"
                                }
                            }}
                            messageBarType={MessageBarType.info}
                        >
                            No Workspaces assigned.
                </MessageBar>
                        <MessageBar
                            onDismiss={() => setIsNewWSSuccessful(false)}
                            styles={{
                                root: {
                                    display: isNewWSSuccessful === false ? "none" : "block"
                                }
                            }}
                            messageBarType={MessageBarType.success}
                        >
                            New workspace created.
                    </MessageBar>
                        <ShimmeredDetailsList
                            items={amlworkspaces || []}
                            columns={amlWorkspaceColumns}
                            selectionMode={SelectionMode.none}
                            useReducedRowRenderer={true}
                            onActiveItemChanged={
                                (item?: any, index?: number | undefined, ev?: React.FocusEvent<HTMLElement> | undefined) => {
                                    SetEditWorkspace({
                                        AADApplicationId: item.AADApplicationId,
                                        AADApplicationSecret: item.AADApplicationSecret,
                                        AADTenantId: item.AADTenantId,
                                        ResourceId: item.ResourceId,
                                        WorkspaceName: item.WorkspaceName
                                    });
                                    toggleEditPanel(true);
                                }
                            }
                            selectionPreservedOnEmptyClick={true}
                            enableShimmer={isDataLoading}
                            ariaLabelForShimmer="Content is being fetched"
                            ariaLabelForGrid="Item details"
                        />
                    </div>
                </Stack>

            </div>
            <Panel
                isOpen={isOpen}
                onDismiss={dismissPanel}
                headerText="Create Linked Service"
                closeButtonAriaLabel="Close"
                onRenderFooterContent={onRenderFooterContent}
                // Stretch panel content to fill the available height so the footer is positioned
                // at the bottom of the page
                isFooterAtBottom={true}
            >
                <Stack tokens={{ childrenGap: 10 }}>
                    <TextField
                        required
                        autoComplete="off"
                        label="Workspace Name"
                        value={newWorkspace.WorkspaceName}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = newWorkspace;
                            setNewWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: newValue!
                            });
                            sessionStorage.setItem('newWS', JSON.stringify(ws));
                        })}
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        value={newWorkspace.ResourceId}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = newWorkspace;
                            setNewWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: newValue!,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('newWS', JSON.stringify(ws));
                        })}
                        label="Resource ID"
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        label="AAD Application ID"
                        value={newWorkspace.AADApplicationId}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = newWorkspace;
                            setNewWorkspace({
                                AADApplicationId: newValue!,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('newWS', JSON.stringify(ws));
                        })}
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        label="AAD Tenant ID"
                        value={newWorkspace.AADTenantId}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = newWorkspace;
                            setNewWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: newValue!,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('newWS', JSON.stringify(ws));
                        })}
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        label="AAD Application Secret"
                        value={newWorkspace.AADApplicationSecret}
                        type="password"
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = newWorkspace;
                            setNewWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: newValue!,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('newWS', JSON.stringify(ws));
                        })}
                    ></TextField>

                    <Separator />

                    <CommandButton
                        href="https://ms.portal.azure.com/#create/Microsoft.MachineLearningServices"
                        target="_blank"
                        iconProps={{ iconName: 'Link' }}
                    >Create Workspace</CommandButton>
                </Stack>
            </Panel>

            <Panel
                isOpen={isEditPanelOpen}
                onDismiss={() => toggleEditPanel(false)}
                headerText="Edit Linked Service"
                closeButtonAriaLabel="Close"
                onRenderFooterContent={onRenderEditFooterContent}
                isFooterAtBottom={true}
            >
                <Stack tokens={{ childrenGap: 10 }}>
                    <TextField
                        disabled
                        autoComplete="off"
                        label="Workspace Name"
                        value={EditWorkspace.WorkspaceName}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = EditWorkspace;
                            SetEditWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: newValue!
                            });
                            sessionStorage.setItem('editWS', JSON.stringify(ws));
                        })}
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        value={EditWorkspace.ResourceId}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = EditWorkspace;
                            SetEditWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: newValue!,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('editWS', JSON.stringify(ws));
                        })}
                        label="Resource ID"
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        label="AAD Application ID"
                        value={EditWorkspace.AADApplicationId}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = EditWorkspace;
                            SetEditWorkspace({
                                AADApplicationId: newValue!,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('editWS', JSON.stringify(ws));
                        })}
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        label="AAD Tenant ID"
                        value={EditWorkspace.AADTenantId}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = EditWorkspace;
                            SetEditWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: ws.AADApplicationSecret,
                                AADTenantId: newValue!,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('editWS', JSON.stringify(ws));
                        })}
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        label="AAD Application Secret"
                        value={EditWorkspace.AADApplicationSecret}
                        type="password"
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let ws = EditWorkspace;
                            SetEditWorkspace({
                                AADApplicationId: ws.AADApplicationId,
                                AADApplicationSecret: newValue!,
                                AADTenantId: ws.AADTenantId,
                                ResourceId: ws.ResourceId,
                                WorkspaceName: ws.WorkspaceName
                            });
                            sessionStorage.setItem('editWS', JSON.stringify(ws));
                        })}
                    ></TextField>

                    <Separator />

                    <CommandButton
                        href="https://ms.portal.azure.com/#create/Microsoft.MachineLearningServices"
                        target="_blank"
                        iconProps={{ iconName: 'Link' }}
                    >Create Workspace</CommandButton>
                </Stack>
            </Panel>
        </>
    );


}