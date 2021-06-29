import * as React from 'react';
import { useHistory, useLocation } from 'react-router';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton, CommandButton, MessageBar, MessageBarType, Icon } from '@fluentui/react';
import { IApplicationSubscription } from './IApplicationSubscription';
import { SharedColors } from '@uifabric/fluent-theme';


const MySubscriptionDetails = (props: any) => {

    const [isBaseUrlCopySuccess, setBaseUrlCopySuccess] = React.useState<boolean>(false);
    const [isPrimaryKeyCopySuccess, setPrimaryKeyCopySuccess] = React.useState<boolean>(false);
    const [isSecondaryKeyCopySuccess, setSecondaryKeyCopySuccess] = React.useState<boolean>(false);
    const [isNotesUpdateSuccess, setNotesUpdateSuccess] = React.useState<boolean>(false);
    const [isHidePrimaryKey, setHidePrimaryKey] = React.useState<boolean>(true);
    const [isHideSecondaryKey, setHideSecondaryKey] = React.useState<boolean>(true);
    const [primaryKey, setPrimaryKey] = React.useState<string>();
    const [secondaryKey, setSecondaryKey] = React.useState<string>();
    const [subNotes, setNotes] = React.useState<string>();
    const [subscriptionDetail, setSubscriptionDetail] = React.useState<IApplicationSubscription>();
    const [hideRegenerateKeyDialog, setHideRegenerateKeyDialog] = React.useState(true);
    const [keyName, setKeyName] = React.useState('');
    const [hideDeleteSubscriptionDialog, setHideDeleteSubscriptionDialog] = React.useState(true);
    const [subName, setSubName] = React.useState<string>();
    const [isSubNameWrong, setSubNameWrong] = React.useState<boolean>(false);
    const [selectedApplication, setselectedApplication] = React.useState<string | null>(sessionStorage.getItem('selectedApplication'));

    const labelId: string = 'dialogLabel';
    const subTextId: string = 'subTextLabel';

    const history = useHistory();        
    React.useEffect(() => {
        loadSubscriptionDetail(props.subscription.SubscriptionId);
    }, []);

    const toggleHideDialog = () => {
        if (hideRegenerateKeyDialog) {
            setHideRegenerateKeyDialog(false);
        } else {
            setHideRegenerateKeyDialog(true);
        }
    }

    const toggleDeleteSubDialog = () => {
        if (hideDeleteSubscriptionDialog) {
            setHideDeleteSubscriptionDialog(false);
        } else {
            setHideDeleteSubscriptionDialog(true);
        }
    }

    const modalProps: IModalProps = {
        titleAriaId: labelId,
        subtitleAriaId: subTextId,
        isBlocking: true,
        isDarkOverlay: true,
        allowTouchBodyScroll: true,
    }

    const dialogContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Confirm regenerate key',
        closeButtonAriaLabel: 'Cancel',
        isMultiline: true
        // subText: 'Are you sure you want to add as an admin',
    };

    const dialogDeleteSubContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Confirm delete subscription',
        closeButtonAriaLabel: 'Cancel',
        isMultiline: true
    };
    const copyClick = (type: string, value: any) => {
        const urlValue = value;
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
        if (type === 'BaseUrl') {
            setBaseUrlCopySuccess(true);

            setTimeout(() => {
                setBaseUrlCopySuccess(false);
            }, 3000);
        }
        else if (type === 'PrimaryKey') {
            setPrimaryKeyCopySuccess(true);

            setTimeout(() => {
                setPrimaryKeyCopySuccess(false);
            }, 3000);

        }
        else if (type === 'SecondaryKey') {
            setSecondaryKeyCopySuccess(true);

            setTimeout(() => {
                setSecondaryKeyCopySuccess(false);
            }, 3000);

        }

    }
    const hideShowPrimaryKey = () => {
        isHidePrimaryKey ? setHidePrimaryKey(false) : setHidePrimaryKey(true);
    }
    const hideShowSecondaryKey = () => {
        isHideSecondaryKey ? setHideSecondaryKey(false) : setHideSecondaryKey(true);
    }
    const setNotesValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setNotes(newValue || '');
    }
    const setSubNameValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setSubName(newValue || '');
    }
    const updateNotes = () => {
        var updatedNotes = { notes: subNotes };
        console.log(JSON.stringify(updatedNotes))
        fetch(window.BASE_URL + '/gallery/applications/'+selectedApplication+'/subscriptions/' + subscriptionDetail?.SubscriptionId + '/updatenotes', {
            mode: "cors",
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Luna-User-Id': 'test-admin',
                'Host': 'lunatest-gateway.azurewebsites.net'
            },
            body: JSON.stringify(updatedNotes),
        })
            .then(response => response.json())
            .then(_data => {
                setNotesUpdateSuccess(true);
                setTimeout(() => {
                    setNotesUpdateSuccess(false);
                }, 3000);
            });
    }
    const loadSubscriptionDetail = (subscriptionId: string) => {
        fetch(window.BASE_URL + '/gallery/applications/'+selectedApplication+'/subscriptions/' + subscriptionId, {
            mode: "cors",
            method: "GET",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Luna-User-Id': 'test-admin',
                'Host': 'lunatest-gateway.azurewebsites.net'
            }
        })
            .then(response => response.json())
            .then(_data => {
                setSubscriptionDetail(_data);
                setPrimaryKey(_data.PrimaryKey);
                setSecondaryKey(_data.SecondaryKey);
                setNotes(_data.Notes);
            });
    }

    const regenerateSubscriptionKey = (keyType: string) => {
        fetch(window.BASE_URL + '/gallery/applications/'+selectedApplication+'/subscriptions/' + props.subscription.SubscriptionId + '/regeneratekey?key-name=' + keyType, {
            mode: "cors",
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Luna-User-Id': 'test-admin',
                'Host': 'lunatest-gateway.azurewebsites.net'
            }
        })
            .then(response => response.json())
            .then(_data => {
                setPrimaryKey(_data.PrimaryKey)
                setSecondaryKey(_data.SecondaryKey)
            });
    }

    const deleteSubscriptionKey = () => {
        if (subName === subscriptionDetail?.SubscriptionName) {
            fetch(window.BASE_URL + '/gallery/applications/'+selectedApplication+'/subscriptions/' + subscriptionDetail?.SubscriptionName, {
                mode: "cors",
                method: "DELETE",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Luna-User-Id': 'test-admin',
                    'Host': 'lunatest-gateway.azurewebsites.net'
                }
            })
                .then(response => response.json())
                .then(_data => {
                    history.push('servicedetails')                
            });
        }
        else
        {
            setSubNameWrong(true);
        }
    }
    return (
        <div>
            <Stack>
                <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }} title={"Subscription Id"}>Subscription Id:</Text>
                <Text style={{ marginTop: '5px' }}>{props.subscription.SubscriptionId}</Text>
                <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }} title={"Created Date"}>Created Date:</Text>
                <Text style={{ marginTop: '5px' }}>{props.subscription.CreatedTime}</Text>

                <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }} title={"Base URL"}>Base URL:</Text>
                <Stack style={{ display: isBaseUrlCopySuccess ? "block" : 'none' }}>
                    <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
                </Stack>
                <Stack horizontal verticalAlign="baseline">
                    <input
                        style={{ height: '27px', width: '243px' }}
                        disabled={true}
                        value={subscriptionDetail?.BaseUrl}
                    />
                    <CommandButton onClick={() => copyClick("BaseUrl", subscriptionDetail?.BaseUrl)} iconProps={{ iconName: 'Copy' }} title={"Copy"} />
                </Stack>

                <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }} title={"Primary Key"}>Primary Key:</Text>
                <Stack style={{ display: isPrimaryKeyCopySuccess ? "block" : 'none' }}>
                    <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
                </Stack>
                <Stack horizontal verticalAlign="baseline">
                    <input
                        style={{ height: '25px', width: '250px' }}
                        disabled={true}
                        value={primaryKey}
                        type={isHidePrimaryKey ? "password" : "text"} />
                    <CommandButton onClick={() => copyClick("PrimaryKey", primaryKey)} iconProps={{ iconName: 'Copy' }} title={"Copy"} />
                    <CommandButton onClick={() => hideShowPrimaryKey()} iconProps={{ iconName: 'RedEye' }} title={"Show Password"} />
                    <CommandButton onClick={() => { toggleHideDialog(); setKeyName("PrimaryKey") }} iconProps={{ iconName: 'Refresh' }} title={"Regenerate Key"} />
                </Stack>

                <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }} title={"Secondary Key"}>Secondary Key:</Text>
                <Stack style={{ display: isSecondaryKeyCopySuccess ? "block" : 'none' }}>
                    <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
                </Stack>
                <Stack horizontal verticalAlign="baseline">
                    <input
                        style={{ height: '25px', width: '250px' }}
                        disabled={true}
                        value={secondaryKey}
                        type={isHideSecondaryKey ? "password" : "text"} />
                    <CommandButton onClick={() => copyClick("SecondaryKey", secondaryKey)} iconProps={{ iconName: 'Copy' }} title={"Copy"} />
                    <CommandButton onClick={() => hideShowSecondaryKey()} iconProps={{ iconName: 'RedEye' }} title={"Show Password"} />
                    <CommandButton onClick={() => { toggleHideDialog(); setKeyName("SecondaryKey") }} iconProps={{ iconName: 'Refresh' }} title={"Regenerate Key"} />
                </Stack>
                <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }} title={"Notes"}>Notes:</Text>
                <Stack style={{ display: isNotesUpdateSuccess ? "block" : 'none' }}>
                    <MessageBar messageBarType={MessageBarType.success}>Notes Updated Successfully</MessageBar>
                </Stack>
                <TextField
                    value={subNotes}
                    multiline={true}
                    onChange={setNotesValue}
                />
            </Stack>
            <PrimaryButton style={{ marginTop: '10px' }} onClick={updateNotes} >Update</PrimaryButton>
            <br />
            <p />
            <Link onClick={() => props.toggle(true)}>Manage Owners&gt;&gt;</Link>
            <Stack horizontal style={{ marginTop: '30px' }}>
                <DefaultButton text="Delete" style={{ color: '#ff0000', borderColor: '#ff0000' }} onClick={() => toggleDeleteSubDialog()}></DefaultButton>
                <div style={{ marginLeft: '190px' }}>
                    <DefaultButton onClick={()=>props.closePanel()}>Close</DefaultButton>
                </div>
            </Stack>
            <Dialog
                hidden={hideRegenerateKeyDialog}
                onDismiss={toggleHideDialog}
                dialogContentProps={dialogContentProps}
                modalProps={modalProps}
            >
                <Stack horizontal>
                    <Icon iconName="Warning"
                        style={{ cursor: "pointer", color: SharedColors.orange10, fontSize: '40px', margin: '10px' }} />
                    <Text block variant={'small'}>Warning! Regenerating key is irreversible and cannot be undone. Proceeding will permanently disable the current key immediately.</Text>
                </Stack>
                <Text block variant={'small'} style={{ marginLeft: '40px' }}><Link>Learn about best practice for key rotation </Link><IconButton iconProps={{ iconName: "OpenInNewWindow" }} target="" /></Text>
                <hr></hr>
                <DialogFooter>
                    <PrimaryButton
                        text="Continue"
                        onClick={() => { regenerateSubscriptionKey(keyName); toggleHideDialog() }} />
                    <DefaultButton onClick={toggleHideDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>

            <Dialog
                hidden={hideDeleteSubscriptionDialog}
                onDismiss={toggleDeleteSubDialog}
                dialogContentProps={dialogDeleteSubContentProps}
                modalProps={modalProps}
            >
                <Stack horizontal>
                    <Icon iconName="Warning"
                        style={{ cursor: "pointer", color: SharedColors.orange10, fontSize: '40px', margin: '10px' }} />
                    <Text block variant={'small'}>Warning! Deleting the subscription is irreversible and cannot be undone. Proceeding will permanently delete this subscription. The subscription keys will stop working immediately.</Text>
                </Stack>
                <hr></hr>
                <Text block variant={'small'} style={{ color: 'grey', marginBottom: '10px' }}>Type the subscription name</Text>
                <TextField style={{ marginTop: '5px' }}
                    value={subName}
                    onChange={setSubNameValue}></TextField>
                     <Stack style={{ display: isSubNameWrong ? "block" : 'none' }}>
                    <MessageBar messageBarType={MessageBarType.error}>Please enter “{subscriptionDetail?.SubscriptionName}” to confirm delete</MessageBar>
                </Stack>
                <DialogFooter>
                    <PrimaryButton
                        text="Delete"
                        onClick={() => { deleteSubscriptionKey();}} />
                    <DefaultButton onClick={toggleDeleteSubDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
        </div>
    )
}

export default MySubscriptionDetails;
