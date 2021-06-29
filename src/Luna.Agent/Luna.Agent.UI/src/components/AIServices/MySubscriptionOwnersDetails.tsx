import * as React from 'react';
import { useHistory, useLocation } from 'react-router';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton, CommandButton, MessageBar, MessageBarType, Icon } from '@fluentui/react';
import { IApplicationSubscription } from './IApplicationSubscription';
import { SharedColors } from '@uifabric/fluent-theme';
import { v4 as uuid } from "uuid";


const MySubscriptionOwnersDetails = (props: any) => {
    const [subscriptionDetail, setSubscriptionDetail] = React.useState<IApplicationSubscription>();
    const [selectedApplication, setselectedApplication] = React.useState<string | null>(sessionStorage.getItem('selectedApplication'));
    const [hideAddNewOwner, setHideAddNewOwner] = React.useState(true);
    const [userName, setUserName] = React.useState<string>('');
    const [userId, setUserId] = React.useState<string>('');
    const [hideDeleteDialog, setHideDeleteDialog] = React.useState(true);


    React.useEffect(() => {
        loadSubscriptionDetail(props.subscription.SubscriptionId);
    }, []);

    const labelId: string = 'dialogLabel';
    const subTextId: string = 'subTextLabel';

    const setUserIdValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setUserId(newValue || '');
    }
    const setUserNameValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setUserName(newValue || '');
    }

    const dialogDeleteOwnerContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Remove Owner',
        closeButtonAriaLabel: 'Close',
        isMultiline: false,
        subText: `Are you sure you want remove ?`,
    };

    const modalProps: IModalProps = {
        titleAriaId: labelId,
        subtitleAriaId: subTextId,
        isBlocking: true,
        isDarkOverlay: true,
        allowTouchBodyScroll: true
    }

    const toggleHideDeleteDialog = (aadId?: string) => {       
        if (hideDeleteDialog) {
            setHideDeleteDialog(false);
        } else {
            setHideDeleteDialog(true);
        }
    }
    const deleteOwnerValues = (id: string,name: string) =>{
        setUserName(name);
        setUserId(id);
        toggleHideDeleteDialog()
    }

    const loadSubscriptionDetail = (subscriptionId: string) => {
        fetch(window.BASE_URL + '/gallery/applications/' + selectedApplication + '/subscriptions/' + subscriptionId, {
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
            });
    }

    const addOwner = () => {
        fetch(window.BASE_URL + '/gallery/applications/' + selectedApplication + '/subscriptions/' + props.subscription.SubscriptionId + '/addowner', {
            mode: "cors",
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Luna-User-Id': 'test-admin',
                'Host': 'lunatest-gateway.azurewebsites.net'
            },
            body: JSON.stringify({
                "Userid": uuid(),
                "UserName": userName
            })
        })
            .then(response => response.json())
            .then(_data => {
                loadSubscriptionDetail(props.subscription.SubscriptionId);
            });
    }

    const removeOwner = () => {
        fetch(window.BASE_URL + '/gallery/applications/' + selectedApplication + '/subscriptions/' + props.subscription.SubscriptionId + '/removeowner', {
            mode: "cors",
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Luna-User-Id': 'test-admin',
                'Host': 'lunatest-gateway.azurewebsites.net'
            },
            body: JSON.stringify({
                "Userid": userId,
                "UserName": userName
            })
        })
            .then(response => response.json())
            .then(_data => {
                loadSubscriptionDetail(props.subscription.SubscriptionId);
            });
    }
    return (
        <div>
            <Text block variant={'medium'} style={{ marginTop: '10px' }}>Owners:</Text>
            {subscriptionDetail?.Owners.map((owner, index) => {
                return (
                    <div>
                        <table>
                            <tbody>
                                <tr>
                                    <td>
                                        <Text block variant='medium'>{owner.UserName}</Text>
                                </td>
                                <td rowSpan={2}>
                                <IconButton style={{ display: index === 0 ? 'none' : 'block', color: SharedColors.red10, fontSize:'40px' }}
                                iconProps={{ iconName: "Cancel" }} onClick={()=>deleteOwnerValues(owner.UserId,owner.UserName)}></IconButton>
                                </td>
                                </tr>
                                <tr>
                                <Text block variant='medium'>{owner.UserId}</Text>
                                </tr>
                            </tbody>
                        </table>                                        
                        <br />
                    </div>
                )

            })}
            <Link style={{ marginTop: '50px' }} onClick={() => setHideAddNewOwner(false)}>+ New Owner</Link>
            <div style={{ display: hideAddNewOwner ? 'none' : 'block', width: '250px', border: '1px solid black', padding: '10px' }}>
                {/* <TextField label={"User Id:"} value={userId} onChange={setUserIdValue}></TextField> */}
                <TextField label={"User Name:"} value={userName} onChange={setUserNameValue}></TextField>
                <PrimaryButton style={{ marginTop: '5px' }} onClick={() => { addOwner(); setUserId(''); setUserName('');setHideAddNewOwner(true); }}>Submit</PrimaryButton>
            </div>
            <Dialog
                hidden={hideDeleteDialog}
                onDismiss={() => toggleHideDeleteDialog(undefined)}
                dialogContentProps={dialogDeleteOwnerContentProps}
                modalProps={modalProps}
            >
                <DialogFooter>
                    <PrimaryButton
                        style={{
                            borderColor: SharedColors.red20,
                            backgroundColor: SharedColors.red20,
                            color: "white"
                        }}
                        onClick={()=>{removeOwner();toggleHideDeleteDialog(undefined); }} text="Delete" />
                    <DefaultButton onClick={() => toggleHideDeleteDialog(undefined)} text="Cancel" />
                </DialogFooter>
            </Dialog>
        </div>
    )
}

export default MySubscriptionOwnersDetails;