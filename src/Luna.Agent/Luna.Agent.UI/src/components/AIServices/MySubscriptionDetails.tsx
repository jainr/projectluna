import * as React from 'react';
import { useHistory, useLocation } from 'react-router';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton, CommandButton, MessageBar, MessageBarType } from '@fluentui/react';
import { IApplicationSubscription } from './IApplicationSubscription';


const MySubscriptionDetails = (props: any) => {

    const [isBaseUrlCopySuccess, setBaseUrlCopySuccess] = React.useState<boolean>(false);
    const [isPrimaryKeyCopySuccess, setPrimaryKeyCopySuccess] = React.useState<boolean>(false);
    const [isSecondaryKeyCopySuccess, setSecondaryKeyCopySuccess] = React.useState<boolean>(false);
    const [isNotesUpdateSuccess, setNotesUpdateSuccess] = React.useState<boolean>(false);
    const [isHidePrimaryKey,setHidePrimaryKey] = React.useState<boolean>(true);
    const [isHideSecondaryKey,setHideSecondaryKey] = React.useState<boolean>(true);
    const [primaryKey,setPrimaryKey]= React.useState<string>();
    const [secondaryKey,setSecondaryKey]= React.useState<string>();
    const [subNotes,setNotes]= React.useState<string>();
    const [subscriptionDetail,setSubscriptionDetail] = React.useState<IApplicationSubscription>();

    // const {togglePanel} = props;
    // const history = useHistory();
    console.log(props);
    // props.subscription.BaseUrl = "https://luna.windows.net/api";
    // props.subscription.PrimaryKey = "https://luna.windows.net/api";
    // props.subscription.SecondaryKey = "https://luna.windows.net/api";
    React.useEffect(() => {
        loadSubscriptionDetail(props.subscription.SubscriptionId);       
    }, []);

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
    const hideShowPrimaryKey= () =>{        
        isHidePrimaryKey ? setHidePrimaryKey(false) : setHidePrimaryKey(true) ;
    }
    const hideShowSecondaryKey= () =>{        
        isHideSecondaryKey ? setHideSecondaryKey(false) : setHideSecondaryKey(true) ;
    }
    const setNotesValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setNotes(newValue || '');
    }
    const updateNotes = () => {
        var updatedNotes = { notes: subNotes };
        console.log(JSON.stringify(updatedNotes))
        fetch(window.BASE_URL + '/gallery/applications/lunanlp/subscriptions/'+subscriptionDetail?.SubscriptionId+'/updatenotes', {
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
        fetch(window.BASE_URL + '/gallery/applications/lunanlp/subscriptions/'+ subscriptionId, {
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
        fetch(window.BASE_URL + '/gallery/applications/lunanlp/subscriptions/'+props.subscription.SubscriptionId+'/regeneratekey?key-name='+keyType, {
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
                    style={{height:'25px',width:'180px'}}
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
                    style={{height:'25px',width:'180px'}}
                    disabled={true}
                    value={primaryKey}
                    type ={isHidePrimaryKey ? "password" :"text"}/>                   
                    <CommandButton onClick={() => copyClick("PrimaryKey", primaryKey)} iconProps={{ iconName: 'Copy' }} title={"Copy"} />
                    <CommandButton onClick={()=> hideShowPrimaryKey()} iconProps={{ iconName: 'RedEye' }} title={"Show Password"} />
                    <CommandButton onClick={()=> regenerateSubscriptionKey("PrimaryKey")} iconProps={{ iconName: 'Refresh' }} title={"Regenerate Key"} />
                </Stack>

                <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }} title={"Secondary Key"}>Secondary Key:</Text>
                <Stack style={{ display: isSecondaryKeyCopySuccess ? "block" : 'none' }}>
                    <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
                </Stack>
                <Stack horizontal verticalAlign="baseline">
                    <input 
                     style={{height:'25px',width:'180px'}}
                    disabled={true}
                    value={secondaryKey}
                    type ={isHideSecondaryKey ? "password" :"text"}/>  
                    <CommandButton onClick={() => copyClick("SecondaryKey", secondaryKey)} iconProps={{ iconName: 'Copy' }} title={"Copy"} />
                    <CommandButton onClick={()=> hideShowSecondaryKey()} iconProps={{ iconName: 'RedEye' }} title={"Show Password"} />
                    <CommandButton onClick={()=> regenerateSubscriptionKey("SecondaryKey")} iconProps={{ iconName: 'Refresh' }} title={"Regenerate Key"} />
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
            {/* <DefaultButton onClick={() => props.toggle(true)}>Open second panel</DefaultButton> */}
        </div>
    )
}

export default MySubscriptionDetails;
