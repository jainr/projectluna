import React from 'react';
import {Stack,} from 'office-ui-fabric-react';

const NoVersion: React.FunctionComponent = () => {

  return (
    <Stack
      verticalAlign="start"
      horizontal={false}
      styles={{
        root: {
          width: '100%',
          height: '100%',
          textAlign: 'center',
        }
      }}
    >
      <iframe width="1140" height="541.25" src="https://msit.powerbi.com/reportEmbed?reportId=1150275e-5d29-4a62-b880-2a88a06deb3c&autoAuth=true&ctid=72f988bf-86f1-41af-91ab-2d7cd011db47&config=eyJjbHVzdGVyVXJsIjoiaHR0cHM6Ly9kZi1tc2l0LXNjdXMtcmVkaXJlY3QuYW5hbHlzaXMud2luZG93cy5uZXQvIn0%3D" frameBorder="0">

      </iframe>
    </Stack>
  );
};

export default NoVersion;