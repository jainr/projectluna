// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import FooterLinks from '../FooterLinks/FooterLinks';
import { Stack, Text, Link, Image, StackItem, TextField, ImageFit } from '@fluentui/react';
import { PanelStyles } from '../../helpers/PanelStyles';
import { getTheme } from '@fluentui/react';
import { GetInternalOffers } from './GetInternalOffers';
import { GetMarketplaceOffers } from './GetMarketplaceOffers';

import './../AIServices/AIServices.css';


const theme = getTheme();

const AIServices = () => {
  const [offerData, setOfferData] = React.useState<any[]>();
  const [initOffferData, setInitOfferData] = React.useState<any[]>();

  React.useEffect(() => {
      loadData();
  }, []);

  /**
   * Load data and merge all offer types into one object.
   */
  const loadData = async () => {
    let dataSet = [];
    let internalOffers = await GetInternalOffers();
    let marketplaceOffers = await GetMarketplaceOffers();

    for (const key in internalOffers) {
      if (Object.prototype.hasOwnProperty.call(internalOffers, key)) {
        const element = internalOffers[key];
        dataSet.push(element);
      }
    }

    for (const key in marketplaceOffers) {
      if (Object.prototype.hasOwnProperty.call(marketplaceOffers, key)) {
        const element = marketplaceOffers[key];
        dataSet.push(element);
      }
    }
    setInitOfferData(dataSet);
    setOfferData(dataSet);
  }

  const searchFilter = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
    setOfferData(filterByValue(initOffferData!, newValue!));
  }

  return (
    <div className="AIServices">
      <div style={PanelStyles}>
        <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
          <StackItem>
            <Text block variant={'xLargePlus'}>AI Services</Text>
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
          <Text block variant={'xLarge'}>Azure Marketplace Offers</Text>
          <br />
        <Stack wrap={true} horizontal tokens={{ childrenGap: 20 }}>
            {
              offerData?.map((offer, index) => 
              <div 
                  key={index}
                  style={{ 
                  display: offer.OfferType === 'Marketplace' ? 'block' : 'none',
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
                      alt={offer.OfferName}
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
                      src={process.env.PUBLIC_URL + '/defaultlogo.png'}
                      alt={offer.OfferName}
                      maximizeFrame={true}
                      width={100}
                      height={100}
                      loading="lazy"
                    />
                  }
                  {
                    offer.LogoImageUrl !== 'https://lalalal' && offer.LogoImageUrl !== '' &&
                    <Image 
                      imageFit={ImageFit.contain}
                      src={offer.LogoImageUrl}
                      alt={offer.OfferName}
                      maximizeFrame={true}
                      width={100}
                      height={100}
                      loading="lazy"
                    />
                  }
                  <Text block variant={'xLarge'} 
                  title={offer.OfferName}
                  style={{ overflow: "hidden", whiteSpace: 'nowrap', textOverflow: 'ellipsis'}}>{offer.OfferName}</Text>
                  <Text block variant={'medium'}
                  title={offer.PublisherName}
                  className="ellipsis">{offer.PublisherName}</Text>
                  <Text block variant={'small'} className="block-ellipsis" title={offer.Description}>{offer.Description}</Text>
                  
                  </Stack>
                  <Stack tokens={{ childrenGap: 40 }}  horizontal horizontalAlign="space-between" style={{ marginTop: '10px' }}>
                    {
                      offer.DocumentationUrl !== null &&
                      <Link href={offer.DocumentationUrl} target="_blank" hrefLang="en-us">Learn More</Link>
                    }
                    {
                      offer.SubscribePageUrl !== null &&
                      <Link href={offer.SubscribePageUrl} target="_blank" hrefLang="en-us">Subscribe</Link>
                    }
                  </Stack>
               </div>
              )}
          </Stack>
      </div>
      <br />
      <div style={PanelStyles}>
        <Text block variant={'xLarge'}>Internal Offers</Text>
          <br />
        <Stack wrap={true} horizontal tokens={{ childrenGap: 20 }}>
            
        {
              offerData?.map((offer, index) => 
              <div 
                  key={index}
                  style={{ 
                  display: offer.OfferType === 'Internal' ? 'block' : 'none',
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
                      alt={offer.OfferName}
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
                      alt={offer.OfferName}
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
                      alt={offer.OfferName}
                      maximizeFrame={true}
                      width={100}
                      height={100}
                      loading="lazy"
                    />
                  }
                  <Text block variant={'xLarge'} 
                  title={offer.OfferName}
                  style={{ overflow: "hidden", whiteSpace: 'nowrap', textOverflow: 'ellipsis'}}>{offer.OfferName}</Text>
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
                      <Link href={offer.SubscribePageUrl} target="_blank" hrefLang="en-us">Subscribe</Link>
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

export default AIServices;
