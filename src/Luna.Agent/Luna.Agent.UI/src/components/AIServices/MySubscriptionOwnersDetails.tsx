import * as React from 'react';
import { useHistory, useLocation } from 'react-router';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton, CommandButton, MessageBar, MessageBarType, Icon } from '@fluentui/react';
import { IApplicationSubscription } from './IApplicationSubscription';
import { SharedColors } from '@uifabric/fluent-theme';


const MySubscriptionOwnersDetails = (props: any) => {
    const [subscriptionDetail, setSubscriptionDetail] = React.useState<IApplicationSubscription>();

    
    React.useEffect(() => {
        loadSubscriptionDetail(props.subscription.SubscriptionId);
    }, []);

    const loadSubscriptionDetail = (subscriptionId: string) => {
        fetch(window.BASE_URL + '/gallery/applications/newapp/subscriptions/' + subscriptionId, {
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
    return (
        <div>
            <Text block variant={'medium'} style={{marginTop:'10px'}}>Owners:</Text>
            {subscriptionDetail?.Owners.map((owner,index) =>
            {   
                return (
                    <div>
                <Text block variant='medium'>{owner.UserName}</Text>
                <Text block variant='medium'>{owner.UserId}</Text>
               </div>
                )

            })}
            <Link style={{marginTop:'50px'}}>+ New Owner</Link>
        </div>
    )
}

export default MySubscriptionOwnersDetails;