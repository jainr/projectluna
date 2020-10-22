// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import { mount, configure } from 'enzyme';
import Adapter from 'enzyme-adapter-react-16';
configure({ adapter: new Adapter() });
import Navigation from './Navigation';

describe('Navigation component', () => {
    it('does not error', () => {
      const wrapper = mount(<Navigation />);
      expect(wrapper.not(console.error));
    });
});