// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import { mount, configure } from 'enzyme';
import Adapter from 'enzyme-adapter-react-16';
configure({ adapter: new Adapter() });
import App from './App';

describe('App component', () => {
    it('does not error', () => {
      const wrapper = mount(<App />);
      expect(wrapper.not(console.error));
    });
});