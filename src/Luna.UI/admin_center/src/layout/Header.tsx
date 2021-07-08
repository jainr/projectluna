import React from 'react';
import {Image, Link, Stack} from 'office-ui-fabric-react';
import {useHistory} from "react-router";
import {WebRoute} from "../shared/constants/routes";
import adalContext from "../adalConfig";


const Header: React.FunctionComponent = () => {


  const logo = "../../Picture1.png";
  const isvName = window.Configs.ISV_NAME;
  const headerBackgroundColor = window.Configs.HEADER_BACKGROUND_COLOR;
  let userName = "";  
  const history = useHistory();

  var response = adalContext.AuthContext.getCachedUser();
  if (response && response.profile && response.profile.name)
    userName = response.profile.name;

  const handleLogOut = () => {
    adalContext.LogOut();
  };

  return (
    <Stack
      horizontal={true}
      verticalAlign={"center"}
      styles={{
        root: {
          backgroundColor: `${headerBackgroundColor}`,
          height:48
        }
      }}
    >
      <Stack
        horizontal={true}
        horizontalAlign={"center"}
        verticalAlign={"center"}
        styles={{
          root: {
            marginLeft:27,
            marginRight:'1%'
          }
        }}
      >        
        <Image src={logo} onClick={() => {history.push(WebRoute.Home)}} style={{cursor: 'pointer'}} />
      </Stack>
      
      <span className={'isv_title'} style={{textAlign:"left",flexGrow:1,fontWeight:'bold'}}>
        {isvName}
      </span>
      <div>
        <span className={'isv_title'} style={{fontSize:14}}>
          Welcome, {userName}
        </span>
        <span className={'isv_title'} style={{fontSize:14, margin:5}}>
          |
        </span>
        <Link onClick={handleLogOut} className={'isv_title'} style={{marginRight: 27, fontSize:14, color:'white'}}>
          Log Out
        </Link>
      </div>

    </Stack>
  );
};

export default Header;