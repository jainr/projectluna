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
            <li className="nav-item"><a href="/#/aiservices" onClick={setActive}><Icon iconName="Shop" /> AI Services</a></li>
            <li className="nav-item"><a href="/#/" onClick={setActive}><Icon iconName="BulletedList" /> Subscriptions</a></li>
            <li className="nav-item"><a href="/#/datasources" onClick={setActive}><Icon iconName="DataManagementSettings" /> Data Sources</a></li>
            <li className="nav-item"><a href="/#/tasks" onClick={setActive}><Icon iconName="Settings" /> Feature Store</a></li>
        </ul>
    </nav>
  );
}

export default Navigation;
