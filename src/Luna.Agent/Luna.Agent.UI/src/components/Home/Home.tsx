// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import { SelectionMode, Text, IColumn, ShimmeredDetailsList } from '@fluentui/react';
import '../Home/Home.css';
import FooterLinks from '../FooterLinks/FooterLinks';
import { PanelStyles } from '../../helpers/PanelStyles';

class Home extends React.Component<{}, BaseSubscription> {

  constructor(props: {}) {
    super(props);
    this.state = {
      isLoading: true,
      subscriptions: [{
        HostType: "",
        Id: 0,
        PrimaryKey: "",
        SecondaryKey: "",
        ProductType: "",
        PublisherId: "",
        Status: "",
        SubscriptionId: "",
        UserId: "",
        CreatedTime: "",
        DeploymentName: "",
        BaseUrl: "",
        SubscriptionName: ""
      }],
      columns: [

        {
          name: "",
          ariaLabel: "List Icon",
          key: "icon",
          minWidth: 30,
          maxWidth: 30,
          isResizable: false,
          isIconOnly: true,
          onColumnClick: this._onColumnClick
        },
        {
          name: "Subscription ID",
          key: "column1",
          minWidth: 100,
          maxWidth: 250,
          fieldName: "SubscriptionId",
          isResizable: true,
          isSortedDescending: true,
          onColumnClick: this._onColumnClick
        },
        {
          name: "Offer Name",
          key: "column2",
          minWidth: 50,
          maxWidth: 100,
          fieldName: "DeploymentName",
          isResizable: true,
          onColumnClick: this._onColumnClick
        },
        {
          name: "Status",
          key: "column3",
          minWidth: 40,
          maxWidth: 100,
          fieldName: "Status",
          isResizable: true,
          onColumnClick: this._onColumnClick
        },
        {
          name: "Created Time",
          key: "column5",
          minWidth: 30,
          maxWidth: 40,
          fieldName: "CreatedTime",
          isResizable: true,
          onColumnClick: this._onColumnClick
        }
      ]
    }

    // binded methods.
    this.navigateToSubscriptionDetail = this.navigateToSubscriptionDetail.bind(this);
    this._onColumnClick = this._onColumnClick.bind(this);
  }

  async componentWillMount() {

    const bearerToken = 'Bearer ' + sessionStorage.getItem(`msal.${window.MSAL_CONFIG.appId}.idtoken`);
    const requestURL = window.BASE_URL + '/subscriptions';

    fetch(requestURL, {
      mode: "cors",
      method: "GET",
      headers: {
        'Authorization': bearerToken,
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.json())
      .then(_data => {
        this.setState({ subscriptions: _data });
        setTimeout(() => {
          this.setState({ isLoading: false });
        }, 1500);
      });
  }

  navigateToSubscriptionDetail(event: Subscription) {
    window.location.href = window.location.origin + "/#/details/" + event.SubscriptionId;
  };

  _onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
    const items = this.state.subscriptions;
    const columns = this.state.columns;
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
    const newItems = this._copyAndSort(items, currColumn.fieldName!, currColumn.isSortedDescending);
    this.setState({
      columns: newColumns,
      subscriptions: newItems,
    });
  };

  private _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
  }

  render() {
    return (
      <div className="Home">
        <div style={PanelStyles}>
          <Text variant={'xLarge'}>From Azure Marketplace</Text>
          <p style={{ display: this.state.subscriptions.length <= 1 ? "block" : "none" }}>
            <Text variant={'small'}>No subscriptions found.</Text>
          </p>
          <div style={{ display: this.state.subscriptions.length <= 1 ? "none" : "block" }}>
            <ShimmeredDetailsList
              columns={this.state.columns || []}
              compact={true}
              items={this.state.subscriptions}
              setKey="subscriptionId"
              selectionMode={SelectionMode.none}
              useReducedRowRenderer={true}
              enableShimmer={this.state.isLoading}
              enableUpdateAnimations={true}
              onActiveItemChanged={this.navigateToSubscriptionDetail}
              ariaLabelForShimmer="Content is being fetched"
              ariaLabelForGrid="Item details"
            />
          </div>

        </div>
        <FooterLinks />
      </div>
    )
  };

}

export default Home;

interface Subscription {
  BaseUrl: string;
  CreatedTime: string;
  Id: number;
  PrimaryKey: string;
  SecondaryKey: string;
  HostType: string;
  ProductType: string;
  PublisherId: string;
  DeploymentName: string;
  Status: string;
  SubscriptionId: string;
  SubscriptionName: string;
  UserId: string;
}

interface BaseSubscription {
  eventData?: string;
  isLoading: boolean;
  columns: IColumn[];
  subscriptions: Subscription[];
}