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
import SettingsService from "../../services/SettingsService";
import { ISettingsModel } from '../../models';
import { initialUserList } from "./formUtils/SettingsFormUtils";

const Settings: React.FunctionComponent = () => {

  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars  
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingUserData, setLoadingUserData] = useState<boolean>(true);
  const [userData, setUserData] = useState<ISettingsModel[]>([]);  

  useEffect(() => {
    loadstaticdata();
    // getUserDataList();    
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadstaticdata = async () => {
    setLoadingUserData(true);
    setUserData(initialUserList);
    setLoadingUserData(false);
  }

  const getUserDataList = async () => {

    setLoadingUserData(true);
    const results = await SettingsService.userDataList();
    if (results && !results.hasErrors && results.value)
      setUserData(results.value);
    else {
      setUserData([]);
      if (results.hasErrors) {
        // TODO: display errors
        alert(results.errors.join(', '));
      }
    }
    setLoadingUserData(false);
  }

  const User = ({ userData }) => {
    if (!userData || userData.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Data</span></td>
      </tr>;
    } else {
      return (
        userData.map((value: ISettingsModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span style={{ width: 200 }} className="acolor"><a href="">{value.user}</a></span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.role}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.createdDate}</span>
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
          <h1>Settings</h1>
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
          <h3>
            Users
          </h3>

          <table className="noborder offergrid" style={{ marginTop: 20, width: '100%' }} cellPadding={5} cellSpacing={0}>
            <thead>
              <tr style={{ fontWeight: 'normal' }}>
                <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"User"} /></th>
                <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Role"} /></th>
                <th style={{ width: 100, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Created Date"} /></th>
              </tr>
            </thead>
            <tbody>
              {loadingUserData ?
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
                <User userData={userData} />
              }
            </tbody>
          </table>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default Settings;