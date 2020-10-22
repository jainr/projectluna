// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as React from 'react';
import imgSource from '../UnderConstruction/contruction.svg';

interface IUnderConstruction {
    title: string;
    children?: any;
}

/** @component Under Contruction reusable UI component. */
export const UnderConstruction: React.FC<IUnderConstruction> = ({ title }: IUnderConstruction) => {
    return (
        <div style={panelStyles}>
            <h1 style={h1Styles}>{title + ' '}Coming Soon</h1>
            <img src={imgSource} style={imageStyles} alt="Under Construction" />
        </div>
    );
}

const panelStyles: React.CSSProperties = {
    background: '#DFDFE6 0% 0% no-repeat padding-box',
    borderRadius: '4px',
    textAlign: 'center',
    padding: '40px'
}

const imageStyles: React.CSSProperties = {
    width: '80%'
}

const h1Styles: React.CSSProperties = {
    font: 'normal normal bold 42px/48px "Segoe UI", Arial, Helvetica, sans-serif',
    letterSpacing: 0,
    marginTop: 0,
    marginBottom: 40,
    color: '#16181A',
    userSelect: 'none',
    textRendering: 'optimizeLegibility'
}