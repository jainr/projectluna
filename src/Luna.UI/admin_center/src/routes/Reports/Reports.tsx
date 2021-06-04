import React, { useEffect, useState } from 'react';
import {
  Stack,
  PrimaryButton,
  MessageBar,
  MessageBarType,
  Dialog, DialogType, DialogFooter,
  FontIcon,
  TextField,
  FontWeights,
  Pivot,
  Label,
  PivotItem,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { useHistory } from "react-router";
import { WebRoute } from "../../shared/constants/routes";
import { Loading } from "../../shared/components/Loading";
import AlternateButton from "../../shared/components/AlternateButton";
import { Formik } from "formik";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { DialogBox } from '../../shared/components/Dialog';

const Reports: React.FunctionComponent = () => {

  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars  
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingData, setLoadingData] = useState<boolean>(true);

  useEffect(() => {
    setLoadingData(true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <Stack
      verticalAlign="start"
      horizontal={false}
      styles={{
        root: {
          width: '100%',
          height: '100%',
          textAlign: 'left',
        }
      }}
    >
      <Stack
        horizontalAlign="start"
        verticalAlign="center"
        styles={{
          root: {
            width: '100%'
          }
        }}
      >
        <Stack
          horizontalAlign="start"
          verticalAlign="center"
          styles={{
            root: {
              width: '100%',
              //position:'absolute'
            }
          }}
        >
          <h1>Reports</h1>
        </Stack>
        <Stack
          horizontalAlign="start"
          verticalAlign="center"
          styles={{
            root: {
              width: '100%',
              //position:'absolute'
            }
          }}
        >
          <Pivot aria-label="Power BI Report" style={{width:'100%'}}>
            <PivotItem
              headerText="Luna App"
              headerButtonProps={{
                'data-order': 1,
                'data-title': 'Luna App',
              }}>
              <div style={{backgroundColor:'#4472C4',minHeight:'275px',width:'100%',marginTop:'1%'}}>
                <Label style={{paddingLeft: '45%',paddingTop:'8%',color:'#fff'}}><h3>This is Power BI Report #1</h3></Label>
              </div>
            </PivotItem>
            <PivotItem headerText="Usage Analysis"
            headerButtonProps={{
              'data-order': 2,
              'data-title': 'Usage Analysis',
            }}>
              <div style={{backgroundColor:'#4472C4',minHeight:'275px',width:'100%',marginTop:'1%'}}>
              <Label style={{paddingLeft: '45%',paddingTop:'8%',color:'#fff'}}><h3>This is Power BI Report #2</h3></Label>
              </div>
            </PivotItem>
            <PivotItem headerText="Supportability" headerButtonProps={{
              'data-order': 3,
              'data-title': 'Supportability',
            }}>
              <div style={{backgroundColor:'#4472C4',minHeight:'275px',width:'100%',marginTop:'1%'}}>
              <Label style={{paddingLeft: '45%',paddingTop:'8%',color:'#fff'}}><h3>This is Power BI Report #3</h3></Label>
              </div>
            </PivotItem>
          </Pivot>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default Reports;