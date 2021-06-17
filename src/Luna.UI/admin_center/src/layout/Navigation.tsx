import React from 'react';
import { Icon } from '@fluentui/react';
import '../../src/navigation.css';
import { Link } from 'office-ui-fabric-react';
import { Redirect } from 'react-router-dom';
import { WebRoute } from '../shared/constants/routes';
import { useHistory, useLocation } from 'react-router';

const Navigation: React.FunctionComponent = () => {

    const history = useHistory();

    const setActive = (route:string,event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
        for (const key in document.getElementsByClassName('nav-item')) {
            if (Object.prototype.hasOwnProperty.call(document.getElementsByClassName('nav-item'), key)) {
                const liElement = document.getElementsByClassName('nav-item')[key] as HTMLLIElement;
                liElement.classList.remove('active');
            }
        }
        let p = event.currentTarget.parentElement;
        if (p) {
            p.classList.add('active');
        }
        history.push(route);
    }
    return (
        <React.Fragment>
            <nav className="Navigation">
                <div className={"collapsearrow"}>
                    <Icon iconName="DoubleChevronLeft8" className="Arrowicon" />
                </div>
                <ul>
                    <li className="nav-item"><a  onClick={(event) => setActive('Dashboard', event)}><Icon iconName="Home" /> Home</a></li>
                    <li className="nav-item"><a  onClick={(event) => setActive('Reports', event)}><Icon iconName="BarChartVertical" />Reports</a></li>
                    <li className="nav-item"><a  onClick={(event) => setActive('Supports', event)}><Icon iconName="Settings" />Supports</a></li>
                    <li className="nav-item"><a  onClick={(event) => setActive('Settings', event)}><Icon iconName="Settings" />Settings</a></li>
                </ul>
            </nav>
        </React.Fragment>
    );
}

export default Navigation;
