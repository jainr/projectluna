import React from 'react';
import { mount, configure } from 'enzyme';
import Adapter from 'enzyme-adapter-react-16';
configure({ adapter: new Adapter() });
import Home from './Home';

describe('Home component', () => {
    it('does not error', () => {
      const wrapper = mount(<Home />);
      expect(wrapper.not(console.error));
    });
});