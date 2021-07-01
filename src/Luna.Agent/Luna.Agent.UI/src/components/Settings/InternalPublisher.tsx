import * as React from 'react';
import { useHistory, useLocation } from 'react-router';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton, CommandButton, MessageBar, MessageBarType, Icon } from '@fluentui/react';
import { IInternalPublisher } from '../AIServices/ISettings';

export interface IInternalPublisherProps {
    internalPublisherData: IInternalPublisher;
    closeInternalPublisherPannel: any;
    loadData: any;
}

const InternalPublisher = (props: IInternalPublisherProps) => {
    const [isInternalPublisherPanelOpen, toggleInternalPublisherPanel] = React.useState(false);
    const [isHidePublisherKey, setHidePublisherKey] = React.useState<boolean>(true);
    const [internalPublisherDetails, setInternalPublisherDetails] = React.useState<IInternalPublisher>();
    const [pubName, setPubName] = React.useState<string>();
    const [pubEndpointURL, setPubEndpointURL] = React.useState<string>();
    const [pubKey, setPubKey] = React.useState<string>();
    const [pubNotes, setPubNotes] = React.useState<string>();
    const [operation, setOperation] = React.useState<string>();
    const [errorMessage, setErrorMessage] = React.useState<string>();

    const history = useHistory();
    const hideShowPublisherKey = () => {
        isHidePublisherKey ? setHidePublisherKey(false) : setHidePublisherKey(true);
    }
    const addUpdatePublisher = () => {
        var addPublisherModel = { Name: pubName, DisplayName: pubName, Type: "Internal", Description: pubNotes, endpointUrl: pubEndpointURL, websiteUrl: pubEndpointURL, isEnabled: true, publisherKey: pubKey };
        if (operation === "Register") {
            fetch(window.BASE_URL + '/gallery/applicationpublishers/' + pubName, {
                mode: "cors",
                method: "PUT",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Luna-User-Id': 'test-admin',
                    'Host': 'lunatest-gateway.azurewebsites.net'
                },
                body: JSON.stringify(addPublisherModel),
            })
                .then(response => response.json())
                .then(_data => {                    
                    props.closeInternalPublisherPannel();
                    props.loadData();
                    history.push("settings");
                });
        }
        else if (operation === "Update") {
            var updatePublisherModel = {
                Name: pubName === undefined ? props.internalPublisherData.Name : pubName,
                DisplayName: pubName === undefined ? props.internalPublisherData.Name : pubName,
                Type: "Internal",
                Description: pubNotes === undefined ? props.internalPublisherData.Description : pubNotes,
                endpointUrl: pubEndpointURL === undefined ? props.internalPublisherData.EndpointUrl : pubEndpointURL,
                websiteUrl: pubEndpointURL === undefined ? props.internalPublisherData.EndpointUrl : pubEndpointURL,
                isEnabled: true,
                publisherKey: pubKey === undefined ? props.internalPublisherData.PublisherKey : pubKey
            };
            fetch(window.BASE_URL + '/gallery/applicationpublishers/' + props.internalPublisherData.Name, {
                mode: "cors",
                method: "PATCH",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Luna-User-Id': 'test-admin',
                    'Host': 'lunatest-gateway.azurewebsites.net'
                },
                body: JSON.stringify(updatePublisherModel),
            })
                .then(response => response.json())
                .then(_data => {
                    props.closeInternalPublisherPannel();
                    props.loadData();
                    history.push("settings");
                });
        }
    }
    const setNameValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setPubName(newValue || '');
    }
    const setEndpointValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setPubEndpointURL(newValue || '');
    }
    const setKeyValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setPubKey(newValue || '');
    }
    const setNotesValue = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setPubNotes(newValue || '');
    }
    React.useEffect(() => {
        if (props.internalPublisherData.Name === '') {
            setOperation("Register");
        }
        else
            setOperation("Update");
    }, []);
    console.log(props);
    return (
        <React.Fragment>
            <Stack style={{ height: '510px' }}>
                <Stack style={{ width: '263px' }}>
                    <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }}>Publisher Name:</Text>
                    <TextField
                        value={pubName}
                        defaultValue={props.internalPublisherData.Name}
                        onChange={setNameValue} />
                </Stack>
                <Stack style={{ width: '263px' }}>
                    <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }}>Endpoint URL:</Text>
                    <TextField
                        defaultValue={props.internalPublisherData.EndpointUrl}
                        value={pubEndpointURL}
                        onChange={setEndpointValue}
                    />
                </Stack>
                <Stack style={{ width: '263px' }}>
                    <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }}>Publisher Key:</Text>
                    <Stack horizontal verticalAlign="baseline">
                        <TextField
                            style={{ width: '227px' }}
                            value={pubKey}
                            defaultValue={props.internalPublisherData.PublisherKey}
                            type={isHidePublisherKey ? "password" : "text"}
                            canRevealPassword={true}
                            onChange={setKeyValue}
                        />
                        {/* <CommandButton onClick={() => hideShowPublisherKey()} iconProps={{ iconName: 'RedEye' }} title={"Show Password"} /> */}
                    </Stack>
                </Stack>
                <Stack style={{ width: '260px' }}>
                    <Text block variant={'medium'} style={{ fontWeight: 600, marginTop: '10px' }}>Notes:</Text>
                    <TextField
                        multiline={true}
                        defaultValue={props.internalPublisherData.Description}
                        value={pubNotes}
                        onChange={setNotesValue}
                    />
                </Stack>
            </Stack>
            <Stack horizontal>
                <PrimaryButton style={{ marginLeft: '120px' }} onClick={addUpdatePublisher} >{operation}</PrimaryButton>
                <div>
                    <DefaultButton style={{ marginLeft: '20px' }} onClick={() => { props.closeInternalPublisherPannel(); }}>Close</DefaultButton>
                </div>
            </Stack>
        </React.Fragment>
    )
}
export default InternalPublisher;