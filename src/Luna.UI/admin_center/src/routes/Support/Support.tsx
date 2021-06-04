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

const Support: React.FunctionComponent = () => {

  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars  
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingActiveCaseData, setLoadingActiveCaseData] = useState<boolean>(true);
  const [loadingPermissionsData, setLoadingPermissionsDataData] = useState<boolean>(true);

  useEffect(() => {
    setLoadingActiveCaseData(true);
    setLoadingPermissionsDataData(true);
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
          <h1>Support</h1>
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
          <Pivot aria-label="Power BI Report" style={{ width: '100%' }}>
            <PivotItem
              headerText="Support Case"
              headerButtonProps={{
                'data-order': 1,
                'data-title': 'Support Case',
              }}>
              <h3>
                Active Cases
              </h3>
              
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
                    : null}
                </tbody>
              </table>
            </PivotItem>
            <PivotItem headerText="Permissions"
              headerButtonProps={{
                'data-order': 2,
                'data-title': 'Permissions',
              }}>
              <h3>
                Luna Installation
              </h3>              
              <table className="noborder offergrid" style={{ marginTop: 20, width: '100%' }} cellPadding={5} cellSpacing={0}>
                <thead>
                  <tr style={{ fontWeight: 'normal' }}>
                    <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Customer Name"} /></th>
                    <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Active Support Case"} /></th>
                    <th style={{ width: 100, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Permissions"} /></th>
                    <th style={{ width: 300, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"History"} /></th>
                  </tr>
                </thead>
                <tbody>
                  {loadingPermissionsData ?
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
            </PivotItem>
            <PivotItem headerText="Resources" headerButtonProps={{
              'data-order': 3,
              'data-title': 'Resources',
            }}>              
              <table className="noborder offergrid" style={{ marginTop: 20, width: '100%' }} cellPadding={5} cellSpacing={0}>
                <tr>
                  <td>
                    <a href="#">Internal Documentations</a>
                  </td>
                </tr>
                <tr>
                  <td>
                    <a href="#">Source Code</a>
                  </td>
                </tr>
                <tr>
                  <td>
                    <a href="#">TSGs</a>
                  </td>
                </tr>
                <tr>
                  <td>
                    <a href="#">Public Documnetation</a>
                  </td>
                </tr>
                <tr>
                  <td>
                    <a href="#">Support cases in GitHub repo</a>
                  </td>
                </tr>
                <tr>
                  <td>
                    <a href="#">ICM</a>
                  </td>
                </tr>
              </table>
            </PivotItem>
          </Pivot>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default Support;