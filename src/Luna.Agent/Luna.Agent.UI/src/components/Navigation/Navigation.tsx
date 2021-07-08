import React from 'react';
import { Icon } from '@fluentui/react';
import '../Navigation/Navigation.css';

function Navigation() {
  /**
   * Set the active LI for the navigation item.
   * @param event React.MouseEvent<HTMLAnchorElement, MouseEvent>
   */
  const setActive = (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
    for (const key in document.getElementsByClassName('nav-item')) {
      if (Object.prototype.hasOwnProperty.call(document.getElementsByClassName('nav-item'), key)) {
        const liElement = document.getElementsByClassName('nav-item')[key] as HTMLLIElement;
        liElement.classList.remove('active');
      }
    }
    let p = event.currentTarget.parentElement
    p?.classList.add('active');
  }
  return (
    <nav className="Navigation">
        <ul>
            <li className="nav-item"><a href="/aiservices" onClick={setActive}><Icon iconName="Shop" /> Machine Learning Gallery</a></li>            
            <li className="nav-item"><a href="/settings" onClick={setActive}><Icon iconName="Settings" />Settings</a></li>
        </ul>
    </nav>
  );
}

export default Navigation;
