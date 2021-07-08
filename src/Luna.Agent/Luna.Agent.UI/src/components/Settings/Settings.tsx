import * as React from 'react';
import { CommandButton, DefaultButton, Panel, PanelType, PrimaryButton, Stack, StackItem, Text, TextField } from '@fluentui/react';
import FooterLinks from '../FooterLinks/FooterLinks';
import { LinkedServices } from './Settings.LinkedServices';
import { Administration } from './Settings.Administration';
import { AgentProperties } from './Settings.AgentProperties';
import { Publishers } from './Settings.Publishers';
import { PanelStyles } from '../../helpers/PanelStyles';
import { IInternalPublisher, IMarketPlacePublisher } from '../AIServices/ISettings'
import { GetInternalPublisher, GetMarketPlacePublisher } from '../Settings/GetPublisher'
import Moment from 'react-moment';
import { useBoolean } from '@uifabric/react-hooks';

import MarketPlacePublisher from './MarketPlacePublisher'
import InternalPublisher from './InternalPublisher'

/** @component Settings view and code including PivotView for subpages. */
function Settings() {

  const [isInternalPublishiingLoading, setIsInternalPublishiingLoading] = React.useState<boolean>(true);
  const [isMarketPublishiingLoading, setIsMarketPublishiingLoading] = React.useState<boolean>(true);
  const [internalPublisher, setInternalPublisher] = React.useState<IInternalPublisher[]>();
  const [marketPlacePublisher, setMarketPlacePublisher] = React.useState<IMarketPlacePublisher[]>();
  const [isOpen, { setTrue: openInternalPublisherPanel, setFalse: dismissInternalPublisherPanel }] = useBoolean(false);
  const [isMarketOpen, { setTrue: openMarketPublisherPanel, setFalse: dismissMarketPublisherPanel }] = useBoolean(false);
  const [isInternalPublisherPanelOpen, toggleInternalPublisherPanel] = React.useState(false);
  const [isHidePublisherKey, setHidePublisherKey] = React.useState<boolean>(true);
  const [primaryKey, setPrimaryKey] = React.useState<string>();
  const [inernalPublisherHeader, setInernalPublisherHeader] = React.useState<string>();
  const [internalPublisherDetails, setInternalPublisherDetails] = React.useState<IInternalPublisher>();

  React.useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    await LoadInternalPublisher();
    await LoadMarketPublisher();
  }

  const LoadInternalPublisher = async () => {
    setIsInternalPublishiingLoading(true);
    let iInternalPublisher: IInternalPublisher[] = [];
    let interPublishers = await GetInternalPublisher();
    for (const key in interPublishers) {
      if (Object.prototype.hasOwnProperty.call(interPublishers, key)) {
        const element = interPublishers[key];
        iInternalPublisher.push(element);
      }
    }
    setInternalPublisher(iInternalPublisher);
    setIsInternalPublishiingLoading(false);
  }

  const LoadMarketPublisher = async () => {
    setIsMarketPublishiingLoading(true);
    let iIMarketPlacePublisher: IMarketPlacePublisher[] = [];
    let marketPlacePublisher = await GetMarketPlacePublisher();
    for (const key in marketPlacePublisher) {
      if (Object.prototype.hasOwnProperty.call(marketPlacePublisher, key)) {
        const element = marketPlacePublisher[key];
        iIMarketPlacePublisher.push(element);
      }
    }
    setMarketPlacePublisher(iIMarketPlacePublisher);
    setIsMarketPublishiingLoading(false);
  }

  const selectInternalPublication = (selectedInternalPublisher: IInternalPublisher, operation: string) => {
    openInternalPublisherPanel();
    if (operation === "Add") {
      // let iInternalPublisher: IInternalPublisher = {} as IInternalPublisher;
       const  internalPublisher = (): IInternalPublisher => ({
        CreatedTime: '',
        Description: '',
        DisplayName: '',
        EndpointUrl: '',
        IsEnabled: '',
        LastUpdatedTime: '',
        Name: '',
        PublisherKey: '',
        Type: '',
        WebsiteUrl: '',
       });    
      setInternalPublisherDetails(internalPublisher);
    }
    else if ("Update") {

      setInternalPublisherDetails(selectedInternalPublisher);
    }
  }

  const selectMarketPublication = (selectedMarketPublisher: IMarketPlacePublisher) => {
    openMarketPublisherPanel();
  }

  const OpenInternalPublisherPannel = () => {
    openInternalPublisherPanel();
  }

  const closeInternalPublisherPannel = () => {
    dismissInternalPublisherPanel();
  }
  const hideShowPrimaryKey = () => {
    isHidePublisherKey ? setHidePublisherKey(false) : setHidePublisherKey(true);
  }
  const onRenderFooterContent = React.useCallback(
    () => (
      <Stack horizontal>
        <PrimaryButton style={{ marginLeft: '120px' }} text="Save" ></PrimaryButton>
        <div>
          <DefaultButton style={{ marginLeft: '20px' }} onClick={() => { closeInternalPublisherPannel(); }}>Close</DefaultButton>
        </div>
      </Stack>
    ),
    [dismissInternalPublisherPanel],
  );

  return (
    <div className="settings">
      <div style={PanelStyles}>
        <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
          <StackItem>
            <Text block variant={'xLargePlus'}>Settings</Text>
          </StackItem>
        </Stack>
        <br />
        <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
          <StackItem>
            <Text block variant={'xLarge'}>Publisher</Text>
          </StackItem>
        </Stack>

        <Stack className="section">
          <hr style={{ width: "100%" }} />
          <Text block variant={'xLarge'} className="title">Internal Publishers
            <a className="anchor" onClick={() => {
              openInternalPublisherPanel();
              setInernalPublisherHeader("Register Internal Publisher"); selectInternalPublication(internalPublisherDetails!, "Add")
            }}>+ Register New</a>
          </Text>

          <React.Fragment>
            <table cellSpacing={0} cellPadding={0} className="internalPublisherGrid">
              <thead>
                <tr>
                  <th>
                    Publisher Name
                  </th>
                  <th>
                    EndPoint URL
                  </th>
                  <th>
                    Registered Date
                  </th>
                </tr>
              </thead>
              <tbody>
                {
                  isInternalPublishiingLoading ? <tr><td colSpan={3}>loading data.....</td></tr>
                    :
                    internalPublisher && internalPublisher?.length > 0 ?
                      internalPublisher?.map((values: IInternalPublisher, idx: number) => {
                        return (
                          <tr key={idx}>
                            <td>
                              <a onClick={(event) => { selectInternalPublication(values, "Update"); setInernalPublisherHeader("Internal Publisher"); }} className="anchor">
                                {values.DisplayName}
                              </a>
                            </td>
                            <td>
                              {values.EndpointUrl}
                            </td>
                            <td>
                              <Moment format="MM/DD/yyyy">
                                {values.CreatedTime}
                              </Moment>
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
        </Stack>
        <Stack className="section">
          <hr style={{ width: "100%" }} />
          <Text block variant={'xLarge'} className="title">Azure Market Publishers
            <a className="anchor" onClick={openMarketPublisherPanel}>+ Manage</a>
          </Text>

          <React.Fragment>
            <table cellSpacing={0} cellPadding={0} className="marketPublisherGrid">
              <thead>
                <tr>
                  <th>
                    Publisher Name
                  </th>
                  <th>
                    EndPoint URL
                  </th>
                  <th>
                    Registered Date
                  </th>
                </tr>
              </thead>
              <tbody>
                {
                  isMarketPublishiingLoading ? <tr><td colSpan={3}>loading data.....</td></tr>
                    :
                    marketPlacePublisher && marketPlacePublisher?.length > 0 ?
                      marketPlacePublisher?.map((values: IMarketPlacePublisher, idx: number) => {
                        return (
                          <tr key={idx}>
                            <td>
                              <a onClick={(event) => selectMarketPublication(values)} className="anchor">
                                {values.DisplayName}
                              </a>
                            </td>
                            <td>
                              {values.WebsiteUrl}
                            </td>
                            <td>
                              {
                                <Moment format="MM/DD/yyyy">
                                  {values.CreatedTime}
                                </Moment>
                              }
                            </td>
                          </tr>
                        )
                      })
                      :
                      <tr>
                        <td colSpan={3} style={{ textAlign: 'center' }}>
                          <Text block variant={'medium'} style={{ color: 'grey', fontWeight: 600, marginTop: '10px' }}> Oops! No Market Publishers found.</Text>
                        </td>
                      </tr>
                }
              </tbody>
            </table>
          </React.Fragment>
        </Stack>
      </div>

      <Panel
        headerText={inernalPublisherHeader}
        isOpen={isOpen}
        isFooterAtBottom={true}
        hasCloseButton={false}
        isBlocking={true}
      // onRenderFooterContent={onRenderFooterContent}      
      >
        {internalPublisherDetails ? <InternalPublisher closeInternalPublisherPannel={closeInternalPublisherPannel} internalPublisherData={internalPublisherDetails} loadData={loadData}/> : null}
      </Panel>
      <Panel
        headerText="Manage Azure Marketplace Publishers"
        isOpen={isMarketOpen}
        isFooterAtBottom={true}
        hasCloseButton={true}
        type={PanelType.custom}
        customWidth={"600px"}
        isBlocking={true}
        closeButtonAriaLabel="Cancel"
      >
        {marketPlacePublisher ? <MarketPlacePublisher Closepannel={dismissMarketPublisherPanel} marketPlacePublisher={marketPlacePublisher} /> : null}
      </Panel>
    </div>
  );
}

export default Settings;