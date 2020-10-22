// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import '../FooterLinks/FooterLinks.css';

function FooterLinks() {
  return (
    <div className="FooterLinks">
        <div 
            className="footerLinks">
            <a href="https://go.microsoft.com/fwlink/?LinkId=521839" rel="noopener noreferrer" target="_blank">Privacy &amp; Cookies</a>
            <a href="https://go.microsoft.com/fwlink/?LinkID=206977" rel="noopener noreferrer" target="_blank">Terms Of Use</a>
            <span>&copy; Microsoft 2020</span>
        </div>
    </div>
  );
}

export default FooterLinks;