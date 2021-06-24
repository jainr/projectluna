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
import { ISupportCasesModel } from '../../models';
import { initialSupportCaseList } from '../Support/formUtils/SupportFormUtils';
import SupportService from '../../services/SupportService';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowAltCircleRight } from '@fortawesome/free-solid-svg-icons'

const Dashboard: React.FunctionComponent = () => {

  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars  
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingData, setLoadingData] = useState<boolean>(true);
  const [loadingActiveCaseData, setLoadingActiveCaseData] = useState<boolean>(true);
  const [activeCaseData, setActiveCaseData] = useState<ISupportCasesModel[]>([]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/exhaustive-deps
    loadstaticdata();
    // getSupportActiveCaseData();
  }, []);

  const loadstaticdata = async () => {
    setLoadingActiveCaseData(true);
    setActiveCaseData(initialSupportCaseList);
    setLoadingActiveCaseData(false);
  }

  const getSupportActiveCaseData = async () => {

    setLoadingActiveCaseData(true);
    const results = await SupportService.supportCaseList();
    if (results && !results.hasErrors && results.value)
      setActiveCaseData(results.value);
    else {
      setActiveCaseData([]);
      if (results.hasErrors) {
        // TODO: display errors
        alert(results.errors.join(', '));
      }
    }
    setLoadingActiveCaseData(false);
  }

  const SupportCase = ({ activeCase }) => {
    if (!activeCase || activeCase.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Data</span></td>
      </tr>;
    } else {
      return (
        activeCase.map((value: ISupportCasesModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span style={{ width: 200 }} className="acolor"><a href="">{value.title}</a></span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.createdBy}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.createdTime}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.lastUpdatedTime}</span>
              </td>
              <td>
                <span style={{ width: 200 }} className="acolor"> <a href="">{value.icmTicket}</a></span>
              </td>
            </tr>
          );
        })
      );
    }
  }

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
              <h1>15</h1>
              <div className="cardtext">
                <span>Active <br></br>Installation</span> <br></br>
              </div>
              <div className="txticon">
                <span className="fnt10">Go to details</span>&nbsp;&nbsp;
                {/* <FontIcon iconName="Edit" className="Arrowicon" /> */}
                <FontAwesomeIcon icon={faArrowAltCircleRight} className={"dashboardarrowIcon"} />
              </div>
            </div>
            <div className="dashboardcard bgclrblue floatLeft">
              <h1 style={{paddingLeft:'8%'}}>2</h1>
              <div className="cardtext">
                <span>New Installation in<br></br> Past 30 days</span> <br></br>
              </div>
              <div className="txticon">
                <span className="fnt10">Go to details</span>&nbsp;&nbsp;
                <FontAwesomeIcon icon={faArrowAltCircleRight} className={"dashboardarrowIcon"} />
              </div>

            </div>
            <div className="dashboardcard bgclrred floatLeft">
              <h1>1</h1>
              <div className="cardtext">
                <span>Customer  churn in<br></br> past 30 days</span> <br></br>
              </div>
              <div className="txticon">
                <span className="fnt10">Go to details</span>&nbsp;&nbsp;
                <FontAwesomeIcon icon={faArrowAltCircleRight} className={"dashboardarrowIcon"} />
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
        ><h3>Outstanding Support Cases &nbsp;&nbsp; <a href="#" style={{ fontSize: 14, fontWeight: 'normal' }}>{"See all support cases"}</a></h3></Stack>

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
            {loadingActiveCaseData ?
              (
                <tr>
                  <td colSpan={4} align={"center"}>
                    <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                      <Loading />
                    </Stack>
                  </td>
                </tr>
              )
              :
              <SupportCase activeCase={activeCaseData} />
            }
          </tbody>
        </table>
      </Stack>
    </Stack>
  );
};

export default Dashboard;