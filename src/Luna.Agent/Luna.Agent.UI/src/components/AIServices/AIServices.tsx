// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import FooterLinks from '../FooterLinks/FooterLinks';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit, Panel, DefaultButton, PrimaryButton, Separator, Dropdown, IDropdownOption, Dialog, DialogType, IModalProps, IDialogContentProps, DialogFooter } from '@fluentui/react';
import { PanelStyles } from '../../helpers/PanelStyles';
import { getTheme } from '@fluentui/react';
import { GetInternalOffers } from './GetInternalOffers';
import { SharedColors } from '@uifabric/fluent-theme';
import { GetMarketplaceOffers } from './GetMarketplaceOffers';
import { useBoolean } from '@uifabric/react-hooks';
import { ISubscription } from '../../interfaces/ISubscription';

import './../AIServices/AIServices.css';
import { getTypeParameterOwner } from 'typescript';
import { PromptState } from 'msal/lib-commonjs/utils/Constants';
import { withRouter } from "react-router-dom";
import { useHistory, useLocation } from 'react-router';

function generateUUID() { // Public Domain/MIT
  var d = new Date().getTime();//Timestamp
  var d2 = (performance && performance.now && (performance.now()*1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      var r = Math.random() * 16;//random number between 0 and 16
      if(d > 0){//Use timestamp until depleted
          r = (d + r)%16 | 0;
          d = Math.floor(d/16);
      } else {//Use microseconds since page-load if supported
          r = (d2 + r)%16 | 0;
          d2 = Math.floor(d2/16);
      }
      return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
  });
}

const theme = getTheme();

const AIServices = () => {
  const history = useHistory();
  const [offerData, setOfferData] = React.useState<any[]>();
  const [initOffferData, setInitOfferData] = React.useState<any[]>();
  const [isOpen, { setTrue: openPanel, setFalse: dismissPanel }] = useBoolean(false);
  const [isNewSubSuccessful, setIsNewSubSuccessful] = React.useState(false);
  const [isDataLoading, setIsDataLoading] = React.useState(true);
  const [planOptions, setPlanOptions] = React.useState<IDropdownOption[]>([]);
  const [hideNewSubDialog, setHideNewSubDialog] = React.useState(true);

  const [newSubscription, setNewSubscription] = React.useState<ISubscription>({
    OfferDisplayName: '',
    OfferName: '',
    PlanName: '',
    Name: '',
    SubscriptionId: '',
    Owner: ''
  });

  React.useEffect(() => {
      loadData();
  }, []);

  /**
   * Load data and merge all offer types into one object.
   */
  const loadData = async () => {
    let dataSet = [];
    let internalOffers = await GetInternalOffers();
    //let marketplaceOffers = await GetMarketplaceOffers();

    for (const key in internalOffers) {
      if (Object.prototype.hasOwnProperty.call(internalOffers, key)) {
        const element = internalOffers[key];
        dataSet.push(element);
      }
    }
/*
    for (const key in marketplaceOffers) {
      if (Object.prototype.hasOwnProperty.call(marketplaceOffers, key)) {
        const element = marketplaceOffers[key];
        dataSet.push(element);
      }
    }
*/
    setInitOfferData(dataSet);
    setOfferData(dataSet);
  }

  const searchFilter = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
    setOfferData(filterByValue(initOffferData!, newValue!));
  }

  const buttonStyles = { root: { marginRight: 8 } };
  
  const CreateSubscription = () => {
    setIsDataLoading(true);
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
    const newSub = JSON.parse(sessionStorage.getItem('newSub')!);
    fetch(`${window.BASE_URL}/subscriptions/${newSub.SubscriptionId}`, {
        mode: "cors",
        method: "PUT",
        headers: {
            'Authorization': bearerToken,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newSub)
    })
        .then(response => {
            if (response.status === 200 || 202) {
              setIsNewSubSuccessful(true);
                loadData();
                setTimeout(() => {
                  setIsNewSubSuccessful(false);
                }, 7000);
                dismissPanel();
                setHideNewSubDialog(false);
            } else {
                window.alert(`Error creating new subscription. - ${response.status}`);
            }
            return response.json();
        }).finally(() => setIsDataLoading(true));
  }
  
  const onRenderFooterContent = React.useCallback(
    () => (
        <div>
            <PrimaryButton
                onClick={CreateSubscription} styles={buttonStyles}>
                Submit
      </PrimaryButton>
            <DefaultButton onClick={dismissPanel}>Cancel</DefaultButton>
        </div>
    ),
    [dismissPanel],
  );

  const dialogDeleteUserContentProps: IDialogContentProps = {
    type: DialogType.normal,
    title: 'New Subscription is being created',
    closeButtonAriaLabel: 'Close',
    isMultiline: true,
    subText: `New subscription ${newSubscription.SubscriptionId} is being created.`,
  };

  const modalProps: IModalProps = {
    titleAriaId: 'titleId',
    subtitleAriaId: 'subtitleId',
    isBlocking: true,
    isDarkOverlay: true,
    allowTouchBodyScroll: true
  }

  const setSubscriptionsPageActive = () => {
    for (const key in document.getElementsByClassName('nav-item')) {
      if (Object.prototype.hasOwnProperty.call(document.getElementsByClassName('nav-item'), key)) {
        const liElement = document.getElementsByClassName('nav-item')[key] as HTMLLIElement;
        if (key === "1")
        {
          liElement.classList.add('active');
        }
        else
        {
          liElement.classList.remove('active');
        }
      }
    }
  }

  return (
    <div className="AIServices">
      <div style={PanelStyles}>
        <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
          <StackItem>
            <Text block variant={'xLargePlus'}>Machine Learning Gallery</Text>
          </StackItem>
          <StackItem>
            <TextField
              underlined
              autoComplete="false"
              autoCorrect="false"
              autoFocus={true}
              iconProps={{ iconName: 'Search' }}
              onChange={searchFilter}
              label="Search"
            ></TextField>
          </StackItem>
        </Stack>
        </div>
        <br />
        

      <div style={PanelStyles}>
        <p style={{ display: offerData && offerData.length >= 1 ? "none" : "block" }}>
            <Text variant={'medium'}>Loading Available Offers...</Text>
        </p>
        <Dialog
        hidden={hideNewSubDialog}
        onDismiss={() => setHideNewSubDialog(true)}
        dialogContentProps={dialogDeleteUserContentProps}
        modalProps={modalProps}
      >
        <DialogFooter>
          <DefaultButton onClick={function(event){setSubscriptionsPageActive(); history.push('../#'); }} text="Go to my subscriptions" />
          <DefaultButton onClick={() => setHideNewSubDialog(true)} text="Close" />
        </DialogFooter>
      </Dialog>

        <Panel  
                isOpen={isOpen}
                onDismiss={dismissPanel}
                headerText="Subscribe new Service"
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
                        label="Subscription Id"
                        value={newSubscription.SubscriptionId}
                        readOnly={true}
                        disabled={true}
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        value={newSubscription.OfferDisplayName}
                        readOnly={true}
                        disabled={true}
                        label="Machine Learning Service Name"
                    ></TextField>
                    <TextField
                        required
                        autoComplete="off"
                        label="Subscription Name"
                        value={newSubscription.Name}
                        onChange={((event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
                            let sub = newSubscription;
                            sub = {
                              OfferName: sub.OfferName, 
                              OfferDisplayName: sub.OfferDisplayName,
                              PlanName: sub.PlanName,
                              Name: newValue!,
                              SubscriptionId: sub.SubscriptionId,
                              Owner: sub.Owner
                            };
                            setNewSubscription(sub);
                            sessionStorage.setItem('newSub', JSON.stringify(sub));
                        })}
                    ></TextField>
                    <Separator />
                </Stack>
            </Panel>
        <Stack wrap={true} horizontal tokens={{ childrenGap: 20 }}>
            
        {
              offerData?.map((offer, index) => 
              <div 
                  key={index}
                  style={{ 
                  display: offer.OfferType === 'internal' ? 'block' : 'none',
                  width: '15vw',
                  boxShadow: theme.effects.elevation8, 
                  padding: '10px 10px 20px' }}>
                  <Stack 
                  styles={{ root: { minHeight: '200px' }}}
                  tokens={{ childrenGap: 14 }}>
                  {
                    offer.LogoImageUrl === 'https://lalalal' &&
                    <Image 
                      imageFit={ImageFit.contain}
                      srcSet={`${process.env.PUBLIC_URL}/noimage.png 1x, ${process.env.PUBLIC_URL}/noimage@2x.png 2x`}
                      src={process.env.PUBLIC_URL + '/noimage.png'}
                      alt={offer.OfferDisplayName}
                      maximizeFrame={true}
                      width={100}
                      height={100}
                      loading="lazy"
                    />
                  }
                  {
                    offer.LogoImageUrl === '' &&
                    <Image 
                      imageFit={ImageFit.contain}
                      srcSet={`${process.env.PUBLIC_URL}/noimage.png 1x, ${process.env.PUBLIC_URL}/noimage@2x.png 2x`}
                      src={process.env.PUBLIC_URL + '/noimage.png'}
                      alt={offer.OfferDisplayName}
                      maximizeFrame={true}
                      width={100}
                      height={100}
                      loading="lazy"
                    />
                  }
                  {
                    offer.LogoImageUrl !== 'https://lalalal' && offer.LogoImageUrl !== ''  &&
                    <Image 
                      imageFit={ImageFit.contain}
                      src={offer.LogoImageUrl}
                      alt={offer.OfferDisplayName}
                      maximizeFrame={true}
                      width={100}
                      height={100}
                      loading="lazy"
                    />
                  }
                  <Text block variant={'xLarge'} 
                  title={offer.OfferDisplayName}
                  style={{ overflow: "hidden", whiteSpace: 'nowrap', textOverflow: 'ellipsis'}}>{offer.OfferDisplayName}</Text>
                  <Text block variant={'medium'}
                  title={offer.PublisherName}
                  className="ellipsis">{offer.PublisherName}</Text>
                  <Text block variant={'small'} className="block-ellipsis" title={offer.Description}>{offer.Description}</Text>
                  
                  </Stack>
                  <Stack tokens={{ childrenGap: 40 }} horizontal horizontalAlign="space-between" style={{ marginTop: '10px' }}>
                    {
                      offer.DocumentationUrl !== null &&
                      <Link href={offer.DocumentationUrl} target="_blank" hrefLang="en-us">Learn More</Link>
                    }
                    {
                      offer.SubscribePageUrl !== null &&
                      <Link onClick={function(event){ 
                        
                        let planOptions: IDropdownOption[] = [];
                        for (const plan in offer.Plans) {
                          //planOptions.push({"key": (plan as unknown as IPlan).PlanName, "text": (plan as unknown as IPlan).PlanDisplayName})
                          planOptions.push({"key": offer.Plans[plan].PlanName, "text": offer.Plans[plan].PlanDisplayName})
                        }
                        // setPlanOptions(planOptions);
                        setNewSubscription({
                        OfferName: offer.OfferName+'',
                        OfferDisplayName: offer.OfferDisplayName,
                        PlanName: 'default',
                        Name: '',
                        SubscriptionId: generateUUID(),
                        Owner: sessionStorage.getItem('_userEmail')+'',
                      });
                      
                      sessionStorage.setItem('newSub', JSON.stringify(newSubscription));
                      openPanel(); }} hrefLang="en-us">Subscribe</Link>
                    }
                  </Stack>
               </div>
              )}
          </Stack>
      </div>
      <FooterLinks />
    </div>
  );
}

function filterByValue(array: any[], value: string) {
  return array.filter((data) =>  JSON.stringify(data).toLowerCase().indexOf(value.toLowerCase()) !== -1);
}

export interface IPlan{
  PlanName: string;
  PlanDisplayName: string;
  Description: string;
}

export default AIServices;
