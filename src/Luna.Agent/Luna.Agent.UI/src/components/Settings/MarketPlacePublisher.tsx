import * as React from 'react';
import { useHistory, useLocation } from 'react-router';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter, Pivot, PivotItem, Label, IconButton, CommandButton, MessageBar, MessageBarType, Icon } from '@fluentui/react';
import { SharedColors } from '@uifabric/fluent-theme';
import { IMarketPlacePublisher } from '../AIServices/ISettings'
import { UpdateMarketPlacePublisher } from './GetPublisher';
import { Spinner, SpinnerSize } from '@fluentui/react';

export interface IMarketPlacePublisherProps {
    marketPlacePublisher: IMarketPlacePublisher[];
    Closepannel: any
}
const MarketPlacePublisher = (props: IMarketPlacePublisherProps) => {

    const history = useHistory();
    const [marketPlacePublisher, setMarketPlacePublisher] = React.useState<IMarketPlacePublisher[]>(props.marketPlacePublisher);
    const [marketPlacePublisherUpdate, setMarketPlacePublisherUpdate] = React.useState<IMarketPlacePublisher[]>([]);
    const [isLoading, setIsLoading] = React.useState<boolean>(false);
    const [isDisabled, setIsDisabled] = React.useState<boolean>(false);

    React.useEffect(() => {
    }, []);


    const onchkChange = (cntrlId: any, values: IMarketPlacePublisher, event: any) => {
        let data: IMarketPlacePublisher[] = [];
        values.IsEnabled = values.IsEnabled ? false : true;
        if (marketPlacePublisher.filter(x => x.Name != values.Name).length > 0) {
            marketPlacePublisherUpdate.push(values);
        }
        marketPlacePublisher.forEach(element => {
            element.IsEnabled = event.target.checked;
            data.push(element);
        });
        setMarketPlacePublisher(data);
    }

    const onUpdate = () => {
        setIsDisabled(true);
        setIsLoading(true);
        marketPlacePublisherUpdate.forEach(async element => {
            
            // await UpdateMarketPlacePublisher(element);
        });
        setIsLoading(false);
        setIsDisabled(false);
    }

    return (
        <React.Fragment>
            <Stack horizontal horizontalAlign="space-between" verticalAlign="center" className="pannelsubtitle">
                <StackItem>
                    <Text block variant={'xLarge'}>Publisher</Text>
                </StackItem>
            </Stack>
            <Stack className="pannelcontent">
                {
                    isLoading ?
                        <div style={{ marginTop: '40px' }}>
                            <Spinner size={SpinnerSize.large} label="Loading data..." className="pannelspinner" />
                        </div> :
                        <React.Fragment>
                            <table cellSpacing={0} cellPadding={0} className="marketPublisherGrid pannel">
                                <thead>
                                    <tr>
                                        <th>
                                            Publisher Name
                                        </th>
                                        <th>
                                            EndPoint URL
                                        </th>
                                        <th>
                                            Enabled
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {

                                        marketPlacePublisher && marketPlacePublisher?.length > 0 ?
                                            marketPlacePublisher?.map((values: IMarketPlacePublisher, idx: number) => {
                                                return (
                                                    <tr key={idx}>
                                                        <td>
                                                            {values.DisplayName}
                                                        </td>
                                                        <td>
                                                            {values.EndpointUrl}
                                                        </td>
                                                        <td>
                                                            <input type="checkbox"
                                                                id={'chkenabled' + idx.toString()} onChange={(event) => onchkChange('chkenabled' + idx.toString(), values, event)}
                                                                checked={values.IsEnabled} />
                                                        </td>
                                                    </tr>
                                                )
                                            })
                                            :
                                            <tr>
                                                <td colSpan={3} style={{ textAlign: 'center' }}>
                                                    <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! No Internal Publishers found.</Text>
                                                </td>
                                            </tr>
                                    }
                                </tbody>
                            </table>
                        </React.Fragment>
                }
            </Stack>
            <Stack horizontal style={{ marginTop: '30px', float: 'right' }}>
                <PrimaryButton disabled={isDisabled} text="Update" onClick={(event) => { onUpdate() }}></PrimaryButton>         &nbsp;&nbsp;
                <DefaultButton disabled={isDisabled} onClick={() => props.Closepannel()}>Cancel</DefaultButton>
            </Stack>
        </React.Fragment>
    )
}

export default MarketPlacePublisher;
