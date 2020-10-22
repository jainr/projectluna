import * as React from 'react';
import { Button, Text, MessageBar, MessageBarType, SelectionMode, ShimmeredDetailsList, IColumn, IconButton, Stack, DefaultButton, Dialog, DialogFooter, PrimaryButton, DialogType, IDialogContentProps, IModalProps, ActionButton, Panel, TextField, BaseButton, Spinner, StackItem, PanelType } from '@fluentui/react';
import { PanelStyles } from '../../helpers/PanelStyles';
import { SharedColors } from '@uifabric/fluent-theme';

/** @component Publishers view and code. */
export const Publishers = () => {
  var initPublishers: any[] = [];
  const [isLoading, setIsLoading] = React.useState<boolean>(true);
  const [lagDelay, setLagDelay] = React.useState<boolean>(true);
  const [publishers, setPublishers] = React.useState<IPublishers[]>([]);
  const [showDeletePanel, setShowDeletePanel] = React.useState<boolean>(false);
  const [activePublisherToBeDeleted, setActivePublisherToBeDeleted] = React.useState<IPublishers>();
  const [IsPanelOpen, SetIsPanelOpen] = React.useState<boolean>(false);

  const [IsSuccess, SetSuccess] = React.useState<boolean>(false);
  const [IsError, SetError] = React.useState<boolean>(false);

  const [PublisherGUID, SetPublisherGUID] = React.useState('');
  const [PublisherURL, SetPublisherURL] = React.useState('');

  function _onColumnClick(ev: React.MouseEvent<HTMLElement, MouseEvent>, column: IColumn): void {
    const columns = publisherColumns;
    const newColumns: IColumn[] = columns.slice();
    const currColumn: IColumn = newColumns.filter(currCol => column.key === currCol.key)[0];
    newColumns.forEach((newCol: IColumn) => {
      if (newCol === currColumn) {
        currColumn.isSortedDescending = !currColumn.isSortedDescending;
        currColumn.isSorted = true;
      } else {
        newCol.isSorted = false;
        newCol.isSortedDescending = true;
      }
    });
    const newItems = _copyAndSort(initPublishers, currColumn.fieldName!, currColumn.isSortedDescending);
    setPublishers(newItems);
    setPublisherColumns(newColumns);
  };

  function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
  }
  //#region Columns Management
  const [publisherColumns, setPublisherColumns] = React.useState<IColumn[]>([
    {
      key: "field1",
      name: "Control Plane URL",
      fieldName: "ControlPlaneUrl",
      isResizable: true,
      isMultiline: true,
      minWidth: 200,
      maxWidth: 300,
      onColumnClick: _onColumnClick
    },
    {
      key: "field2",
      name: "Publisher Name",
      fieldName: "Name",
      isResizable: true,
      isMultiline: true,
      minWidth: 100,
      maxWidth: 200,
      onColumnClick: _onColumnClick
    },
    {
      key: "field3",
      name: "Publisher ID",
      fieldName: "PublisherId",
      isResizable: true,
      isMultiline: false,
      minWidth: 100,
      onColumnClick: _onColumnClick
    },
    {
      key: "deleteCol",
      name: "Delete",
      isResizable: false,
      minWidth: 80,
      maxWidth: 80,
      isMultiline: false,
      isIconOnly: true,
      onColumnClick: _onColumnClick,
      onRender: (item: IPublishers) => {
        return (<Stack horizontal verticalAlign="center" onClick={() => confirmDeletePublisher(item)}>
          <Text variant={'small'} styles={{ root: { color: SharedColors.red20 } }}>Remove</Text>
          <IconButton
            style={{ color: SharedColors.red20 }}
            iconProps={{ iconName: "Delete" }}
            ariaLabel={"Delete "} />
        </Stack>)
      }
    }
  ]);
  //#endregion

  React.useEffect(() => {
    loadData();
  }, []);

  const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);

  const loadData = () => {
    fetch(window.BASE_URL + '/publishers', {
      mode: "cors",
      method: "GET",
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.json())
      .then(data => {
        initPublishers = data;
        setPublishers(data);
        setIsLoading(false);
      }).finally(() => {
        // finish loading UI.
        setLagDelay(false);
      });
  }

  const confirmDeletePublisher = (item: IPublishers) => {
    setActivePublisherToBeDeleted(item);
    setShowDeletePanel(true);
  }

  const deletePublisher = async () => {
    await fetch(`${window.BASE_URL}/publishers/${activePublisherToBeDeleted?.PublisherId}`, {
      mode: "cors",
      method: "DELETE",
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    })
      .then(response => {
        setShowDeletePanel(false);
      });
  }

  const modalProps: IModalProps = {
    titleAriaId: 'dialogLabel',
    subtitleAriaId: 'subTextLabel',
    isBlocking: true,
    isDarkOverlay: true,
    allowTouchBodyScroll: true
  }

  const dialogContentProps: IDialogContentProps = {
    type: DialogType.normal,
    title: 'Delete Publisher',
    closeButtonAriaLabel: 'Close',
    isMultiline: true,
    subText: `Are you sure you want to delete this publisher?`,
  };

  const CreateNewPublisher = async (event: React.MouseEvent<HTMLAnchorElement | HTMLButtonElement | HTMLDivElement | BaseButton | Button | HTMLSpanElement, MouseEvent>) => {
    setIsLoading(true);
    await fetch(`${window.BASE_URL}/publishers/${PublisherGUID}`, {
      mode: "cors",
      method: 'PUT',
      cache: 'no-cache',
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        "PublisherId": PublisherGUID,
        "ControlPlaneUrl": PublisherURL
      })
    })
      .then(response => {
        if (response.status === 200 || 202 || 204) {
          SetSuccess(true);
        } else {
          SetError(false);
          console.error(response);
        }
      }).finally(() => {
        loadData();
        SetIsPanelOpen(false);
        setIsLoading(false);
      });
  }

  const onRenderFooterContent = React.useCallback(
    () => (
      <div>
        <PrimaryButton
          disabled={PublisherURL.length < 5 || PublisherGUID.length !== 36}
          onClick={CreateNewPublisher} styles={{ root: { marginRight: 8 } }}>
          Save
        </PrimaryButton>
        <DefaultButton onClick={() => SetIsPanelOpen(false)}>Cancel</DefaultButton>
      </div>
    ),
    [() => SetIsPanelOpen(false)],
  );

  return (
    <div style={PanelStyles}>
      <Stack horizontal horizontalAlign="space-between">
        <Text variant={'xLargePlus'}>Publishers</Text>
        <ActionButton text="Register New" onClick={() => SetIsPanelOpen(true)} iconProps={{ iconName: 'Add' }}></ActionButton>
      </Stack>
      <br />
      <br />
      {
        (!isLoading && publishers.length && !lagDelay) === 0 &&
        <MessageBar messageBarType={MessageBarType.info}>No publishers assigned.</MessageBar>
      }
      {
        publishers.length > 0 &&
        <ShimmeredDetailsList
          items={publishers || []}
          selectionMode={SelectionMode.none}
          columns={publisherColumns}
          useReducedRowRenderer={true}
          selectionPreservedOnEmptyClick={true}
          enableShimmer={isLoading}
          ariaLabelForShimmer="Content is being fetched"
          ariaLabelForGrid="Publisher details"
        />
      }
      <Dialog
        hidden={!showDeletePanel}
        onDismiss={() => setShowDeletePanel(false)}
        modalProps={modalProps}
        dialogContentProps={dialogContentProps}
      >
        <DialogFooter>
          <PrimaryButton
            style={{
              borderColor: SharedColors.red20,
              backgroundColor: SharedColors.red20,
              color: "white"
            }}
            onClick={deletePublisher} text="Delete" />
          <DefaultButton onClick={() => setShowDeletePanel(false)} text="Cancel" />
        </DialogFooter>
      </Dialog>
      <Panel
        headerText="Register New Publisher"
        isOpen={IsPanelOpen}
        type={PanelType.medium}
        onRenderFooterContent={onRenderFooterContent}
        isFooterAtBottom={true}
        onDismiss={() => SetIsPanelOpen(false)}
        // You MUST provide this prop! Otherwise screen readers will just say "button" with no label.
        closeButtonAriaLabel="Close"
      >
        <Stack tokens={{ childrenGap: 10 }}>
          {
            IsSuccess &&
            <MessageBar
              onDismiss={() => SetSuccess(false)}
              messageBarType={MessageBarType.success}>Publisher Added!</MessageBar>
          }
          {
            IsError &&
            <MessageBar
              onDismiss={() => SetError(false)}
              messageBarType={MessageBarType.error}>Error adding publisher.</MessageBar>
          }
          <TextField label="Publisher URL"
            required
            value={PublisherURL}
            onChange={(event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
              SetPublisherURL(newValue!);
            }}
            placeholder="Example: https://lunaaiprod-apiapp.azurewebsites.net" />
          <TextField label="Publisher ID" placeholder="Example: 644e1dd7-2a7f-18fb-b8ed-ed78c3f92c2b"
            required
            maxLength={36}
            onChange={(event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string | undefined) => {
              SetPublisherGUID(newValue!);
            }}
            value={PublisherGUID} />
          {
            isLoading &&
            <StackItem align="center">
              <Spinner label="Submitting Publisher..." />
            </StackItem>
          }
        </Stack>
      </Panel>
    </div>
  );
}

// Interfaces
interface IPublishers {
  ControlPlaneUrl: string;
  Id: number;
  Name: string;
  PublisherId: string;
}

