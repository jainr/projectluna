import { DefaultButton, Dialog, DialogFooter, DialogType, IDialogContentProps, IModalProps, PrimaryButton } from '@fluentui/react';
import { SharedColors } from '@uifabric/fluent-theme';
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';

interface IDeleteSubscriptionProps {
    /** Required Subscription ID for the API to delete the subscription. */
    SubscriptionId: string;
}

const cancelBtnStyles: React.CSSProperties = {
    borderColor: "#8A8886",
    color: "#C1423F"
}

const dialogContentProps: IDialogContentProps = {
    type: DialogType.largeHeader,
    title: 'Delete Subscription',
    closeButtonAriaLabel: 'Close',
    isMultiline: true,
    subText: 'Are you sure you want to Delete your subscription?  You will not be able to recover it after removal.',
};
const successDialogContentProps: IDialogContentProps = {
    type: DialogType.largeHeader,
    title: 'Subscription Deleted',
    closeButtonAriaLabel: 'Close',
    isMultiline: false,
    subText: 'Deletion complete, click or tap close to return to your subscription list...',
};
const deleteDialogContentProps: IDialogContentProps = {
    type: DialogType.largeHeader,
    title: 'Error',
    closeButtonAriaLabel: 'Close',
    isMultiline: false,
    subText: 'There was an issue deleting the subscription, reach out to support.',
};
const modalProps: IModalProps = {
    titleAriaId: 'titleId',
    subtitleAriaId: 'subtitleId',
    isBlocking: true,
    isDarkOverlay: true,
    closeButtonAriaLabel: 'Close',
    allowTouchBodyScroll: true
}

/** @component Panel for deleting a subscrption (including UI layout). */
export const DeleteSubscription = (props: IDeleteSubscriptionProps) => {
    const [hideDialog, setHideDialog] = React.useState(true);
    const [deleteDialog, setDeleteDialog] = React.useState(true);
    const [successDialog, setSuccessDialog] = React.useState(true);

    const toggleDeleteShowHideDialog = () => {
        if (hideDialog) {
            setHideDialog(false);
        } else {
            setHideDialog(true);
        }
    }
    const toggleErrorDialog = () => {
        if (deleteDialog) {
            setDeleteDialog(false);
        } else {
            setDeleteDialog(true);
        }
    }
    const toggleSuccessDialog = () => {
        if (successDialog) {
            setSuccessDialog(false);
        } else {
            setSuccessDialog(true);
        }
    }

    const confirmDelete = async () => {
        const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
        toggleDeleteShowHideDialog();
        await fetch(`${window.BASE_URL}/subscriptions/${props.SubscriptionId}`, {
            method: 'DELETE',
            mode: 'cors',
            cache: 'no-cache',
            headers: {
                'Authorization': bearerToken,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        })
            .then(response => {
                if (response.status === 200 || 202 || 204) {
                    toggleSuccessDialog();
                } else {
                    toggleDeleteShowHideDialog();
                }
            })
    }

    return (
        <div id="DeleteSubscription">
            <DefaultButton
                style={cancelBtnStyles}
                onClick={toggleDeleteShowHideDialog}>Delete Subscription</DefaultButton>
            <Dialog
                hidden={hideDialog}
                onDismiss={toggleDeleteShowHideDialog}
                dialogContentProps={dialogContentProps}
                modalProps={modalProps}>
                <DialogFooter>
                    <PrimaryButton
                        style={{
                            borderColor: SharedColors.red20,
                            backgroundColor: SharedColors.red20,
                            color: "white"
                        }}
                        onClick={confirmDelete} text="Delete" />
                    <DefaultButton onClick={toggleDeleteShowHideDialog} text="Close" />
                </DialogFooter>
            </Dialog>
            <Dialog
                hidden={successDialog}
                onDismiss={toggleSuccessDialog}
                dialogContentProps={successDialogContentProps}
                modalProps={modalProps}>
                <DialogFooter>
                    <DefaultButton onClick={() => {
                        window.location.href = `${window.location.origin}/#/`;
                    }} text="Close" />
                </DialogFooter>
            </Dialog>
            <Dialog
                hidden={deleteDialog}
                onDismiss={toggleDeleteShowHideDialog}
                dialogContentProps={deleteDialogContentProps}
                modalProps={modalProps}>
                <DialogFooter>
                    <DefaultButton onClick={toggleErrorDialog} text="Close" />
                </DialogFooter>
            </Dialog>
        </div>
    )
}

