// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import { IPersonaSharedProps, Persona, PersonaSize, PersonaPresence, Stack } from '@fluentui/react';
import '../SiteHeader/SiteHeader.css';

interface ISiteHeaderProps {
  userDisplayName?: string;
  userLocation?: string;
  userPhotoDataStr?: string;
}

const HeaderLabelStyles: React.CSSProperties = {
  font: "normal normal bold 16px/21px 'Segoe UI', sans-serif",
  letterSpacing: '0px',
  color: '#FFFFFF',
  textRendering: 'optimizeLegibility'
}

/** @component Displays the site header. */
function SiteHeader(props: ISiteHeaderProps) {
  const [renderDetails] = React.useState(true);

  const examplePersona: IPersonaSharedProps = {
    text: props.userDisplayName,
    imageAlt: props.userDisplayName,
    secondaryText: props.userLocation,
    imageUrl: props.userPhotoDataStr
  };

  return (
    <div className="SiteHeader"
      style={{ backgroundColor: window.HEADER_HEX_COLOR }}
    >
      <div className="left">
        <Stack horizontal tokens={{ childrenGap: 10 }}>
          <a href="/#/">
            <img src={process.env.PUBLIC_URL + '/aigent.svg'} alt={window.SITE_TITLE} className="logo" />
          </a>
          <span style={HeaderLabelStyles}>{window.SITE_TITLE}</span>
        </Stack>
      </div>
      <div className="right">
        <Persona
          {...examplePersona}
          size={PersonaSize.size24}
          presence={PersonaPresence.none}
          hidePersonaDetails={!renderDetails}
        />
      </div>
    </div>
  );
}

export default SiteHeader;
