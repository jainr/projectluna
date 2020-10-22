// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import { mount, configure } from 'enzyme';
import Adapter from 'enzyme-adapter-react-16';
configure({ adapter: new Adapter() });
import Settings from './Settings';

describe('Settings component', () => {
    it('does not error', () => {
      const wrapper = mount(<Settings />);
      expect(wrapper.not(console.error));
    });
});