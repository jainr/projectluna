import React, { useState } from 'react';
import { Icon } from '@fluentui/react';
import '../../src/navigation.css';
import { Link } from 'office-ui-fabric-react';
import { Redirect } from 'react-router-dom';
import { WebRoute } from '../shared/constants/routes';
import { useHistory, useLocation } from 'react-router';
import { ProSidebar, Menu, MenuItem, SidebarHeader, SidebarFooter, SidebarContent } from "react-pro-sidebar";
import { FaHome, FaRegChartBar } from "react-icons/fa";
import { FiSettings } from "react-icons/fi";
import { BiSupport } from "react-icons/bi";

const Navigation: React.FunctionComponent = () => {

    const history = useHistory();
    const [menuCollapse, setMenuCollapse] = useState(false)

    const setActive = (route: string, event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
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
    const menuIconClick = () => {
        //condition checking to change state from true to false and vice versa
        menuCollapse ? setMenuCollapse(false) : setMenuCollapse(true);
    };

    return (
        <React.Fragment>
            {/* <nav className="Navigation">
                <div className={"collapsearrow"} onClick={menuIconClick}>                                        
                    {menuCollapse ? (
                        <Icon iconName="DoubleChevronRight8" className="Arrowicon"  /> 
                    ) : (
                        <Icon iconName="DoubleChevronLeft8" className="Arrowicon"  /> 
                    )}
                </div>
                <ul>
                    <li className="nav-item"><a onClick={(event) => setActive('Dashboard', event)}><Icon iconName="Home" /> Home</a></li>
                    <li className="nav-item"><a onClick={(event) => setActive('Reports', event)}><Icon iconName="BarChartVertical" />Reports</a></li>
                    <li className="nav-item"><a onClick={(event) => setActive('Supports', event)}><Icon iconName="Settings" />Supports</a></li>
                    <li className="nav-item"><a onClick={(event) => setActive('Settings', event)}><Icon iconName="Settings" />Settings</a></li>
                </ul>
            </nav> */}
            <nav className="Navigation" id="Navigation">
                <ProSidebar collapsed={menuCollapse}>
                    <SidebarHeader>
                        <div className={"collapsearrow"} onClick={menuIconClick}>
                            {menuCollapse ? (
                                <Icon iconName="DoubleChevronRight8" className="Arrowicon" />
                            ) : (
                                <Icon iconName="DoubleChevronLeft8" className="Arrowicon" />
                            )}
                        </div>
                    </SidebarHeader>
                    <SidebarContent>
                        {menuCollapse ?
                            <ul>
                                <li className="nav-item"><a onClick={(event) => setActive('Dashboard', event)}><Icon iconName="Home" /></a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Reports', event)}><Icon iconName="BarChartVertical" /></a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Supports', event)}><Icon iconName="Settings" /></a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Settings', event)}><Icon iconName="Settings" /></a></li>
                            </ul>
                            :
                            <ul>
                                <li className="nav-item"><a onClick={(event) => setActive('Dashboard', event)}><Icon iconName="Home" /> Home</a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Reports', event)}><Icon iconName="BarChartVertical" />Reports</a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Supports', event)}><Icon iconName="Settings" />Supports</a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Settings', event)}><Icon iconName="Settings" />Settings</a></li>
                            </ul>                           
                        }
                        {/* <Menu>
                            <MenuItem icon={<FaHome />}>Home</MenuItem>
                            <MenuItem icon={<FaRegChartBar />}>Reports</MenuItem>
                            <MenuItem icon={<BiSupport />}>Supports</MenuItem>
                            <MenuItem icon={<FiSettings />}>Settings</MenuItem>
                        </Menu> */}
                    </SidebarContent>
                </ProSidebar>
            </nav>
        </React.Fragment>
    );
}

export default Navigation;
