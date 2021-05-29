import React from 'react';
import {Image, Link, Stack} from 'office-ui-fabric-react';
import {WebRoute} from "../shared/constants/routes";

const Footer: React.FunctionComponent = () => {


  const logo = "../../Picture1.png";
  const isvName = window.Configs.ISV_NAME;
  const footerBackgroundColor = "#f1f1f1";

  return (
    <Stack
      horizontal={true}
      verticalAlign={"center"}
      styles={{
        root: {
          backgroundColor: `${footerBackgroundColor}`,
          height:'auto'
        }
      }}
    >
      <Stack
        horizontal={true}
        horizontalAlign={"center"}
        verticalAlign={"center"}
        className= {"footerclass"}
        styles={{
          root: {
            marginLeft:27,
            marginRight:'1%'
          }
        }}
      >       
        <div>
          <ul>
            <li>
                <a href="#">Privacy & Cookies</a>
            </li>
            <li className={"sperator"}>
                <span>|</span>
            </li>
            <li>
            <a href="#">Term Of Use</a>
            </li>
            <li className={"sperator"}>
                <span>|</span>
            </li>
            <li>
            <a href="#">&#169; Microsoft {(new Date().getFullYear())}</a>
            </li>
          </ul>
        </div>
      </Stack>
    </Stack>
  );
};

export default Footer;