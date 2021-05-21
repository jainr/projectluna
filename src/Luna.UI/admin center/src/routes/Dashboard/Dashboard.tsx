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

const Dashboard: React.FunctionComponent = () => {

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
          <h1>Luna Management Center</h1>
          <div style={{ width: '100%' }}>
            <div className="dashboardcard bgclrblue floatLeft">
              <div className="cardtext">
                <span>15</span> <br></br>
                <span>Active <br></br>Installation</span> <br></br>
              </div>              
              <div className="txticon">
                <span className="fnt10">Go to details</span>&nbsp;&nbsp;<FontIcon iconName="Edit" className="deleteicon" />
              </div>
            </div>
            <div className="dashboardcard bgclrblue floatLeft">
              <div className="cardtext">
                <span>12</span> <br></br>
                <span>New Installation in<br></br> Past 30 days</span> <br></br>
              </div>              
              <div className="txticon">
                <span className="fnt10">Go to details</span>&nbsp;&nbsp;<FontIcon iconName="NavigateForwardIcon" className="deleteicon" />
              </div>

            </div>
            <div className="dashboardcard bgclrred floatLeft">
              <div className="cardtext">
                <span>1</span> <br></br>
                <span>Customer  churn in<br></br> past 30 days</span> <br></br>
              </div>
              <div className="txticon">
                <span className="fnt10">Go to details</span>&nbsp;&nbsp;<FontIcon iconName="NavigateForwardIcon" className="deleteicon" />
              </div>
            </div>
          </div>
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
        ><h3>Outstanding Support Cases &nbsp;&nbsp; <a href="#" style={{fontSize:14,fontWeight: 'normal'}}>{"See all support cases"}</a></h3></Stack>

        <table className="noborder offergrid" style={{ marginTop: 20, width: '100%' }} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr style={{ fontWeight: 'normal' }}>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Title"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Created By"} /></th>
              <th style={{ width: 100, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Created Date"} /></th>
              <th style={{ width: 300, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Last Updated Date"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"IcM Ticket"} /></th>
            </tr>
          </thead>
          <tbody>
            {loadingData ?
              (
                <tr>
                  <td colSpan={4} align={"center"}>
                    <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                      <Loading />
                    </Stack>
                  </td>
                </tr>
              )
              : null}
          </tbody>
        </table>
      </Stack>
    </Stack>
  );
};

export default Dashboard;