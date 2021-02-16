// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import { Stack, Dropdown, Text, IDropdownOption, PrimaryButton, BaseButton, Spinner, SpinnerSize, MessageBar, MessageBarType, Button, DropdownMenuItemType, Toggle } from '@fluentui/react';
import { ISubscriptionDetail } from './ISubscriptionDetail';

export const DefaultSettings: React.FunctionComponent<IDetailsProps> = (props) => {

  const id = props.subscriptionID;

  const [amlworkspaces, setAmlWorkSpaces] = React.useState<IAmlWorkSpaces | undefined>();
  const [computeClustersOptions, setComputeClustersOptions] = React.useState<IDropdownOption[]>([]);
  const [deploymentTargetTypesOptions, setDeploymentTargetTypesOptions] = React.useState<IDropdownOption[]>([]);
  const [deploymentClusterOptions, setDeploymentClusterOptions] = React.useState<IDropdownOption[]>([]);
  const [isDeploymentClusterDisabled, SetIsDeploymentClusterDisabled] = React.useState<boolean>(true);
  const [selectedValues, setSelectedValues] = React.useState<ISelectedItems>();
  const [isFormValid, setIsFormValid] = React.useState(false);
  const [isFormSubmitting, setIsFormSubmitting] = React.useState<boolean>(false);
  const [isSaved, setIsSaved] = React.useState<boolean>(false);
  const [isSavedError, setIsSavedError] = React.useState<boolean>(false);
  const [IsAdminEnabled, SetIsAdminEnabled] = React.useState<boolean>(false);


  // initial load
  React.useEffect(() => {
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
    
    fetch(window.BASE_URL + '/amlworkspaces/myamlworkspace',{
      mode: "cors",
      method: "GET",
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.json())
      .then(_response => {
        setAmlWorkSpaces(_response);
        setOptions(_response);
        return _response;
      })
      .then((_response2) => {
        // init selected values.
        setSelectedValues({
          amlworkspace: _response2.WorkspaceName,
          computeCluster: _response2.ComputeClusters[0],
          deployTargetType: _response2.DeploymentTargetTypes[0].displayName,
          deploymentCluster: _response2.DeploymentClusters[0]
        });
        SetIsDeploymentClusterDisabled(selectedValues?.deployTargetType === 'Azure Container Instances');
      });
  }, []);

  const setOptions = (_response: IAmlWorkSpaces) => {
    // compute
    let computeOptions: IDropdownOption[] = [];
    for (const key in _response.ComputeClusters) {
      if (Object.prototype.hasOwnProperty.call(_response.ComputeClusters, key)) {
        const element = _response.ComputeClusters[key];
        computeOptions.push({ "key": element.toLowerCase(), "text": element })
      }
    }
    setComputeClustersOptions(computeOptions);

    // target types
    let targetTypeOptions: IDropdownOption[] = [];
    for (const key in _response.DeploymentTargetTypes) {
      if (Object.prototype.hasOwnProperty.call(_response.DeploymentTargetTypes, key)) {
        const element = _response.DeploymentTargetTypes[key];
        targetTypeOptions.push({ "key": element.displayName.toLowerCase(), "text": element.displayName })
      }
    }
    setDeploymentTargetTypesOptions(targetTypeOptions);

    // target types
    let clusterOptions: IDropdownOption[] = [];
    for (const key in _response.DeploymentClusters) {
      if (Object.prototype.hasOwnProperty.call(_response.DeploymentClusters, key)) {
        const element = _response.DeploymentClusters[key];
        clusterOptions.push({ "key": element.toLowerCase(), "text": element })
      }
    }
    setDeploymentClusterOptions(clusterOptions);
  }

  const settingSubmit = async (event: React.MouseEvent<HTMLDivElement | HTMLAnchorElement | HTMLButtonElement | BaseButton | Button | HTMLSpanElement, MouseEvent>) => {
    setIsFormSubmitting(true);
    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

    try {
      await fetch(window.BASE_URL + '/subscriptions/' + id, {
        method: 'put',
        headers: { 
          'Authorization': bearerToken,
          'Accept': 'application/json',
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          "AMLWorkspaceComputeClusterName": selectedValues?.computeCluster,
          "AMLWorkspaceDeploymentClusterName": selectedValues?.deploymentCluster,
          "AMLWorkspaceDeploymentTargetType": selectedValues?.deployTargetType,
          "AMLWorkspaceName": selectedValues?.amlworkspace,
          "SubscriptionId": id
        })
      })
        .then(response => {
          setIsSaved(true);
          setIsFormSubmitting(false);
        });
    } catch (error) {
      setIsSavedError(true);
      console.error(error);
    }
  }

  const validateForm = () => {
    if (selectedValues?.computeCluster === "") { setIsFormValid(false); return false; }
    if (selectedValues?.deployTargetType === "") { setIsFormValid(false); return false; }
    if (selectedValues?.deploymentCluster === "") { setIsFormValid(false); return false; }

    setIsFormValid(true);
    return true;
  }

  React.useEffect(() => { validateForm(); }, [selectedValues]);

  return (
    <div>
      {
        amlworkspaces === undefined && 
        <div>
          <Spinner size={SpinnerSize.large} label="Loading..."  />
        </div>
      }
      {
        amlworkspaces !== undefined &&
        <div>
        <Text variant={'xLarge'}>Default Settings (Azure ML)</Text>
              <MessageBar 
                onDismiss={() => setIsSaved(false)}
                styles={{ root: {
                  margin: '10px auto',
                  display: !isSaved ? 'none' : 'block'
                }}}
                messageBarType={MessageBarType.success}>Subscription updated.</MessageBar>
              <MessageBar 
                onDismiss={() => setIsSavedError(false)}
                styles={{ root: {
                  margin: '10px auto',
                  display: !isSavedError ? 'none' : 'block'
                }}}
                messageBarType={MessageBarType.error}>Update failed.</MessageBar>
              <Stack tokens={{ childrenGap: 10 }}>
                <Dropdown
                  label="Azure ML Workspace"
                  disabled={amlworkspaces?.WorkspaceName.length === 0}
                  placeholder="Select Workspace"
                  options={[{
                    'key': amlworkspaces?.WorkspaceName.toLowerCase() || '',
                    'text': amlworkspaces?.WorkspaceName || ''
                  }
                  ]}
                  selectedKey={amlworkspaces?.WorkspaceName.toLowerCase()}
                  onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                    
                    setSelectedValues({
                      amlworkspace: option?.text!,
                      computeCluster: selectedValues?.computeCluster!,
                      deployTargetType: selectedValues?.deployTargetType!,
                      deploymentCluster: selectedValues?.deploymentCluster!
                    });
                  }}
                />
                <Dropdown
                  label="Compute Cluster"
                  placeholder={"props.subscriptionData.AMLWorkspaceComputeClusterName"}
                  options={computeClustersOptions}
                  onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                    setSelectedValues({
                      amlworkspace: selectedValues?.amlworkspace!,
                      computeCluster: option?.text!,
                      deployTargetType: selectedValues?.deployTargetType!,
                      deploymentCluster: selectedValues?.deploymentCluster!
                    });
                  }}
                />
                <Dropdown
                  label="Deploy Target Type"
                  placeholder={"props.subscriptionData.AMLWorkspaceDeploymentTargetType"}
                  options={deploymentTargetTypesOptions}
                  onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                    setSelectedValues({
                      amlworkspace: selectedValues?.amlworkspace!,
                      computeCluster: selectedValues?.computeCluster!,
                      deployTargetType: option?.text!,
                      deploymentCluster: selectedValues?.deploymentCluster!
                    });
                    SetIsDeploymentClusterDisabled(selectedValues?.deployTargetType === 'Azure Container Instances');
                  }}
                />
                <Dropdown
                  label="Deployment Cluster"
                  placeholder={"props.subscriptionData.AMLWorkspaceDeploymentClusterName"}
                  disabled={!isDeploymentClusterDisabled}
                  options={deploymentClusterOptions}
                  onChange={(event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption | undefined, index?: number | undefined) => {
                    setSelectedValues({
                      amlworkspace: selectedValues?.amlworkspace!,
                      computeCluster: selectedValues?.computeCluster!,
                      deployTargetType: selectedValues?.deployTargetType!,
                      deploymentCluster: option?.text!
                    });
                  }}
                />
                <Toggle offText="No" onText="Yes" label="Enable Admin?" inlineLabel checked={IsAdminEnabled} onChange={
                  (event: React.MouseEvent<HTMLElement, MouseEvent>, checked?: boolean | undefined) => {
                    SetIsAdminEnabled(checked!);
                }} />
                <Stack horizontal horizontalAlign="end" tokens={{ childrenGap: 20 }}>
                  <Spinner
                      label="Saving workspace settings..."
                      size={SpinnerSize.small}
                      unselectable={'on'}
                      style={{ visibility: isFormSubmitting ? 'visible' : 'hidden' }}
                      ariaLive="assertive"
                      labelPosition="left" />
                  <PrimaryButton
                      onClick={settingSubmit}
                      disabled={!isFormValid}
                    >Save</PrimaryButton>
                </Stack>
                
              </Stack>
        </div>
      }
      
    </div>
  )
}

interface IDetailsProps {
  subscriptionID: string;
  subscriptionData: ISubscriptionDetail;
}

interface IPlanResponse {
  plans: Array<string>;
}

interface IAmlWorkSpaces {
  AADApplicationId: string;
  AADApplicationSecret: string;
  AADApplicationSecretName?: any;
  AADTenantId: string;
  ComputeClusters: string[];
  DeploymentClusters: string[];
  DeploymentTargetTypes: DeploymentTargetType[];
  Id: number;
  Region?: any;
  ResourceId: string;
  WorkspaceName: string;
}

interface DeploymentTargetType {
  displayName: string;
  id: string;
}

interface ISelectedItems {
  amlworkspace: string;
  computeCluster: string;
  deployTargetType: string;
  deploymentCluster: string;
}