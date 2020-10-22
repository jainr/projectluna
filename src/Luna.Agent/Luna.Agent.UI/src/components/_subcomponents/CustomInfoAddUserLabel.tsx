// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ITextFieldProps, Stack, TooltipHost, Icon } from "@fluentui/react"
import React from "react"
import { SharedColors  } from "@uifabric/fluent-theme";

/** @component Custom Label UI. */
export const CustomInfoAddUserLabel = (lblProps: ITextFieldProps  | undefined) => {
    return(
      <Stack horizontal tokens={{ childrenGap: 5 }}>
        <span>{lblProps?.label}</span>
        <TooltipHost
          id="toolTipHelp"
          content="You can use your email or your AAD ID (GUID)."
          >
          <Icon iconName="Info"
            style={{ cursor: "pointer", color: SharedColors.blue10 }}
          ></Icon>
        </TooltipHost>
      </Stack>
    )
  };