import { Formik, useFormikContext } from 'formik';
import { DefaultButton, Dropdown, IconButton, IDropdownOption, Link, Pivot, PivotItem, PrimaryButton, Stack, TextField } from 'office-ui-fabric-react';
import React, { useEffect, useState } from 'react';
import { useHistory } from "react-router";
import AlternateButton from '../shared/components/AlternateButton';
import {initialWizardFormValues, IWizardFormValues, wizardFormValidationSchema } from '../routes/Wizard/formUtils/wizardFormUtils'
import { useGlobalContext } from '../shared/components/GlobalProvider';
import { toast } from 'react-toastify';
import adalContext from '../adalConfig';
import WizardService from '../services/WizardService'
// import { useGlobalContext } from "../../shared/components/GlobalProvider";


const WizardContent: React.FunctionComponent = (props) => {
  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars  
  const [serviceTypeList, setServiceTypeList] = useState<IDropdownOption[]>([]);
  const [serviceList, setServiceList] = useState<IDropdownOption[]>([]);
  const [mLComponentTypeList, setMLComponentTypeList] = useState<IDropdownOption[]>([]);
  const [MLComponentList, setMLComponentList] = useState<IDropdownOption[]>([]);
  const [sourceServiceTypeList, setSourceServiceTypeList] = useState<IDropdownOption[]>([]);
  const [sourceServiceList, setSourceServiceList] = useState<IDropdownOption[]>([]);
  const [computeServiceTypeList, setComputeServiceTypeList] = useState<IDropdownOption[]>([]);
  const [computeServiceList, setComputeServiceList] = useState<IDropdownOption[]>([]);
  const [formState, setFormState] = useState<IWizardFormValues>(initialWizardFormValues);
  const [selectedKey, setSelectedKey] = React.useState(0);
  const [isExpand, setIsExpand] = React.useState(true);
  const [isGitHubSelected, setGitHubSelected] = React.useState(false);

  let userName = "";  
  var response = adalContext.AuthContext.getCachedUser();
  if (response && response.profile && response.profile.name)
    userName = response.profile.name;

  // const { values, handleChange, handleBlur, touched, errors, handleSubmit, setFieldValue } = useFormikContext<IWizardFormValues>(); // formikProps
  const getServiceTypes = async () => {

    let serviceTypeList: IDropdownOption[] = [];
    let serviceTypeResponse = await WizardService.serviceTypesList();
    if (serviceTypeResponse.value && serviceTypeResponse.success) {
      var serviceTypes = serviceTypeResponse.value;
      serviceTypes.map((item, index) => {
        return (
          serviceTypeList.push(
            {
              key: item.Id,
              text: item.DisplayName
            })
        )
      })
    }    
    setServiceTypeList([...serviceTypeList]);
  }

  const getServices = async () => {

    let serviceList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    serviceList.push(
      {
        key: "MyAMLWorkspaces",
        text: "My AML Workspaces"
      })
    setServiceList([...serviceList]);
  }
  const getSourceServiceType = async () => {

    let sourceServiceTypeList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    sourceServiceTypeList.push(
      {
        key: "GitHub",
        text: "GitHub"
      })
    setSourceServiceTypeList([...sourceServiceTypeList]);
  }
  const getSourceService = async () => {

    let sourceServiceList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    sourceServiceList.push(
      {
        key: "MyGitRepo",
        text: "My Git Repo"
      })
    setSourceServiceList([...sourceServiceList]);
  }
  const getMLComponentType = async () => {

    let mLComponentTypeList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    mLComponentTypeList.push(
      {
        key: "Published Realtime Endpoints",
        text: "Published Realtime Endpoints"
      })
    setMLComponentTypeList([...mLComponentTypeList]);
  }
  const getMLComponent = async () => {

    let mLComponentList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    mLComponentList.push(
      {
        key: "FakeNewsDetection",
        text: "Fake News Detection"
      })
    setMLComponentList([...mLComponentList]);
  }
  const getComputeServiceType = async () => {

    let computeServiceTypeList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    computeServiceTypeList.push(
      {
        key: "AzureSynapseWorkspace",
        text: "Azure Synapse Workspace"
      })
    setComputeServiceTypeList([...computeServiceTypeList]);
  }
  const getComputeService = async () => {

    let computeServiceList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    computeServiceList.push(
      {
        key: "MySynapseWorkspace",
        text: "My Synapse Workspace"
      })
    setComputeServiceList([...computeServiceList]);
  }
  useEffect(() => {
    getServiceTypes();
    getServices();
    getSourceServiceType();
    getSourceService();
    getMLComponentType();
    getMLComponent();
    getComputeServiceType();
    getComputeService();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
  
  const onNextButtonClick = () => {
    setSelectedKey((selectedKey + 1) % 3);
  };
  const onBackButtonClick = () => {
    setSelectedKey((selectedKey - 1) % 3);
  };
  const ExpandCollapseClick = () => {
    setIsExpand(!isExpand);
  }
  const DisplayErrors = (errors) => {
    console.log('display errors:');
    console.log(errors);
    
    return null;
  }

  const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      let key = (option.key as string);
      setFieldValue(fieldKey, key, true); 
      if(key === 'GitHub')       
      {
        setGitHubSelected(true);
      }   
      else
      {
        setGitHubSelected(false);
      }
    }
  };
  const getFormErrorString = (touched, errors, property: string) => {
    return touched.wizard && errors.wizard && touched.wizard[property] && errors.wizard[property] ? errors.wizard[property] : '';
  };
  return (
    <Stack
      verticalAlign="start"
      horizontal={false}
      styles={{
        root: {
          width: '87%',
          height: '100%',
          textAlign: 'left',
          margin: '0 auto',
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
        <React.Fragment>
        <Formik
            initialValues={initialWizardFormValues}
            validationSchema={wizardFormValidationSchema}
            enableReinitialize={true}
            validateOnBlur={true}
            onSubmit={async (values, { setSubmitting, setErrors }) => {    
              toast.success("Success!");         
            }}
          >
        {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
        // <form style={{ width: '100%' }} autoComplete={"off"} onSubmit={handleSubmit}>
        // <DisplayErrors errors={errors} />
        <React.Fragment>
          <Stack>
            <h1>Luna Application Wizard</h1>
            <Pivot selectedKey={String(selectedKey)}>
              <PivotItem headerText="1. Machine Learnig Services" itemKey="0">
                <h3>Step 1: Choose machine learning service or code repo</h3>
                <label>Tell us where are your machine learning components(models,endpoints,project...)</label>
                <table>
                  <tr>
                    <td >
                      Source Service Type:
                        </td>
                    <td>
                      <Dropdown style={{ width: '220px' }} 
                      id={`wizard.sourceServiceType`}                                                            
                      options={serviceTypeList}                        
                      placeHolder="Select Service Type"
                      defaultSelectedKey={values.wizard.sourceServiceType}
                      onBlur={handleBlur} 
                      onChange={(event, option, index) => {
                        selectOnChange(`wizard.sourceServiceType`, setFieldValue, event, option, index);}}      
                      errorMessage={getFormErrorString(touched, errors, 'sourceServiceType')} />
                    </td>
                  </tr>
                  <tr>
                    <td>
                      Source Service:
                        </td>
                    <td>
                      <Dropdown 
                      id={`wizard.sourceService`}
                      options={serviceList} 
                      placeHolder="Select Service"
                      defaultSelectedKey={values.wizard.sourceService} 
                      onBlur={handleBlur}      
                      onChange={(event, option, index) => {
                        selectOnChange(`wizard.sourceService`, setFieldValue, event, option, index);}}      
                      errorMessage={getFormErrorString(touched, errors, 'sourceService')}
                      />
                    </td>
                  </tr>
                  <tr>
                    <td>

                    </td>
                    <td>
                      <a href="#">Register New</a>
                    </td>
                  </tr>
                  <tr>
                    <td>

                    </td>
                    <td>

                    </td>
                    <td colSpan={2}>
                      <Stack>
                        <div style={{ width: '350px', background: '#d3d3d3', height: '250px', marginTop: '-96px', marginLeft: '25px' }}>
                          <p />
                          <p />
                          <p />
                          <div style={{ padding: '10px' }}><IconButton iconProps={{ iconName: 'Lightbulb' }} />Tips:
                            <ul>
                              <li><p>Don’t see your machine learning service or Git repo? Click on “Register New” to register your services.</p></li>
                              <li><p>Learn more about how to register a partner service.</p></li></ul></div>
                        </div>
                      </Stack>
                    </td>
                  </tr>
                  <tr>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td style={{ textAlign: 'right' }}>
                      <AlternateButton style={{ margin: '20px' }}>Cancel</AlternateButton>
                      <PrimaryButton style={{ margin: '20px' }} onClick={onNextButtonClick}>Next</PrimaryButton>
                    </td>
                  </tr>
                </table>
              </PivotItem>
              <PivotItem headerText="2. Machine Learning Components" itemKey="1">
                <h3>Step 2: Choose machine learning components</h3>
                <label>Tell us where are your machine learning components (models,endpoints,project...) you want to publish as API applications.</label>
                <table>
                  <tbody>
                  <tr style={{ ...!isGitHubSelected ? { display: 'table-row' } : { display: 'none' } }}>
                    <td>
                      ML Component Type:
                        </td>
                    <td>
                      <Dropdown 
                      id={`wizard.mLComponentType`}
                      style={{ width: '220px' }} 
                      options={mLComponentTypeList} 
                      placeHolder="Select Service Type"
                      defaultSelectedKey={values.wizard.mLComponentType}
                      onBlur={handleBlur}      
                      onChange={(event, option, index) => {
                        selectOnChange(`wizard.mLComponentType`, setFieldValue, event, option, index);}}      
                      errorMessage={getFormErrorString(touched, errors, 'mLComponentType')} />
                    </td>
                  </tr>                  
                  <tr style={{ ...!isGitHubSelected ? { display: 'table-row' } : { display: 'none' } }}>
                    <td>
                      ML Component:
                        </td>
                    <td>
                      <Dropdown 
                      id={`wizard.mLComponent`}
                      options={MLComponentList} 
                      placeHolder="Select Service"
                      defaultSelectedKey={values.wizard.mLComponent} 
                      onBlur={handleBlur}    
                      onChange={(event, option, index) => {
                        selectOnChange(`wizard.mLComponent`, setFieldValue, event, option, index);}}        
                      errorMessage={getFormErrorString(touched, errors, 'mLComponent')}/>
                    </td>
                  </tr>
                  <tr style={{ ...!isGitHubSelected ? { display: 'table-row' } : { display: 'none' } }}>
                    <td>
                      Operation Name:
                        </td>
                    <td>
                      <TextField 
                      name={'wizard.operationName'}
                      placeholder={"Operation Name"}
                      value={values.wizard.operationName} 
                      onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'operationName')} />
                    </td>
                  </tr>                  
                  <tr style={{ ...isGitHubSelected ? { display: 'table-row' } : { display: 'none' } }}>
                    <td>
                    Branch or Commit Hash:
                        </td>
                    <td>
                    <TextField 
                      name={'wizard.branchOrCommitHash'}
                      placeholder={"Branch or Commit Hash"}
                      value={values.wizard.branchOrCommitHash} 
                      onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'branchOrCommitHash')} />
                    </td>
                  </tr> 
                  <tr style={{ ...isGitHubSelected ? { display: 'table-row' } : { display: 'none' } }}>
                    <td>
                    Execution Config File:
                        </td>
                    <td>
                    <TextField 
                      name={'wizard.executionConfigFile'}
                      placeholder={"Execution Config File"}
                      value={values.wizard.executionConfigFile} 
                      onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'executionConfigFile')} />
                    </td>
                  </tr> 
                  <tr style={{ ...isGitHubSelected ? { display: 'table-row' } : { display: 'none' } }}>
                    <td>
                    Compute Service Type:
                        </td>
                    <td>
                      <Dropdown 
                      id={`wizard.computeServiceType`}
                      style={{ width: '220px' }} 
                      options={computeServiceTypeList} 
                      placeHolder="Select Compute Service Type"
                      defaultSelectedKey={values.wizard.computeServiceType}
                      onBlur={handleBlur}      
                      onChange={(event, option, index) => {
                        selectOnChange(`wizard.computeServiceType`, setFieldValue, event, option, index);}}      
                      errorMessage={getFormErrorString(touched, errors, 'computeServiceType')} />
                    </td>
                  </tr> 
                  <tr style={{ ...isGitHubSelected ? { display: 'table-row' } : { display: 'none' } }}>
                    <td>
                    Compute Service:
                        </td>
                    <td>
                      <Dropdown 
                      id={`wizard.computeService`}
                      style={{ width: '220px' }} 
                      options={computeServiceList} 
                      placeHolder="Select Compute Service"
                      defaultSelectedKey={values.wizard.computeService}
                      onBlur={handleBlur}      
                      onChange={(event, option, index) => {
                        selectOnChange(`wizard.computeService`, setFieldValue, event, option, index);}}      
                      errorMessage={getFormErrorString(touched, errors, 'computeService')} />
                    </td>
                  </tr> 
                  <tr>
                    <td>

                    </td>
                    <td>

                    </td>
                    <td colSpan={2}>
                      <Stack>
                        <div style={{ width: '350px', background: '#d3d3d3', height: '250px', marginTop: '-115px', marginLeft: '25px' }}>
                          <p />
                          <p />
                          <p />
                          <div style={{ padding: '10px' }}>
                            <IconButton iconProps={{ iconName: 'Lightbulb' }} /> Tips:
                            <ul>
                              <li><p>This will be a part of the URL when user calling the API service.</p></li>
                              <li><p>Allows only lowercase characters, numbers and hyphen, start with lowercase characters and 128 characters max.</p></li></ul></div>
                        </div>
                      </Stack>
                    </td>
                  </tr>
                  <tr>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td style={{ textAlign: 'right' }}>
                      <AlternateButton style={{ margin: '20px' }}>Cancel</AlternateButton>
                      <PrimaryButton style={{ margin: '20px' }} onClick={onBackButtonClick}>Back</PrimaryButton>
                      <PrimaryButton style={{ margin: '20px' }} onClick={onNextButtonClick}>Next</PrimaryButton>
                    </td>
                  </tr>   
                  </tbody>              
                </table>
              </PivotItem>
              <PivotItem headerText="3. API Services" itemKey="2">
                <h3>Step 3: Configure your API service</h3>
                <label>Name your application, API and API version.</label>
                <table>
                  <tr>
                    <td>
                      Application Display Name:
                        </td>
                    <td>
                      <TextField 
                      placeholder={"Application Display Name"} 
                      style={{ width: '220px' }}
                      name={'wizard.applicationDisplayName'}
                      value={values.wizard.applicationDisplayName} 
                      onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'applicationDisplayName')} />
                    </td>
                  </tr>
                  <tr>
                    <td>
                      Application Name:
                        </td>
                    <td>
                      <TextField 
                      placeholder={"Application Name"}
                      name={'wizard.applicationName'}
                      value={values.wizard.applicationName}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'applicationName')} />
                    </td>
                  </tr>
                  <tr>
                    <td>
                      API Name:
                        </td>
                    <td>
                      <TextField 
                      placeholder={"API Name"}
                      name={'wizard.apiName'}
                      value={values.wizard.apiName} 
                      onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'apiName')} />
                    </td>
                  </tr>
                  <tr>
                    <td>
                      API Version:
                        </td>
                    <td>
                      <TextField 
                      placeholder={"API Version"} 
                      name={'wizard.apiVersion'}
                      value={values.wizard.apiVersion} 
                      onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'apiVersion')} />
                    </td>
                  </tr>
                  {/* <div style={{ ...isExpand ? { display: 'none' } : { display: 'block' } }}> Advance Settings <IconButton iconProps={{ iconName: 'ChevronUpMed' }} title="Collapse" ariaLabel="Collapse" onClick={ExpandCollapseClick} /> */}
                  <tr style={{ ...isExpand ? { display: 'none' } : { display: 'table-row' } }}>
                  Advance Settings <IconButton iconProps={{ iconName: 'ChevronUpMed' }} title="Collapse" ariaLabel="Collapse" onClick={ExpandCollapseClick} /> 
                  </tr>
                    <tr style={{ ...isExpand ? { display: 'none' } : { display: 'table-row' } }}>                    
                      <td>
                        Application Description:
                    </td>
                      <td>
                        <TextField 
                        placeholder={"Application Description:"} 
                        style={{ width: '220px' }}
                        name={'wizard.applicationDescription'}
                        value={values.wizard.applicationDescription} 
                        onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'applicationDescription')} />
                      </td>
                    </tr>
                    <tr style={{ ...isExpand ? { display: 'none' } : { display: 'table-row' } }}>
                      <td>
                        Logo Image URL:
                    </td>
                      <td>
                        <TextField 
                        placeholder={"Logo Image URL"}
                        name={'wizard.logoImageURL'}
                        value={values.wizard.logoImageURL} 
                        onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'logoImageURL')} />
                      </td>
                    </tr>
                    <tr style={{ ...isExpand ? { display: 'none' } : { display: 'table-row' } }}>
                      <td>
                        Documentation URL:
                    </td>
                      <td>
                        <TextField 
                        placeholder={"Documentation URL"} 
                        name={'wizard.documentationURL'}
                        value={values.wizard.documentationURL} 
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'documentationURL')} />
                      </td>
                    </tr>
                    <tr style={{ ...isExpand ? { display: 'none' } : { display: 'table-row' } }}>
                      <td>
                        Publisher:
                    </td>
                      <td>
                        <TextField 
                        placeholder={"Publisher"} 
                        name={'wizard.publisher'}
                        value={userName} 
                        onChange={handleChange}
                      onBlur={handleBlur}
                      errorMessage={getFormErrorString(touched, errors, 'publisher')} />
                      </td>
                    </tr>
                  {/* </div> */}
                  <tr style={{ ...isExpand ? { display: 'table-row' } : { display: 'none' } }}> Advance Settings <IconButton iconProps={{ iconName: 'ChevronDownMed' }} title="Expand" ariaLabel="Expand" onClick={ExpandCollapseClick} />
                  </tr>
                  <tr>
                    <td>

                    </td>
                    <td>

                    </td>
                    <td colSpan={2}>
                      <Stack>
                        <div style={{ width: '350px', background: '#d3d3d3', height: '250px', marginTop: '-175px', marginLeft: '25px' }}>
                          <p />
                          <p />
                          <p />
                          <div style={{ padding: '10px' }}><IconButton iconProps={{ iconName: 'Lightbulb' }} />Tips:
                            <ul>
                              <li><p>A descriptive name will help user to understand what your application is for. For example: Text Analysis, Sales Forecasting.</p></li>                              </ul></div>
                        </div>
                      </Stack>
                    </td>
                  </tr>
                  <tr>
                    <td></td>
                    <td style={{display:'inline-flex'}}>
                    <AlternateButton style={{ margin: '20px' }}>Cancel</AlternateButton>
                      <PrimaryButton style={{ margin: '20px' }} onClick={onBackButtonClick}>Back</PrimaryButton>
                    </td>
                    <td colSpan={2} style={{ textAlign: 'left' }}>
                      
                      <PrimaryButton text={"Create"} style={{ margin: '20px' }} onClick={submitForm}></PrimaryButton>
                      <PrimaryButton text={"Create & Publish"} style={{ margin: '20px' }} onClick={submitForm}></PrimaryButton>
                    </td>
                  </tr>
                </table>
              </PivotItem>
            </Pivot>
          </Stack>
        </React.Fragment>
        // </form>
         )}
        </Formik>
        </React.Fragment>
      </Stack>
    </Stack>
  );
}

export default WizardContent;