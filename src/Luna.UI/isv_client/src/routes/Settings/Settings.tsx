import React, { useEffect, useState } from 'react';
import {
  Checkbox,
  DefaultButton,
  Dialog,
  DialogFooter,
  DialogType,
  FontIcon,
  Label,
  Pivot,
  PivotItem,
  PrimaryButton,
  Stack,
  TextField,
  Dropdown,
  IDropdownOption
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { Formik } from "formik";
import { IAutomationWebhookModel, IGitRepoModel, IPartnerServiceModel, IPermissionsModel } from "../../models";
import { Loading } from "../../shared/components/Loading";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import AlternateButton from '../../shared/components/AlternateButton';
// import {  
//   IAMLWorkSpaceFormValues,
//   initialAMLWorkSpaceFormValues,
//   initialAMLWorkSpaceValues,  
// } from '../Products/formUtils/AMLWorkSpaceUtils';
import {
  partnerServiceFormValidationSchema,
  IPartnerServiceFormValues,
  initialPartnerServiceFormValues,
  initialPartnerServiceValues,
} from '../Products/formUtils/PartenerServiceUtils';
import {
  automationWebhookFormValidationSchema,
  IAutomationWebhookFormValues,
  initialAutomationWebhookFormValues,
  initialAutomationWebhookValues,
} from '../Products/formUtils/AutomationWebhookUtils';
import {
  permissionsFormValidationSchema,
  IPermissionsFormValues,
  initialPermissionsFormValues,
  initialPermissionsValues
} from '../Products/formUtils/PermissionsUtils'
import { Hub } from "aws-amplify";
import SettingService from "../../services/SettingsService";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { SettingsMessages } from '../../shared/constants/infomessages';
import { DialogBox } from '../../shared/components/Dialog';

const Settings: React.FunctionComponent = () => {

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const globalContext = useGlobalContext();
  //const [formError, setFormError] = useState<string | null>(null);

  return (
    <Stack
      horizontalAlign="start"
      verticalAlign="start"
      verticalFill
      styles={{
        root: {
          width: '100%',
          margin: 31
        }
      }}
      gap={15}
    >
      <SettingList />
    </Stack>
  );
};

export type ISettingsListProps = {}
export const SettingList: React.FunctionComponent<ISettingsListProps> = (props) => {
  //const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IAMLWorkSpaceListProps>(); // formikProps
  //const { } = props;
  let [gitRepoList, setGitRepoList] = useState<IGitRepoModel[]>();
  let [partnerServiceList, setPartnerServiceList] = useState<IPartnerServiceModel[]>();
  let [automationWebhookList, setAutomationWebhookList] = useState<IAutomationWebhookModel[]>();  
  let [permissionsList, setPermissionsList] = useState<IPermissionsModel[]>([]);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  let [workSpaceDeleteIndex, setworkSpaceDeleteIndex] = useState<number>(0);  
  const [loadingGitRepo, setLoadingGitRepo] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);  
  const [isEdit, setisEdit] = useState<boolean>(true);

  const [automationWebhookDialogVisible, setAutomationWebhookDialogVisible] = useState<boolean>(false);
  const [loadingAutomationWebhook, setLoadingAutomationWebhook] = useState<boolean>(false);
  const [typeList, setTypeList] = useState<IDropdownOption[]>([]);
  const [isDisplayUpdateButton, setDisplayUpdateButton] = useState<boolean>(false);
  let [partnerService, setPartnerService] = useState<IPartnerServiceFormValues>(initialPartnerServiceFormValues);
  let [automationWebhook, setAutomationWebhook] = useState<IAutomationWebhookFormValues>(initialAutomationWebhookFormValues);

  const [partnerServiceDialogVisible, setPartnerServiceDialogVisible] = useState<boolean>(false);
  const [loadingPartnerService, setLoadingPartnerService] = useState<boolean>(false);
  const [newUserDialogVisible, setNewUserDialogVisible] = useState<boolean>(false);
  const [loadingPermissions, setLoadingPermissions] = useState<boolean>(false);
  const [roleList, setRoleList] = useState<IDropdownOption[]>([]);
  let [permissions, setPermissions] = useState<IPermissionsFormValues>(initialPermissionsFormValues);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const globalContext = useGlobalContext();

  const getGitRepoList = async () => {

    setLoadingGitRepo(true);
    const results = await SettingService.getGitRepoList();
    if (results && results.value && results.success) {
      setGitRepoList(results.value);
      setLoadingGitRepo(false);
    } else
      toast.error('Failed to load Git repos');
  }

  const getPartnerServiceList = async () => {

    setLoadingPartnerService(true);
    const results = await SettingService.getPartnerServicesList();
    results.value = [{type:'Azure Synopsis',resourceId:'123',tenantId:'456',clientId:'789',clinetSecrets:'123456789',partnerServiceName:'ABC',createdDate:'02//05/2021'}]
    results.success= true;
    if (results && results.value && results.success) {
      setPartnerServiceList(results.value);
      setLoadingPartnerService(false);
    } else
      toast.error('Failed to load Product Services');
  }

  const getAutomationWebhookList = async () => {

    setLoadingAutomationWebhook(true);
    const results = await SettingService.getAutomationWebhooksList();
    results.value = [{name:'Azure Synopsis',webhookURL:'/webhooks/Mywebhook', enabled:true,createdDate:'02//05/2021',clientId:'123'}]
    results.success= true;
    if (results && results.value && results.success) {
      setAutomationWebhookList(results.value);
      setLoadingAutomationWebhook(false);
    } else
      toast.error('Failed to load Automation Webhooks');
  }
  const getTypes = async () => {

    let typeList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];
    // let TypeResponse = await TypeService.list();
    // if (TypeResponse.value && TypeResponse.success) {
    //   var Types = TypeResponse.value;
    //   Types.map((item, index) => {
    //     return (
    //       typeList.push(
    //         {
    //           key: item.planName,
    //           text: item.planName
    //         })
    //     )
    //   })
    // }
    typeList[0].text="Select Type";
    typeList.push({
      key: "Azure Synapse",
      text: "Azure Synapse"
    });
    typeList.push({
      key: "Azure Machine Learning",
      text: "Azure Machine Learning"
    })
    setTypeList([...typeList]);
  }

  const getPermissionsList = async () => {
    setLoadingPermissions(true);
    const results = await SettingService.getPermissions();
    results.value = [
      {userId: 'Lindsey Allen', role: 'Administrator', clientId: '123', createdDate:'04/22/2021'}, 
      {userId: 'Xiaochen Wu', role: 'Publisher', clientId: '456', createdDate:'04/22/2021'},
      {userId: 'Sophie Hu', role: 'Reviewer', clientId: '789', createdDate:'04/22/2021'},
    ]
    results.success = true;
    if (results && results.value && results.success) {
      setPermissionsList(results.value);
      setLoadingPermissions(false);
    } else
      toast.error('Failed to load Permissions');
  }

  const getRoles = async () => {
    let roleList: IDropdownOption[] = [{
      key: '',
      text: ''
    }];

    roleList[0].text="Select Role";
    roleList.push({
      key: "Administrator",
      text: "Administrator"
    });
    roleList.push({
      key: "Publisher",
      text: "Publisher"
    });
    roleList.push({
      key: "Reviewer",
      text: "Reviewer"
    });
    setRoleList([...roleList]);
  }
  
  const getPermissionsFormErrorString = (touched, errors, property: string) => {
    var retobj =  touched.permissions && errors.permissions && touched.permissions[property] && errors.permissions[property] ? errors.permissions[property] : '';
    return retobj;
  };

  const getFormErrorString = (touched, errors, property: string) => {
    return touched.aMLWorkSpace && errors.aMLWorkSpace && touched.aMLWorkSpace[property] && errors.aMLWorkSpace[property] ? errors.aMLWorkSpace[property] : '';
  };

  const getPartnerServiceFormErrorString = (touched, errors, property: string) => {
    return touched.partnerService && errors.partnerService && touched.partnerService[property] && errors.partnerService[property] ? errors.partnerService[property] : '';
  };

  const getAutomationWebhookFormErrorString = (touched, errors, property: string) => {
    return touched.automationWebhook && errors.automationWebhook && touched.automationWebhook[property] && errors.automationWebhook[property] ? errors.automationWebhook[property] : '';
  };

  const editPartnerService = async (partnerServiceName: string, idx: number) => {

    //let editedWorkspace = initialAMLWorkSpaceList.filter(a => a.workspaceName === Id)[0];
    let editPartnerService = await SettingService.getPartnerServiceByName(partnerServiceName);
    if (editPartnerService && editPartnerService.value && editPartnerService.success) {
      setPartnerService({ partnerService : editPartnerService.value });
      // setworkSpaceDeleteIndex(idx);
    } else
      toast.error('Failed to load Partner Service');

    setisEdit(true);
    setDisplayUpdateButton(true);
    OpenPartnerServiceDialog();
    //history.push(WebRoute.ModifyProductInfo.replace(':productName', productName));
  };

  const editAutomationWebhook = async (automationWebhookName: string, idx: number) => {

    //let editedWorkspace = initialAMLWorkSpaceList.filter(a => a.workspaceName === Id)[0];
    let editAutomationWebhook = await SettingService.getAutomationWebhookByName(automationWebhookName);
    if (editAutomationWebhook && editAutomationWebhook.value && editAutomationWebhook.success) {
      setAutomationWebhook({ automationWebhook : editAutomationWebhook.value });
      // setworkSpaceDeleteIndex(idx);
    } else
      toast.error('Failed to load Automation Webhook');

    setisEdit(true);
    setDisplayUpdateButton(true);
    OpenAutomationWebhookDialog();
    //history.push(WebRoute.ModifyProductInfo.replace(':productName', productName));
  };
  
  const OpenPartnerServiceDialog = () => {
    setPartnerServiceDialogVisible(true);
  }

  const OpenNewPartnerServiceDialog = () => {
    setDisplayUpdateButton(false);
    setPartnerServiceDialogVisible(true);
  }

  const OpenAutomationWebhookDialog = () => {
    setAutomationWebhookDialogVisible(true);
  }

  const OpenNewAutomationWebhookDialog = () => {
    setDisplayUpdateButton(false);
    setAutomationWebhookDialogVisible(true);
  }

  const OpenNewUserServiceDialog = () => {
    setNewUserDialogVisible(true);
  }

  const ClosePartnerServiceDialog = () => {
    setPartnerServiceDialogVisible(false);
  }

  const CloseAutomationWebhookDialog = () => {
    setAutomationWebhookDialogVisible(false);
  }

  const CloseNewUserServiceDialog = () => {
    setNewUserDialogVisible(false);
  }

  useEffect(() => {
    
    getGitRepoList();
    getPartnerServiceList();
    getAutomationWebhookList();
    getTypes();
    getPermissionsList();
    getRoles();

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const AutomationWebhooksList = ({ automationWebhook }) => {
    if (!automationWebhook || automationWebhook.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Automation Webhooks</span></td>
      </tr>;
    } else {
      return (
        automationWebhook.map((value: IAutomationWebhookModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span>{value.name}</span>
              </td>
              <td>
                <span>{value.enabled ? 'Enabled' : 'Disabled'}</span>
              </td>
              <td>
                <span>{value.createdDate}</span>
              </td>
              <td>
                <Stack
                  verticalAlign="center"
                  horizontalAlign={"space-evenly"}
                  gap={15}
                  horizontal={true}
                >
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => {
                    editAutomationWebhook(value.name, idx)
                  }} />
                  {/* <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteWorkSpace(value) }} /> */}
                </Stack>
              </td>
            </tr>
          );
        })
      );

    }
  }

  const GitRepoList = ({ gitRepo }) => {
    if (!gitRepo || gitRepo.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Git repo</span></td>
      </tr>;
    } else {
      return (
        gitRepo.map((value: IGitRepoModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span>{value.repoName}</span>
              </td>
              <td>
                <span>{value.httpUrl}</span>
              </td>
              <td>
                <Stack
                  verticalAlign="center"
                  horizontalAlign={"space-evenly"}
                  gap={15}
                  horizontal={true}
                >
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => {
                    //editWorkSpace(value.repoName, idx)
                  }} />
                  {/* <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteWorkSpace(value) }} /> */}
                </Stack>
              </td>
            </tr>
          );
        })
      );

    }
  }

  const PartnerServiceList = ({ partnerService }) => {
    if (!partnerService || partnerService.length === 0) {
      return <tr>
        <td colSpan={4}><span>No Partner Service</span></td>
      </tr>;
    } else {
      return (
        partnerService.map((value: IPartnerServiceModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span>{value.partnerServiceName}</span>
              </td>
              <td>
                <span>{value.type}</span>
              </td>
              <td>
                <span>{value.createdDate}</span>
              </td>
              <td>
                <Stack
                  verticalAlign="center"
                  horizontalAlign={"space-evenly"}
                  gap={15}
                  horizontal={true}
                >
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => {
                    editPartnerService(value.partnerServiceName, idx)
                  }} />
                  {/* <FontIcon iconName="Cancel" className="deleteicon" onClick={() => { deleteWorkSpace(value) }} /> */}
                </Stack>
              </td>
            </tr>
          );
        })
      );

    }
  }

  const DeletePermission = async (values:IPermissionsModel, index) => {
    
    globalContext.showProcessing();

    // var DeletePermissionResult = await SettingService.deletePermission(values);
    // if (handleSubmissionErrorsForForm(null, null, setFormError, 'delete permission', DeletePermissionResult)) {
    //   toast.error(formError)
    //   globalContext.hideProcessing();
    //   return;
    // }        
    setPermissionsList(permissionsList.filter(x=>x.userId!=values.userId));
    globalContext.hideProcessing();
    toast.success("Success!");    
  }

  const PermissionsList = ({ permissions, role }) => {
    if(!permissions || permissions.length === 0) {
      return <tr>
      <td colSpan={4}><span>No Permissions</span></td>
    </tr>;
    } else {
      return (
        permissions.filter(p => p.role === role).map((value: IPermissionsModel, idx) => {
          return (
            <tr key={idx}>
            <td>
              <span>{value.userId}</span>
            </td>
            <td>
              <span>{value.createdDate}</span>
            </td>
            <td>
              <Stack
                verticalAlign="center"
                horizontalAlign={"space-evenly"}
                gap={15}
                horizontal={true}
              >
              <FontIcon iconName="Cancel" className="deleteicon" onClick={() => {DeletePermission(value,idx)}} />
              </Stack>
            </td>
          </tr>
          )
        })
      )
    }
  }

  const selectOnChange = (fieldKey: string, setFieldValue, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      let key = (option.key as string);      
      setFieldValue(fieldKey, key, true);
    }
  };

  return (
    <React.Fragment>
    <React.Fragment>
      <h1>Settings</h1>
    <Pivot aria-label="Basic Pivot Example" style={{textAlign:'left'}}>
      <PivotItem headerText="Permissions">
      <PrimaryButton style={{marginTop: '20px', marginLeft: '15px'}} text={"New User"} onClick={() => {
                  OpenNewUserServiceDialog() }} /> 
        <Label style={{marginTop: '20px', marginLeft: '15px', fontSize: '20px'}}>Administrators</Label>
      <table className="noborder offer" style={{margin: 10}} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>             
              <th style={{width: 600}}>
                <FormLabel title={"Name"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Created Date"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingPermissions ?
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
              <PermissionsList permissions={permissionsList} role="Administrator" />
            }
          </tbody>
        </table>

        <Label style={{marginTop: '20px', marginLeft: '15px', fontSize: '20px'}}>Publishers</Label>
      <table className="noborder offer" style={{margin: 10}} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>             
              <th style={{width: 600}}>
                <FormLabel title={"Name"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Created Date"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingPermissions ?
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
              <PermissionsList permissions={permissionsList} role="Publisher"/>
            }
          </tbody>
        </table>
        <Label style={{marginTop: '20px', marginLeft: '15px', fontSize: '20px'}}>Reviewers</Label>
      <table className="noborder offer" style={{margin: 10}} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>             
              <th style={{width: 600}}>
                <FormLabel title={"Name"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Created Date"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingPermissions ?
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
              <PermissionsList permissions={permissionsList} role="Reviewer"/>
            }
          </tbody>
        </table>
      </PivotItem>
      <PivotItem headerText="Partner Service" >       
        <table className="noborder offer" style={{margin: 10}} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>
              <th colSpan={3}>              
              <PrimaryButton text={"New Partner Service"} onClick={() => {
                  OpenNewPartnerServiceDialog() }} />   
              </th>              
            </tr>
            <tr>             
              <th style={{width: 200}}>
                <FormLabel title={"Name"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Type"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Created Date"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Operations"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingPartnerService ?
              (
                <tr>
                  <td colSpan={5} align={"center"}>
                    <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                      <Loading />
                    </Stack>
                  </td>
                </tr>
              )
              :
              <PartnerServiceList partnerService={partnerServiceList} />
            }
          </tbody>
        </table>
      </PivotItem>
      <PivotItem headerText="Review Settings">
      <h3 style={{fontWeight:'normal'}}>Automation Webhooks</h3>
      <table className="noborder offer" style={{margin: 10}} cellPadding={5} cellSpacing={0}>     
          <thead>                       
            <tr>             
              <th style={{width: 200}}>
                <FormLabel title={"Name"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Status"} />
              </th>
              <th style={{width: 200}}>
                <FormLabel title={"Created Date"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingAutomationWebhook ?
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
              <AutomationWebhooksList automationWebhook={automationWebhookList} />
            }
          </tbody>
          <tfoot>
          <tr>   
          <td colSpan={3} style={{paddingTop: '1%'}}>
              <PrimaryButton text={"New Automation Webhook"} onClick={() => {
                  OpenNewAutomationWebhookDialog() }} />                 
          </td>          
            </tr>
          </tfoot>
        </table>
      </PivotItem>
    </Pivot>
    </React.Fragment>

      <Dialog
        hidden={!partnerServiceDialogVisible}
        onDismiss={ClosePartnerServiceDialog}
        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {}

          },
          type: DialogType.normal,
          title: 'New Partner Service'
        }}
        modalProps={{
          isBlocking: true,
          isDarkOverlay: true,
          styles: {
            main: {
              minWidth: '35% !important',

            }
          }
        }}
      >
        <Formik
          initialValues={partnerService}
          validationSchema={partnerServiceFormValidationSchema}
          enableReinitialize={true}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            // setFormError(null);
            // setSubmitting(true);
            // globalContext.showProcessing();

            // //TODO: PUT THIS BACK IN
            // var createWorkSpaceResult = await SettingService.createOrUpdateWorkSpace(values.aMLWorkSpace);
            // if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'aMLWorkSpace', createWorkSpaceResult)) {
            //   toast.error(formError);
            //   globalContext.hideProcessing();
            //   return;
            // }

            // setSubmitting(false);

            // await getWorkSpaceList();
            // await getGitRepoList();
            // globalContext.hideProcessing();
            // toast.success("Success!");
            // setisEdit(true);
            // setDisplayDeleteButton(true);

            // Hub.dispatch(
            //   'AMLWorkspaceCreated',
            //   {
            //     event: 'WorkspaceCreated',
            //     data: true,
            //     message: ''
            //   });

            ClosePartnerServiceDialog();
          }}
        >
          {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
            <table className="offer" style={{ width: '100%' }}>
              <tbody>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Type:"} toolTip={SettingsMessages.partnerService.Type} />                                         
                    </React.Fragment>
                  </td>
                  <td>
                  <Dropdown options={typeList} id={`partnerService.type`}
                            onBlur={handleBlur}
                            placeHolder="Choose a Type"
                            errorMessage={getPartnerServiceFormErrorString(touched, errors, 'type')}
                            onChange={(event, option, index) => {
                              selectOnChange(`partnerService.type`, setFieldValue, event, option, index);
                            }} 
                            defaultSelectedKey=""/>                   
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Resource Id:"} toolTip={SettingsMessages.partnerService.ResourceId} />                     
                    </React.Fragment>

                  </td>
                  <td>
                    <TextField
                          name={'partnerService.resourceId'}
                          value={values.partnerService.resourceId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getPartnerServiceFormErrorString(touched, errors, 'resourceId')}
                          placeholder={'Resource Id'}
                          className="txtFormField wdth_100_per" />
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Tenant Id:"} toolTip={SettingsMessages.partnerService.TenantId} />                      
                    </React.Fragment>
                  </td>
                  <td>
                    <TextField
                          name={'partnerService.tenantId'}
                          value={values.partnerService.tenantId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getPartnerServiceFormErrorString(touched, errors, 'tenantId')}
                          placeholder={'Tenant Id'}
                          className="txtFormField wdth_100_per" />
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Client Id:"} toolTip={SettingsMessages.partnerService.ClientId} />                     
                    </React.Fragment>
                  </td>
                  <td>
                    <TextField
                          name={'partnerService.clientId'}
                          value={values.partnerService.clientId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getPartnerServiceFormErrorString(touched, errors, 'clientId')}
                          placeholder={'Client Id'}
                          className="txtFormField wdth_100_per" />
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Client Secret:"} toolTip={SettingsMessages.partnerService.ClientSecret} />                      
                    </React.Fragment>
                  </td>
                  <td>
                    <TextField
                          name={'partnerService.clinetSecrets'}
                          value={values.partnerService.clinetSecrets}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          type={'password'}
                          errorMessage={getPartnerServiceFormErrorString(touched, errors, 'clinetSecrets')}
                          placeholder={'Client Secret'}
                          className="txtFormField wdth_100_per" />
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Name:"} toolTip={SettingsMessages.partnerService.partnerServiceName} />                      
                    </React.Fragment>
                  </td>
                  <td>
                    <TextField
                          name={'partnerService.partnerServiceName'}
                          value={values.partnerService.partnerServiceName}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getPartnerServiceFormErrorString(touched, errors, 'partnerServiceName')}
                          placeholder={'Partner Service Name'}
                          className="txtFormField wdth_100_per" 
                          disabled={isDisplayUpdateButton}/>                          
                  </td>
                </tr>
                <tr>
                  <td colSpan={2}>
                    <DialogFooter>
                      <Stack horizontal={true} gap={15} style={{ width: '100%' }}>                        
                        <div style={{ flexGrow: 1 }}></div>
                        <PrimaryButton type="submit" id="btnTestConnection" className="mar-right-2_Per"
                          text={"Test Connection"} onClick={submitForm} />
                        <PrimaryButton type="submit" id="btnsubmit" className="mar-right-2_Per"
                          text={isDisplayUpdateButton ? "Update" : "Create"} onClick={submitForm} />
                        <AlternateButton
                          onClick={ClosePartnerServiceDialog}
                          text="Cancel" className="mar-right-2_Per" />
                      </Stack>
                    </DialogFooter>
                  </td>
                </tr>
              </tbody>
            </table>
          )}
        </Formik>
      </Dialog>

      <Dialog
        hidden={!automationWebhookDialogVisible}
        onDismiss={CloseAutomationWebhookDialog}
        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {}

          },
          type: DialogType.normal,
          title: 'Add New Automation Webhook'
        }}
        modalProps={{
          isBlocking: true,
          isDarkOverlay: true,
          styles: {
            main: {
              minWidth: '35% !important',

            }
          }
        }}
      >
        <Formik
          initialValues={automationWebhook}
          validationSchema={automationWebhookFormValidationSchema}
          enableReinitialize={true}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            // setFormError(null);
            // setSubmitting(true);
            // globalContext.showProcessing();

            // //TODO: PUT THIS BACK IN
            // var createWorkSpaceResult = await SettingService.createOrUpdateWorkSpace(values.aMLWorkSpace);
            // if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'aMLWorkSpace', createWorkSpaceResult)) {
            //   toast.error(formError);
            //   globalContext.hideProcessing();
            //   return;
            // }

            // setSubmitting(false);

            // await getWorkSpaceList();
            // await getGitRepoList();
            // globalContext.hideProcessing();
            // toast.success("Success!");
            // setisEdit(true);
            // setDisplayDeleteButton(true);

            // Hub.dispatch(
            //   'AMLWorkspaceCreated',
            //   {
            //     event: 'WorkspaceCreated',
            //     data: true,
            //     message: ''
            //   });

            CloseAutomationWebhookDialog();
          }}
        >
          {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
            <table className="offer" style={{ width: '100%' }}>
              <tbody>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Name:"} toolTip={SettingsMessages.automation.Name} />                                         
                    </React.Fragment>
                  </td>
                  <td>
                  <TextField
                          name={'automationWebhook.name'}
                          value={values.automationWebhook.name}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getAutomationWebhookFormErrorString(touched, errors, 'name')}
                          placeholder={'Name'}
                          className="txtFormField wdth_100_per" 
                          disabled={isEdit}/>
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Webhook URL:"} toolTip={SettingsMessages.automation.WebhookURL} />                     
                    </React.Fragment>

                  </td>
                  <td>
                    <TextField
                          name={'automationWebhook.webhookURL'}
                          value={values.automationWebhook.webhookURL}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getAutomationWebhookFormErrorString(touched, errors, 'webhookURL')}
                          placeholder={'Webhook URL'}
                          className="txtFormField wdth_100_per" />
                  </td>
                </tr>   
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Enabled:"} toolTip={SettingsMessages.automation.Enabled} />                     
                    </React.Fragment>

                  </td>
                  <td>
                    <Checkbox
                          name={'automationWebhook.enabled'}
                          defaultChecked={values.automationWebhook.enabled}
                          onChange={handleChange}
                          onBlur={handleBlur}                          
                          placeholder={'Webhook URL'}
                          className="txtFormField wdth_100_per" />
                  </td>
                </tr>           
                <tr>
                  <td colSpan={2}>
                    <DialogFooter>
                      <Stack horizontal={true} gap={15} style={{ width: '100%' }}>                        
                        <div style={{ flexGrow: 1 }}></div>                        
                        <PrimaryButton type="submit" id="btnsubmit" className="mar-right-2_Per"
                          text={isDisplayUpdateButton ? "Update" : "Add"} onClick={submitForm} />
                        <AlternateButton
                          onClick={CloseAutomationWebhookDialog}
                          text="Cancel" className="mar-right-2_Per" />
                      </Stack>
                    </DialogFooter>
                  </td>
                </tr>
              </tbody>
            </table>
          )}
        </Formik>
      </Dialog>

      <Dialog
        hidden={!newUserDialogVisible}
        onDismiss={CloseNewUserServiceDialog}
        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {}
          },
          type: DialogType.normal,
          title: 'Add new user'
        }}
        modalProps={{
          isBlocking: true,
          isDarkOverlay: true,
          styles: {
            main: {
              minWidth: '35% !important',

            }
          }
        }}
      >
          <Formik
          initialValues={permissions}
          validationSchema={permissionsFormValidationSchema}
          enableReinitialize={true}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {
            
            setFormError(null);
            setSubmitting(true);

            globalContext.showProcessing();

            // var CreatePermissionResult = await SettingService.createPermissions(values.permissions);
            // if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'permission', CreatePermissionResult)) {
            //   toast.error(formError)
            //   globalContext.hideProcessing();
            //   return;
            // }

            permissionsList.push(
              {
                clientId:'',
                createdDate: new Date().getDate().toLocaleString(),                
                userId:values.permissions.userId,
                role:values.permissions.role
              });
            setPermissionsList(permissionsList);

            setSubmitting(false);
            globalContext.hideProcessing();
            toast.success("Success!");

            CloseNewUserServiceDialog();
          }}
          >
          {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
            <table className="offer" style={{ width: '100%' }}>
            <tbody>
              <tr>
                <td>
                  <React.Fragment>
                    <FormLabel title={"User id:"} />                                         
                  </React.Fragment>
                </td>
                <td>
                <TextField
                          name={'permissions.userId'}
                          value={values.permissions.userId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getPermissionsFormErrorString(touched, errors, 'userId')}
                          placeholder={'User Id'}
                          className="txtFormField wdth_100_per"/>
                  </td>
                </tr>
                <tr>
                <td>
                  <React.Fragment>
                    <FormLabel title={"Role:"} />                                         
                  </React.Fragment>
                </td>
                <td>
                <Dropdown   options={roleList} 
                            id={`permissions.role`}
                            onBlur={handleBlur}
                            onChange={(event, option, index) => {
                              selectOnChange(`permissions.role`, setFieldValue, event, option, index)
                            }}
                            placeHolder="Choose a Role"
                            errorMessage={getPermissionsFormErrorString(touched, errors, 'role')}
                            defaultSelectedKey=""/> 
                  </td>
                </tr>
                <tr>
                  <td colSpan={2}>
                    <DialogFooter>
                      <Stack horizontal={true} gap={15} style={{ width: '100%' }}>                        
                        <div style={{ flexGrow: 1 }}></div>
                        <PrimaryButton type="submit" id="btnsubmit" className="mar-right-2_Per"
                          text="Add" onClick={submitForm} />
                        <AlternateButton
                          onClick={CloseNewUserServiceDialog}
                          text="Cancel" className="mar-right-2_Per" />
                      </Stack>
                    </DialogFooter>
                  </td>
                </tr>
                </tbody>
                </table>
          )}
          </Formik>
      </Dialog>
            
    </React.Fragment>
  );
}

export default Settings;