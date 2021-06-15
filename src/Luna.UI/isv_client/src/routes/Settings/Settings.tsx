import React, { useEffect, useState } from 'react';
import {
  Checkbox,
  DefaultButton,
  Dialog,
  DialogFooter,
  DialogType,
  Dropdown,
  FontIcon,
  IDropdownOption,
  Label,
  Pivot,
  PivotItem,
  PrimaryButton,
  Stack,
  TextField
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { Formik } from "formik";
import { IAMLWorkSpaceModel, IAutomationWebhookModel, IGitRepoModel, IPartnerServiceModel } from "../../models";
import { Loading } from "../../shared/components/Loading";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import AlternateButton from '../../shared/components/AlternateButton';
import {
  aMLWorkSpaceFormValidationSchema,
  IAMLWorkSpaceFormValues,
  initialAMLWorkSpaceFormValues,
  initialAMLWorkSpaceValues,
  deleteAMLWorkSpaceValidator,
} from '../Products/formUtils/AMLWorkSpaceUtils';
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
import { Hub } from "aws-amplify";
import ProductService from "../../services/ProductService";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import { ProductMessages } from '../../shared/constants/infomessages';
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
      <AMLWorkSpaceList />
    </Stack>
  );
};

export type IAMLWorkSpaceListProps = {}
export const AMLWorkSpaceList: React.FunctionComponent<IAMLWorkSpaceListProps> = (props) => {
  //const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty } = useFormikContext<IAMLWorkSpaceListProps>(); // formikProps
  //const { } = props;
  let [workSpaceList, setWorkSpaceList] = useState<IAMLWorkSpaceModel[]>();
  let [gitRepoList, setGitRepoList] = useState<IGitRepoModel[]>();
  let [partnerServiceList, setPartnerServiceList] = useState<IPartnerServiceModel[]>();
  let [automationWebhookList, setAutomationWebhookList] = useState<IAutomationWebhookModel[]>();
  let [workSpace, setWorkSpace] = useState<IAMLWorkSpaceFormValues>(initialAMLWorkSpaceFormValues);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  let [workSpaceDeleteIndex, setworkSpaceDeleteIndex] = useState<number>(0);
  const [loadingWorkSpace, setLoadingWorkSpace] = useState<boolean>(false);
  const [loadingGitRepo, setLoadingGitRepo] = useState<boolean>(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [workSpaceDialogVisible, setWorkSpaceDialogVisible] = useState<boolean>(false);
  const [isDisplayDeleteButton, setDisplayDeleteButton] = useState<boolean>(true);
  const [isEdit, setisEdit] = useState<boolean>(true);

  const [AMLDeleteDialog, setAMLDeleteDialog] = useState<boolean>(false);
  const [selectedAML, setSelectedAML] = useState<IAMLWorkSpaceModel>(initialAMLWorkSpaceValues);

  const [automationWebhookDialogVisible, setAutomationWebhookDialogVisible] = useState<boolean>(false);
  const [loadingAutomationWebhook, setLoadingAutomationWebhook] = useState<boolean>(false);
  const [typeList, setTypeList] = useState<IDropdownOption[]>([]);
  const [isDisplayUpdateButton, setDisplayUpdateButton] = useState<boolean>(false);
  let [partnerService, setPartnerService] = useState<IPartnerServiceFormValues>(initialPartnerServiceFormValues);
  let [automationWebhook, setAutomationWebhook] = useState<IAutomationWebhookFormValues>(initialAutomationWebhookFormValues);

  const [partnerServiceDialogVisible, setPartnerServiceDialogVisible] = useState<boolean>(false);
  const [loadingPartnerService, setLoadingPartnerService] = useState<boolean>(false);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const globalContext = useGlobalContext();

  const getWorkSpaceList = async () => {

    setLoadingWorkSpace(true);
    const results = await ProductService.getAmlWorkSpaceList();
    if (results && results.value && results.success) {
      setWorkSpaceList(results.value);
      //   if (results.value.length > 4)
      //     body.style.height = 'auto';
      // }
      //setworkSpaceList(initialAMLWorkSpaceList);
      setLoadingWorkSpace(false);
    } else
      toast.error('Failed to load AML Workspaces');
  }
  
  const getGitRepoList = async () => {

    setLoadingGitRepo(true);
    const results = await ProductService.getGitRepoList();
    if (results && results.value && results.success) {
      setGitRepoList(results.value);
      setLoadingGitRepo(false);
    } else
      toast.error('Failed to load Git repos');
  }

  const getPartnerServiceList = async () => {

    setLoadingPartnerService(true);
    const results = await ProductService.getPartnerServicesList();
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
    const results = await ProductService.getAutomationWebhooksList();
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

  const getFormErrorString = (touched, errors, property: string) => {
    return touched.aMLWorkSpace && errors.aMLWorkSpace && touched.aMLWorkSpace[property] && errors.aMLWorkSpace[property] ? errors.aMLWorkSpace[property] : '';
  };

  const getPartnerServiceFormErrorString = (touched, errors, property: string) => {
    return touched.partnerService && errors.partnerService && touched.partnerService[property] && errors.partnerService[property] ? errors.partnerService[property] : '';
  };

  const getAutomationWebhookFormErrorString = (touched, errors, property: string) => {
    return touched.automationWebhook && errors.automationWebhook && touched.automationWebhook[property] && errors.automationWebhook[property] ? errors.automationWebhook[property] : '';
  };

  const getDeleteAMLErrorString = (touched, errors, property: string) => {
    return (touched.selectedWorkspaceName && errors.selectedWorkspaceName && touched[property] && errors[property]) ? errors[property] : '';
  };

  const editWorkSpace = async (workspaceName: string, idx: number) => {

    //let editedWorkspace = initialAMLWorkSpaceList.filter(a => a.workspaceName === Id)[0];
    let editedWorkspace = await ProductService.getAmlWorkSpaceByName(workspaceName);
    if (editedWorkspace && editedWorkspace.value && editedWorkspace.success) {
      setWorkSpace({ aMLWorkSpace: editedWorkspace.value });
      setworkSpaceDeleteIndex(idx);
    } else
      toast.error('Failed to load AML Workspaces');

    setisEdit(true);
    setDisplayDeleteButton(true);
    OpenWorkSpaceDialog();
    //history.push(WebRoute.ModifyProductInfo.replace(':productName', productName));
  };

  const editPartnerService = async (partnerServiceName: string, idx: number) => {

    //let editedWorkspace = initialAMLWorkSpaceList.filter(a => a.workspaceName === Id)[0];
    let editPartnerService = await ProductService.getPartnerServiceByName(partnerServiceName);
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
    let editAutomationWebhook = await ProductService.getAutomationWebhookByName(automationWebhookName);
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

  const deleteWorkSpace = async (aMLWorkSpaceModelSelected: IAMLWorkSpaceModel) => {

    setSelectedAML(aMLWorkSpaceModelSelected);
    setAMLDeleteDialog(true);

  };

  const OpenNewWorkSpaceDialog = () => {

    setWorkSpace(initialAMLWorkSpaceFormValues);
    setisEdit(false);
    setDisplayDeleteButton(false);
    OpenWorkSpaceDialog();
  }

  const OpenWorkSpaceDialog = () => {
    setWorkSpaceDialogVisible(true);
  }

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

  const CloseWorkSpaceDialog = () => {
    setWorkSpaceDialogVisible(false);
  }

  const ClosePartnerServiceDialog = () => {
    setPartnerServiceDialogVisible(false);
  }

  const CloseAutomationWebhookDialog = () => {
    setAutomationWebhookDialogVisible(false);
  }

  useEffect(() => {

    getWorkSpaceList();
    getGitRepoList();
    getPartnerServiceList();
    getAutomationWebhookList();
    getTypes();

    Hub.listen('AMLWorkspaceNewDialog', (data) => {
      OpenNewWorkSpaceDialog();
    })

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const WorkSpaceList = ({ amlWorkSpace }) => {
    if (!amlWorkSpace || amlWorkSpace.length === 0) {
      return <tr>
        <td colSpan={4}><span>No AML Workspaces</span></td>
      </tr>;
    } else {
      return (
        amlWorkSpace.map((value: IAMLWorkSpaceModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span>{value.workspaceName}</span>
              </td>
              <td>
                <span>{value.resourceId}</span>
              </td>
              <td>
                <Stack
                  verticalAlign="center"
                  horizontalAlign={"space-evenly"}
                  gap={15}
                  horizontal={true}
                >
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => {
                    editWorkSpace(value.workspaceName, idx)
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

  const CloseAMLDeleteDialog = () => {
    setAMLDeleteDialog(false);
  }

  return (
    <React.Fragment>
    <React.Fragment>
    <Pivot aria-label="Basic Pivot Example" style={{textAlign:'left'}}>
      <PivotItem headerText="Permissions">
        <Label>Pivot #1</Label>
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
      <React.Fragment>
        <h3 style={{ textAlign: 'left', fontWeight: 'normal', marginTop: 0, marginBottom: 20, width: '100%' }}>AML
          Workspaces</h3>
        <table className="noborder offer" cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>
              <th style={{ width: 200 }}>
                <FormLabel title={"WorkSpace Name"} />
              </th>
              <th style={{ width: 300 }}>
                <FormLabel title={"Resource Id"} />
              </th>
              <th style={{ width: 100 }}>
                <FormLabel title={"Operations"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingWorkSpace ?
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
              <WorkSpaceList amlWorkSpace={workSpaceList} />
            }
          </tbody>
          <tfoot>
            <tr>
              <td colSpan={3} style={{ paddingTop: '1%' }}>
                <PrimaryButton text={"Register New WorkSpace"} onClick={() => {
                  OpenNewWorkSpaceDialog()
                }} />
              </td>
            </tr>
          </tfoot>
        </table>
      </React.Fragment>

      <React.Fragment>
        <h3 style={{ textAlign: 'left', fontWeight: 'normal', marginTop: 0, marginBottom: 20, width: '100%' }}>Git Repos</h3>
        <table className="noborder offer" cellPadding={5} cellSpacing={0}>
          <thead>
            <tr>
              <th style={{ width: 200 }}>
                <FormLabel title={"Git Repo Name"} />
              </th>
              <th style={{ width: 300 }}>
                <FormLabel title={"HTTP URL"} />
              </th>
              <th style={{ width: 100 }}>
                <FormLabel title={"Operations"} />
              </th>
            </tr>
          </thead>
          <tbody>
            {loadingGitRepo ?
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
              <GitRepoList gitRepo={gitRepoList} />
            }
          </tbody>
          <tfoot>
            <tr>
              <td colSpan={3} style={{ paddingTop: '1%' }}>
                <PrimaryButton text={"Register New Git repo"} onClick={() => {
                  //OpenNewWorkSpaceDialog()
                }} />
              </td>
            </tr>
          </tfoot>
        </table>
      </React.Fragment>

      <Dialog
        hidden={!workSpaceDialogVisible}
        onDismiss={CloseWorkSpaceDialog}
        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {}

          },
          type: DialogType.normal,
          title: 'Register AML WorkSpace'
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
          initialValues={workSpace}
          validationSchema={aMLWorkSpaceFormValidationSchema}
          enableReinitialize={true}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            setFormError(null);
            setSubmitting(true);
            globalContext.showProcessing();

            //TODO: PUT THIS BACK IN
            var createWorkSpaceResult = await ProductService.createOrUpdateWorkSpace(values.aMLWorkSpace);
            if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'aMLWorkSpace', createWorkSpaceResult)) {
              toast.error(formError);
              globalContext.hideProcessing();
              return;
            }

            setSubmitting(false);

            await getWorkSpaceList();
            await getGitRepoList();
            globalContext.hideProcessing();
            toast.success("Success!");
            setisEdit(true);
            setDisplayDeleteButton(true);

            Hub.dispatch(
              'AMLWorkspaceCreated',
              {
                event: 'WorkspaceCreated',
                data: true,
                message: ''
              });

            CloseWorkSpaceDialog();
          }}
        >
          {({ handleChange, values, handleBlur, touched, errors, handleSubmit, submitForm, setFieldValue }) => (
            <table className="offer" style={{ width: '100%' }}>
              <tbody>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Workspace Name:"} toolTip={ProductMessages.AMLWorkSpace.WorkspaceName} />
                      <TextField
                        name={'aMLWorkSpace.workspaceName'}
                        value={values.aMLWorkSpace.workspaceName}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'workspaceName')}
                        placeholder={'Workspace Name'}
                        className="txtFormField wdth_100_per" disabled={isEdit} max={50} />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Resource Id:"} toolTip={ProductMessages.AMLWorkSpace.ResourceId} />
                      <TextField
                        name={'aMLWorkSpace.resourceId'}
                        value={values.aMLWorkSpace.resourceId}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'resourceId')}
                        placeholder={'Resource Id'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>

                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Tenant Id:"} toolTip={ProductMessages.AMLWorkSpace.AADTenantId} />
                      <TextField
                        name={'aMLWorkSpace.aadTenantId'}
                        value={values.aMLWorkSpace.aadTenantId}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'aadTenantId')}
                        placeholder={'AAD Tenant Id'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"AAD Application Id:"} toolTip={ProductMessages.AMLWorkSpace.AADApplicationId} />
                      <TextField
                        name={'aMLWorkSpace.aadApplicationId'}
                        value={values.aMLWorkSpace.aadApplicationId}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getFormErrorString(touched, errors, 'aadApplicationId')}
                        placeholder={'AAD Application Id'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"AADApplication Secret:"} toolTip={ProductMessages.AMLWorkSpace.AADApplicationSecret} />
                      <TextField
                        name={'aMLWorkSpace.aadApplicationSecrets'}
                        value={values.aMLWorkSpace.aadApplicationSecrets}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        type={'password'}
                        errorMessage={getFormErrorString(touched, errors, 'aadApplicationSecrets')}
                        placeholder={'AAD Application Secret'}
                        className="txtFormField wdth_100_per" />
                    </React.Fragment>
                  </td>
                </tr>
                <tr>
                  <td colSpan={2}>
                    <DialogFooter>
                      <Stack horizontal={true} gap={15} style={{ width: '100%' }}>
                        {isDisplayDeleteButton &&
                          <DefaultButton type="button" id="btnsubmit" className="addbutton"
                            onClick={() => {
                              deleteWorkSpace(workSpace.aMLWorkSpace)
                            }}>
                            <FontIcon iconName="Cancel" className="deleteicon" /> Delete
                      </DefaultButton>
                        }
                        <div style={{ flexGrow: 1 }}></div>
                        <AlternateButton
                          onClick={CloseWorkSpaceDialog}
                          text="Cancel" className="mar-right-2_Per" />
                        <PrimaryButton type="submit" id="btnsubmit" className="mar-right-2_Per"
                          text={isDisplayDeleteButton ? "Update" : "Create"} onClick={submitForm} />
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
            // var createWorkSpaceResult = await ProductService.createOrUpdateWorkSpace(values.aMLWorkSpace);
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
                      <FormLabel title={"Type:"} toolTip={ProductMessages.AMLWorkSpace.WorkspaceName} />                                         
                    </React.Fragment>
                  </td>
                  <td>
                  <Dropdown options={typeList} id={`Type`}
                            onBlur={handleBlur}
                            placeHolder="Choose a Type"
                            errorMessage={getPartnerServiceFormErrorString(touched, errors, 'type')}
                            // onChange={(event, option, index) => {
                            //   selectOnChange(`Type`, setFieldValue, event, option, index);
                            // }} 
                            defaultSelectedKey=""/>                   
                  </td>
                </tr>
                <tr>
                  <td>
                    <React.Fragment>
                      <FormLabel title={"Resource Id:"} toolTip={ProductMessages.AMLWorkSpace.ResourceId} />                     
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
                      <FormLabel title={"Tenant Id:"} toolTip={ProductMessages.AMLWorkSpace.AADTenantId} />                      
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
                      <FormLabel title={"Client Id:"} toolTip={ProductMessages.AMLWorkSpace.AADApplicationId} />                     
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
                      <FormLabel title={"Client Secret:"} toolTip={ProductMessages.AMLWorkSpace.AADApplicationSecret} />                      
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
                      <FormLabel title={"Name:"} toolTip={ProductMessages.AMLWorkSpace.AADApplicationSecret} />                      
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
            // var createWorkSpaceResult = await ProductService.createOrUpdateWorkSpace(values.aMLWorkSpace);
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
                      <FormLabel title={"Name:"} toolTip={ProductMessages.AMLWorkSpace.WorkspaceName} />                                         
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
                      <FormLabel title={"Webhook URL:"} toolTip={ProductMessages.AMLWorkSpace.ResourceId} />                     
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
                      <FormLabel title={"Enabled:"} toolTip={ProductMessages.AMLWorkSpace.ResourceId} />                     
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

      <DialogBox keyindex='DeploymentVersionmodal' dialogVisible={AMLDeleteDialog}
        title="Delete AML Workspace" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseAMLDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btnAMLDelete') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={selectedAML}
              validationSchema={deleteAMLWorkSpaceValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                globalContext.showProcessing();
                var workspaceResult = await ProductService.deleteWorkSpace(selectedAML.workspaceName);

                if (handleSubmissionErrorsForForm((item) => {
                }, (item) => {
                }, setFormError, 'aMLWorkSpace', workspaceResult)) {
                  toast.error(formError);
                  globalContext.hideProcessing();
                  return;
                }

                await getWorkSpaceList();
                await getGitRepoList();
                globalContext.hideProcessing();
                toast.success("AML Workspace Deleted Successfully!");

                CloseAMLDeleteDialog();
                CloseWorkSpaceDialog();
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <input type="hidden" name={'aMLWorkSpace.workspaceName'} value={values.workspaceName} />
                  <table>
                    <tbody>
                      <tr>
                        <td colSpan={2}>
                          <span> Are you sure you want to delete the AML workspace?</span>
                        </td>
                      </tr>
                      <tr>
                        <td colSpan={2}>
                          {
                            <React.Fragment>
                              <span>Type the workspace name</span>
                              <br />
                              <TextField
                                name={'selectedWorkspaceName'}
                                value={values.selectedWorkspaceName}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={getDeleteAMLErrorString(touched, errors, 'selectedWorkspaceName')}
                                placeholder={'WorkSpace Name'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  <div style={{ display: 'none' }}>
                    <PrimaryButton type="submit" id="btnAMLDelete" text="Save" />
                  </div>
                </form>
              )}
            </Formik>
          </React.Fragment>
        } />
    </React.Fragment>
  );
}

export default Settings;