// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { Stack, TextField, Text, CommandButton, MessageBar, MessageBarType, Label } from '@fluentui/react';
import * as React from 'react';
import { PanelStyles } from '../../helpers/PanelStyles';

/** @component Agent Properties view and code. */
export const AgentProperties = () => {
    const [isCopySuccess, setSuccess] = React.useState<boolean>(false);
    const [agentProperties, setAgentProperties] = React.useState<IAgentProperties>();


  const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
      
  const loadData = () => {
      fetch(window.BASE_URL + '/agentinfo', {
          mode: "cors",
          method: "GET",
          headers: {
              'Authorization': bearerToken,
              'Accept': 'application/json',
              'Content-Type': 'application/json'
          }
      })
          .then(response => response.json())
          .then(agentData => setAgentProperties(agentData));
  }

  React.useEffect(() => {
    loadData();
}, [loadData]);

    const renderLabel = (event: any): JSX.Element  => {
        const copyClick = () => {
          const urlValue = event?.value; //TODO: Assign value
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
          setSuccess(true);
  
          setTimeout(() => {
            setSuccess(false);
          }, 3000);
        }
        return(
          <Stack tokens={{ childrenGap: 10 }}>
            <Stack horizontal verticalAlign="baseline" horizontalAlign="space-between">
            <Label>{event.label}</Label>
            
            <CommandButton onClick={copyClick} iconProps={{ iconName: 'Copy' }}>Copy</CommandButton>
          </Stack>
          </Stack>
        )
      }

    return (
        <div style={PanelStyles}>
          <Text variant={'xLargePlus'}>Agent Properties</Text>
            <br />
            <br />
            
            <Stack 
            tokens={{
              maxWidth: '50%'
            }}
            style={{ 
                display: isCopySuccess ? "block" : 'none' }}>
              <MessageBar messageBarType={MessageBarType.success}>Copied!</MessageBar>
            </Stack>
          <Stack
            tokens={{ childrenGap: 15, maxWidth: "50%" }}
          >
            <TextField
              disabled={true}
              label="Agent ID"
              readOnly={true}
              value={agentProperties?.AgentId}
              onRenderLabel={renderLabel}
            />
            <TextField
              disabled={true}
              label="Agent Key"
              readOnly={true}
              value={agentProperties?.AgentKey}
              onRenderLabel={renderLabel}
            />
            <TextField
              disabled={true}
              label="Agent API Endpoint"
              readOnly={true}
              value={agentProperties?.AgentAPIEndpoint}
              onRenderLabel={renderLabel}
            />
            <TextField
              disabled={true}
              label="Agent API Connection String"
              readOnly={true}
              value={agentProperties?.AgentAPIConnectionString}
              onRenderLabel={renderLabel}
            />
          </Stack>
        </div>
    );
}

// Interfaces
interface IAgentProperties {
  AgentAPIConnectionString: string;
  AgentAPIEndpoint: string;
  AgentId: string;
  AgentKey: string;
}