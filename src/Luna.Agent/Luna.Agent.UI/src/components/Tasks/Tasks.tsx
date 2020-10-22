// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import FooterLinks from '../FooterLinks/FooterLinks';
import { UnderConstruction } from '../_subcomponents/UnderConstruction/UnderConstruction';

/** @component Tasks View. */
function Tasks() {
  return (
    <div className="Tasks">
      <UnderConstruction title="Tasks" />
      <FooterLinks />
    </div>
  );
}

export default Tasks;
