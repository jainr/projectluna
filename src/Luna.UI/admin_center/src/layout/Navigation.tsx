import React, { useEffect, useState } from 'react';
import { Icon } from '@fluentui/react';
import '../../src/navigation.css';
import { useHistory } from 'react-router';
import { ProSidebar, SidebarHeader, SidebarContent } from "react-pro-sidebar";

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
        let maintag = document.getElementsByClassName("main")[0] as HTMLElement;
        menuCollapse == true ? maintag.style.marginLeft = '15%' : maintag.style.marginLeft = '8%';
        
        let NavigationDiv = document.getElementsByClassName("Navigation")[0] as HTMLElement;
        menuCollapse == true ?NavigationDiv.classList.add('wdth10per'):NavigationDiv.classList.remove('wdth10per');
    };

    useEffect(() => {                    
        let maintag = document.getElementsByClassName("main")[0] as HTMLElement;
         menuCollapse == true ? maintag.style.marginLeft = '8%' : maintag.style.marginLeft = '15%';
      }, []);

    return (
        <React.Fragment>
            <nav className="Navigation wdth10per" id="Navigation">
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
                                <li className="nav-item"><a onClick={(event) => setActive('Dashboard', event)}><Icon iconName="Home" /> <span> Home</span></a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Reports', event)}><Icon iconName="BarChartVertical" /><span>Reports</span></a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Supports', event)}><Icon iconName="Settings" /><span>Supports</span></a></li>
                                <li className="nav-item"><a onClick={(event) => setActive('Settings', event)}><Icon iconName="Settings" /><span>Settings</span></a></li>
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
