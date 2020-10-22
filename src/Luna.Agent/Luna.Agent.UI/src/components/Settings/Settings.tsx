import * as React from 'react';
import { Pivot, PivotItem } from '@fluentui/react';
import FooterLinks from '../FooterLinks/FooterLinks';
import { LinkedServices } from './Settings.LinkedServices';
import { Administration } from './Settings.Administration';
import { AgentProperties } from './Settings.AgentProperties';
import { Publishers } from './Settings.Publishers';

/** @component Settings view and code including PivotView for subpages. */
function Settings() {
  return (
    <div className="Settings">
      <Pivot>
        <PivotItem headerText="Linked Services">
          <br />
          <LinkedServices />
        </PivotItem>
        <PivotItem headerText="Administrators">
          <br />
          <Administration />
        </PivotItem>
        <PivotItem headerText="Agent Properties">
          <br />
          <AgentProperties />
        </PivotItem>
        <PivotItem headerText="Publishers">
          <br />
          <Publishers />
        </PivotItem>
      </Pivot>
      <FooterLinks />
    </div>
  );
}

export default Settings;