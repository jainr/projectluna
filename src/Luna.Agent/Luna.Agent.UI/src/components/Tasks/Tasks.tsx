// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import FooterLinks from '../FooterLinks/FooterLinks';
import { UnderConstruction } from '../_subcomponents/UnderConstruction/UnderConstruction';

/** @component FeatureStores View. */
function Tasks() {
  return (
    <div className="Tasks">
      <UnderConstruction title="Feature Store" />
      <FooterLinks />
    </div>
  );
}

export default Tasks;
