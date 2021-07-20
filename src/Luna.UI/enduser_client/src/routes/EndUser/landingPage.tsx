import React, { useEffect, useState } from 'react';
import {
  DatePicker,
  Dropdown,
  getTheme,
  IChoiceGroupOption,
  IDatePickerStrings,
  IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField,
  ChoiceGroup,
  DefaultButton,
  FontIcon,
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { Form, Formik } from 'formik';
import { ILandingModel, IParameterModel } from '../../models/IEnduserLandingModel';
import { Loading } from '../../shared/components/Loading';
import EndUserLandingService from "../../services/EndUserLandingService";
import OfferParameterService from "../../services/OfferParameterService";
import { IOfferParameterModel, ISubscriptionsModel } from '../../models';
import SubscriptionsService from '../../services/SubscriptionsService';
import { useHistory, useLocation } from 'react-router';
import { toast } from "react-toastify";
import * as qs from "query-string";
import adalContext from "../../adalConfig";
import { getInitialLandingModel } from "./formutlis/landingUtils";
import { getInitialCreateSubscriptionModel } from "../Subscriptions/formUtils/subscriptionFormUtils";
import { useGlobalContext } from '../../shared/components/GlobalProvider';
import { handleSubmissionErrorsForForm } from '../../shared/formUtils/utils';

// .ms-Layer.root-131 .ms-Callout-main
// {
//   background-color: #dedede;
//   border: 1px solid #2288d8;
// }
// .ms-Layer.root-131 .ms-Callout-main .ms-Button-flexContainer
// {
//   color: #2288d8;
// }
const LandingPage: React.FunctionComponent = (props) => {

  let body = (document.getElementsByTagName('body')[0] as HTMLElement);
  const history = useHistory();
  const [formState, setFormState] = useState<ILandingModel>(getInitialLandingModel());
  const [formError, setFormError] = useState<string | null>(null);
  const [loadingFormData, setLoadingFormData] = useState<boolean>(true);
  const [accessTokenObtained, setAccessTokenObtained] = useState<boolean>(false);
  const [enableGetAccessToken, setEnableGetAccessToken] = useState<boolean>(false);
  const location = useLocation();
  const DayPickerStrings: IDatePickerStrings = {
    months: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
    shortMonths: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
    days: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
    shortDays: ['S', 'M', 'T', 'W', 'T', 'F', 'S'],
    goToToday: 'Go to today',
    prevMonthAriaLabel: 'Go to previous month',
    nextMonthAriaLabel: 'Go to next month',
    prevYearAriaLabel: 'Go to previous year',
    nextYearAriaLabel: 'Go to next year',
    closeButtonAriaLabel: 'Close date picker'
  };
  const globalContext = useGlobalContext();

  useEffect(() => {
    body.setAttribute('class', 'landing')
    console.log('mounted');
    getinfo();

    return () => {
      console.log('will unmount');
      body.classList.remove('landing');
    }
  }, [])


  // const handledSubmissionErrors = (result: Result<any>, setSubmitting: any): boolean => {
  //   if (!result.success) {
  //     if (result.hasErrors)
  //       // TODO - display the errors here
  //       toast.error(result.errors.join(', '));
  //     setSubmitting(false);
  //     return true;
  //   }
  //   return false;
  // }

  const getinfo = async () => {
    setLoadingFormData(true);
    var result = qs.parse(location.search);
    if (!result.token) {
      setLoadingFormData(false);
      return;
    }

    var token = decodeURI(result.token as string);

    let data = await EndUserLandingService.resolveToken(`\"${token}\"`);

    if (data.value && data.success) {

      let formData: ILandingModel = getInitialLandingModel();

      let usr = adalContext.AuthContext.getCachedUser();
      if (usr && usr.profile) {
        if (usr.profile.name)
          formData.fullName = usr.profile.name;
        if (usr.userName)
          formData.email = usr.userName;
      }

      // set resolvetoken data
      formData.planName = data.value.PlanId;
      formData.offerName = data.value.OfferId;
      formData.beneficiaryTenantId = "";
      formData.purchaserTenantId = "";
      formData.quantity = 1;
      formData.subscriptionId = data.value.Id;
      formData.subscriptionName = data.value.Name;

      const [
        deviceCodeResponse,
        offerParametersResponse,
        subscriptionResponse,
      ] = await Promise.all([
        SubscriptionsService.getDeviceCode(),
        OfferParameterService.list(data.value.OfferId, data.value.PlanId),
        SubscriptionsService.list(formData.email)
      ]);

      if (deviceCodeResponse.success && deviceCodeResponse.value){
        formData.deviceCode = deviceCodeResponse.value.device_code
        formData.userCode = deviceCodeResponse.value.user_code
      }

      // redirect to the subscription list because the user already has the subscription
      if ((subscriptionResponse.value && subscriptionResponse.success
        && (subscriptionResponse.value as ISubscriptionsModel[])
        && (subscriptionResponse.value as ISubscriptionsModel[]).findIndex(x => x.Id === formData.subscriptionId) >= 0)
        || !offerParametersResponse.success) {
        history.push("/Subscriptions");
        return;
      }

      if (offerParametersResponse.value
        && offerParametersResponse.success) {

        var offerParameters = offerParametersResponse.value as IOfferParameterModel[];
        let Parametersarray: IParameterModel[] = [];
        offerParameters.map((item, index) => {
          return (
          Parametersarray.push({
            parameterName: item.ParameterName,
            displayName: item.DisplayName,
            description: item.Description,
            valueType: item.ValueType,
            fromList: item.FromList,
            valueList: item.ValueList,
            maximum: item.Maximum,
            minimum: item.Minimum
          }))
        });

        formData.inputParameters = Parametersarray;
      }

      setFormState({ ...formData });
    }
    else { // resolve token call failed
      history.push("/Subscriptions");
      return;
    }
    setLoadingFormData(false);
  }

  const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    console.log('changed:', fieldKey);
    if (option) {
      let key = (option.key as string);
      setFieldValue(fieldKey, key, true);
    }
  };

  const getFormErrorString = (touched, cntrlid, minvalue, maxvalue) => {
    let cntrlvalue = (document.getElementById(cntrlid) as HTMLElement) as any;
    if (cntrlvalue) {
      let value = parseInt(cntrlvalue.value)
      if (value < minvalue) {
        return 'Value can not be less than ' + minvalue;
      } else if (value > maxvalue) {
        return 'Value can not be greater than ' + maxvalue;
      } else {
        return '';
      }
    } else {
      return '';
    }

  };


  const theme = getTheme();

  const dropDownValues = (items: string): IDropdownOption[] => {
    let listitems: IDropdownOption[] = [];
    items ? items.split(';').map((value, index) => {
      return (
      listitems.push(
        {
          key: value,
          text: value
        }))
    })
      : listitems.push(
        {
          key: '',
          text: ''
        })
    return (listitems)

  };

  const _onFormatDate = (date?: Date): string => {
    if (date) {
      return (date.getMonth() + 1) + '/' + date.getDate() + '/' + (date.getFullYear() % 100);
    }
    let _date = new Date();
    return (_date.getMonth() + 1) + '/' + _date.getDate() + '/' + (_date.getFullYear() % 100);
  };

  const _onSelectDate = (date: Date | null | undefined, fieldKey: string, setFieldValue): void => {
    if (date) {
      let key = ((date.getMonth() + 1) + '/' + date.getDate() + '/' + (date.getFullYear() % 100));
      setFieldValue(fieldKey, key, true);
    }
  };

  const _GetSelectDate = (Parameter: any, fieldKey: string, setFieldValue) => {
    if (Parameter.dob) {
      return new Date(Parameter.dob)
    } else {
      let currentDate = new Date();
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      let key = ((currentDate.getMonth() + 1) + '/' + currentDate.getDate() + '/' + (currentDate.getFullYear() % 100));
      //setFieldValue(fieldKey, key, true);
    }
    return new Date();
  };

  const renderControls = (Parameter: IParameterModel, idx: number, handleChange, handleBlur, setFieldValue, touched) => {
    if (Parameter.valueType === 'String') {
      if (!Parameter.valueList || Parameter.valueList.length === 0) {
        return (
          <TextField
            id={`parameterValues.${idx}.${Parameter.parameterName}`}
            name={`parameterValues.${idx}.${Parameter.parameterName}`}
            onChange={handleChange}
            onBlur={handleBlur}
            // errorMessage={arrayItemErrorMessage(errors, touched, 'offerParameters', idx, 'description')}
            placeholder={Parameter.description} />)
      } else {
        return (
          <Dropdown options={dropDownValues(Parameter.valueList)}
            id={`parameterValues.${idx}.${Parameter.parameterName}`}
            onBlur={handleBlur} onChange={(event, option, index) => {
              selectOnChange(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, event, option, index);
            }} />)

      }
    } else if (Parameter.valueType === 'Number') {
      if (!Parameter.valueList || Parameter.valueList.length === 0) {
        if (Parameter.maximum && Parameter.maximum > 0) {
          return (
            <TextField
              id={`parameterValues.${idx}.${Parameter.parameterName}`}
              name={`parameterValues.${idx}.${Parameter.parameterName}`}
              onChange={handleChange}
              errorMessage={getFormErrorString(touched, `parameterValues.${idx}.${Parameter.parameterName}`, Parameter.minimum, Parameter.maximum)}
              onBlur={handleBlur}
              type="number"
              placeholder={Parameter.displayName} />)
        } else {
          return (
            <TextField
              name={`parameterValues.${idx}.${Parameter.parameterName}`}
              onChange={handleChange}
              onBlur={handleBlur}
              type="number"
              placeholder={Parameter.displayName} />)
        }
      } else {
        return (
          <Dropdown options={dropDownValues(Parameter.valueList)}
            id={`${Parameter.parameterName}`}
            onBlur={handleBlur} onChange={(event, option, index) => {
              selectOnChange(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, event, option, index);
            }} />)
      }

    } else if (Parameter.valueType === 'datetime') {
      return (

        <React.Fragment>
          <TextField name={`parameterValues.${idx}.${Parameter.parameterName}`} type='hidden' />
          <DatePicker
            className={''}
            strings={DayPickerStrings}
            showWeekNumbers={false}
            allowTextInput={true}
            value={_GetSelectDate(Parameter, `parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue)}
            formatDate={_onFormatDate}
            firstWeekOfYear={1}
            onSelectDate={(date) => {
              _onSelectDate(date, `parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue)
            }}
            showMonthPickerAsOverlay={true}
            placeholder="Select a date..."
            ariaLabel="Select a date"
          />
        </React.Fragment>
      )
    }
    else if (Parameter.valueType === 'boolean') {
      return (
        <React.Fragment>
          <input name={`parameterValues.${idx}.${Parameter.parameterName}`} id={`parameterValues.${idx}.${Parameter.parameterName}`} type='hidden' />
          <img src='/logo.png' alt="" onLoad={() =>
            setDefaultRBTValue(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, 'true')
          } style={{ display: 'none' }} />
          <ChoiceGroup
            className="defaultChoiceGroup"
            options={[
              {
                key: 'true',
                text: 'true',
                checked: true,
              },
              {
                key: 'false',
                text: 'false',
                checked: false
              }
            ]}
            onChange={(ev, option) => { _onChange(`parameterValues.${idx}.${Parameter.parameterName}`, setFieldValue, ev as React.FormEvent<HTMLInputElement>, option as IChoiceGroupOption) }}
            onLoad={() => { alert(0) }}
            label=""
            required={true}
          />
        </React.Fragment>
      )
    }
  }

  function _onChange(fieldKey: string, setFieldValue, ev: React.FormEvent<HTMLInputElement>, option: IChoiceGroupOption): void {
    let key = (option.key as string);
    setFieldValue(fieldKey, key, true);
    let hdf = document.getElementById(fieldKey) as HTMLElement;
    hdf.setAttribute('value', key);
  }

  function sleep (time) {
    return new Promise((resolve) => setTimeout(resolve, time));
  }
  
  function setDefaultRBTValue(fieldKey: string, setFieldValue, option: any): void {
    setFieldValue(fieldKey, option, true);
    let hdf = document.getElementById(fieldKey) as HTMLElement;
    hdf.setAttribute('value', option);
  }
  /*
  function copyText(text){
    let input = document.createElement("input");
  
    input.style.opacity="0";
    input.style["pointer-events"] = "none";
    document.body.appendChild(input);
    input.value = text;
    input.focus();
    input.select();
    document.execCommand('copy');
  }
*/
function copyText(text){
  let input = document.querySelector("#usercode");

  if (input && input instanceof HTMLInputElement){
    input.focus();
    input.select();
    document.execCommand('copy');
    input.blur();
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
          backgroundColor: theme.palette.neutralLight,
        }
      }}
    >
      <Stack
        horizontal={false}
        horizontalAlign="start"
        verticalAlign={"start"}
        verticalFill={true}
        styles={{
          root: {
            flexGrow: 1,
            width: '100%',
            maxWidth: 1234,
            backgroundColor: 'white',
            margin: '0 auto'
          }
        }}>
        {loadingFormData ?
          <Stack
            horizontalAlign="center"
            verticalAlign="center"
            verticalFill
            styles={{
              root: {
                width: '100%'
              }
            }}
          >
            <Loading />
          </Stack>
          :
          !formState || !formState.planName || formState.planName.length === 0 ?
            <span>Invalid Token</span>
            :
            <Formik
              initialValues={formState}
              validateOnBlur={true}
              // validationSchema={landingInfoValidationSchema}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                if (!accessTokenObtained){
                  toast.error("Access Token is not abtained.");
                  return;
                }
                globalContext.showProcessing();
                const input = { ...values };

                console.log(input);

                var result = qs.parse(location.search);
                if (!result.token) {
                  setLoadingFormData(false);
                  return;
                }
            
                var token = decodeURI(result.token as string);

                let subscriptionsModel = getInitialCreateSubscriptionModel();
                subscriptionsModel.Id = input.subscriptionId;
                subscriptionsModel.Name = input.subscriptionName;
                subscriptionsModel.OfferId = input.offerName;
                subscriptionsModel.PlanId = input.planName;
                subscriptionsModel.PublisherId = "";
                subscriptionsModel.Token = token;
                console.log('rendering items');
                console.log('param values: ', input.parameterValues);

                input.inputParameters.map((item, index) => {  
                  if (item.valueType === 'number') {
                    return (
                      subscriptionsModel.InputParameters.push(
                        {
                          name: item.parameterName,
                          type: item.valueType,
                         value: '"'+parseInt(input.parameterValues[index][item.parameterName])+'"'
                        }))
                  }
                  else{
                    return (
                    subscriptionsModel.InputParameters.push(
                      {
                        name: item.parameterName,
                        type: item.valueType,
                        value: input.parameterValues[index][item.parameterName]
                      }))
                  }
                })
                
                subscriptionsModel.InputParameters.push(
                  {
                    name: 'luna-jumpbox-access-token',
                    type: 'String',
                    value: input.accessToken
                  })

                let createSubscriptionsResult = await SubscriptionsService.create(subscriptionsModel);

                if (handleSubmissionErrorsForForm(setErrors,setSubmitting,setFormError,'landingpage',createSubscriptionsResult)) {
                  globalContext.hideProcessing();
                  return;
                }

                setSubmitting(false);
                globalContext.hideProcessing();
                toast.success("Success!");
                history.push(`Subscriptions`);
              }}
            >

              {({ isSubmitting, setFieldValue, values, handleChange, handleBlur, touched, errors }) => {
                console.log('values: ' + JSON.stringify(values));
                return (
                  <Form style={{ marginTop: 0, width: '100%' }} autoComplete={"off"}>                    
                    {formError && <MessageBar messageBarType={MessageBarType.error} style={{ marginBottom: 15 }}>
                      {{ formError }}
                    </MessageBar>}
                    <React.Fragment>
                      <div className="landingpagecontainner">
                        <div style={{ borderBottom: '1px solid #efefef', minHeight: '55px' }} className="headertitle">
                          <div style={{ textAlign: 'left' }}>
                            <span><h2>Configure and activate SaaS Subscription</h2></span>
                          </div>
                          <div style={{ textAlign: 'right' }}>
                          </div>
                          <div style={{ textAlign: 'left' }}>
                            <span><h4></h4></span>
                          </div>
                        </div>
                        <table className="mainlanding">
                          <tbody>
                            <tr>
                              <td>
                                <span><h4 style={{backgroundColor: '#dddddd', paddingLeft: '5px'}}>1. Review SaaS subscription info</h4></span>
                              </td>
                              <td>
                                <span><h4 style={{backgroundColor: '#dddddd', paddingLeft: '5px'}}>2. Fill in parameters</h4></span>
                              </td>
                              <td>
                                <span><h4 style={{backgroundColor: '#dddddd', paddingLeft: '5px'}}>3. Get Azure access token</h4></span>
                              </td>
                            </tr>
                            <tr>
                              <td>
                                <table className="mainlanding">
                                  <tbody>
                                    <tr>
                                      <td>
                                        <span><b>Email:</b></span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span>{values.email}</span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span><b>Subscriber full name:</b></span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span>{values.fullName}</span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span><b>Offer Id:</b></span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span>{values.offerName}</span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span><b>Current Plan:</b></span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td><span>{values.planName}</span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span><b>SaaS Subscription ID:</b></span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span>{values.subscriptionId}</span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span><b>SaaS Subscription Name:</b></span>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td>
                                        <span>{values.subscriptionName}</span>
                                      </td>
                                    </tr>
                                  </tbody>
                                </table>
                              </td>
                              <td>
                        <table className="mainlanding">
                          <tbody>
                            {values.inputParameters ?
                              values.inputParameters.map((item, index) => {
                                return (
                                  <tr key={index}>
                                    <td>
                                      <span><b>{item.displayName}</b></span>
                                      {
                                        renderControls(item, index, handleChange, handleBlur, setFieldValue, touched)
                                      }
                                    </td>
                                  </tr>
                                )
                              })
                              : null}
                              </tbody>
                              </table>
                              </td>
                              <td>
                        <table className="mainlanding">
                          <tbody>
                              <tr>
                                <td>
                                  <span><b>3.1 Sign in to Azure CLI with device code</b></span>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                <PrimaryButton type="button" id="btnsubmit" className="signInbutton"
                onClick={(e) => {
                  if (values.userCode){
                    copyText(values.userCode)
                    toast.success("Code copied! Opening login page...")
                    sleep(3000).then(() => {
                      window.open("https://microsoft.com/devicelogin", "_blank")
                      setEnableGetAccessToken(true)
                    })
                  }
                }}>
                <FontIcon iconName="Signin" className="signinicon" />Copy code and sign in
              </PrimaryButton>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <span><FormLabel title={"If the link above does not work, "}/><br/>
                                    <FormLabel title={"use a web browser to open the page "}/><br/>
                                  <a href="https://microsoft.com/devicelogin" target="_blank">https://microsoft.com/devicelogin</a><br/>
                                  <FormLabel title={"and enter following code "}/></span>
                                                        
                                  <TextField
                                    id={`usercode`}
                                    value={values.userCode}
                                    borderless={true}
                                    style={{paddingLeft: '0px', height: '15px'}}/>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <span><b>3.2 Get Azure access token</b></span>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <span><PrimaryButton disabled={!enableGetAccessToken} type="button" id="btnsubmit" className="signInbutton"
                onClick={() => {
                  if (values.deviceCode){
                    SubscriptionsService.getAccessToken(values.deviceCode).then(response => { 
                      if (response.value) {
                        values.accessToken = response.value.access_token
                        toast.success("Access token obtained!")
                        setAccessTokenObtained(true)
                      }
                      })
                  }

                }}>
                <FontIcon iconName="PasswordField" className="signinicon" />Get Access Token
              </PrimaryButton>
              </span>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <span><b>3.3 Activate your SaaS subscription</b></span>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <span>
                            <PrimaryButton disabled={!accessTokenObtained} type="submit" id="btnsubmit" className="signInbutton">
                              <FontIcon iconName="PasswordField" className="PowerButton" />Activate</PrimaryButton></span>
                                </td>
                              </tr>
                          </tbody>
                        </table>
                              </td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </React.Fragment>
                  </Form>);
              }}
            </Formik>
        }
      </Stack>
    </Stack>
  );
};

export default LandingPage;