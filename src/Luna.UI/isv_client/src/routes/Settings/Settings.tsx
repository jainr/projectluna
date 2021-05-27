import React, { useEffect, useState } from 'react';
import {
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
import { IAMLWorkSpaceModel, IGitRepoModel, IPermissionsModel } from "../../models";
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
  permissionsFormValidationSchema,
  IPermissionsFormValues,
  initialPermissionsFormValues,
  initialPermissionsValues
} from '../Products/formUtils/PermissionsUtils'
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
  let [workSpace, setWorkSpace] = useState<IAMLWorkSpaceFormValues>(initialAMLWorkSpaceFormValues);
  let [permissionsList, setPermissionsList] = useState<IPermissionsModel[]>();

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

  const [partnerServiceDialogVisible, setPartnerServiceDialogVisible] = useState<boolean>(false);
  const [newUserDialogVisible, setNewUserDialogVisible] = useState<boolean>(false);
  const [loadingPermissions, setLoadingPermissions] = useState<boolean>(false);
  const [roleList, setRoleList] = useState<IDropdownOption[]>([]);
  let [permissions, setPermissions] = useState<IPermissionsFormValues>(initialPermissionsFormValues);

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

  const getPermissionsList = async () => {
    setLoadingPermissions(true);
    const results = await ProductService.getPermissions();
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
    return touched.permissions && errors.permissions && touched.permissions[property] && errors.permissions[property] ? errors.permissions[property] : '';
  };

  const getFormErrorString = (touched, errors, property: string) => {
    return touched.aMLWorkSpace && errors.aMLWorkSpace && touched.aMLWorkSpace[property] && errors.aMLWorkSpace[property] ? errors.aMLWorkSpace[property] : '';
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

  const OpenNewUserServiceDialog = () => {
    setNewUserDialogVisible(true);
  }

  const CloseWorkSpaceDialog = () => {
    setWorkSpaceDialogVisible(false);
  }

  const ClosePartnerServiceDialog = () => {
    setPartnerServiceDialogVisible(false);
  }

  const CloseNewUserServiceDialog = () => {
    setNewUserDialogVisible(false);
  }

  useEffect(() => {

    getWorkSpaceList();
    getGitRepoList();
    getPermissionsList();
    getRoles();

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
              <FontIcon iconName="Cancel" className="deleteicon" onClick={() => {}} />
              </Stack>
            </td>
          </tr>
          )
        })
      )
    }
  }

  const CloseAMLDeleteDialog = () => {
    setAMLDeleteDialog(false);
  }

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
                  OpenPartnerServiceDialog() }} />   
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
        </table>
      </PivotItem>
      <PivotItem headerText="Review Settings">
        <Label>Pivot #3</Label>
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
          initialValues={workSpace}
          validationSchema={aMLWorkSpaceFormValidationSchema}
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

            CloseWorkSpaceDialog();
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
                    <TextField
                          name={'aMLWorkSpace.workspaceName'}
                          value={values.aMLWorkSpace.workspaceName}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getFormErrorString(touched, errors, 'workspaceName')}
                          placeholder={'Workspace Name'}
                          className="txtFormField wdth_100_per" disabled={isEdit} max={50} />
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
                          name={'aMLWorkSpace.resourceId'}
                          value={values.aMLWorkSpace.resourceId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getFormErrorString(touched, errors, 'resourceId')}
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
                          name={'aMLWorkSpace.aadTenantId'}
                          value={values.aMLWorkSpace.aadTenantId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getFormErrorString(touched, errors, 'aadTenantId')}
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
                          name={'aMLWorkSpace.aadApplicationId'}
                          value={values.aMLWorkSpace.aadApplicationId}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          errorMessage={getFormErrorString(touched, errors, 'aadApplicationId')}
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
                          name={'aMLWorkSpace.aadApplicationSecrets'}
                          value={values.aMLWorkSpace.aadApplicationSecrets}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          type={'password'}
                          errorMessage={getFormErrorString(touched, errors, 'aadApplicationSecrets')}
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
                          name={'aMLWorkSpace.aadApplicationSecrets'}
                          value={values.aMLWorkSpace.aadApplicationSecrets}
                          onChange={handleChange}
                          onBlur={handleBlur}
                          type={'password'}
                          errorMessage={getFormErrorString(touched, errors, 'aadApplicationSecrets')}
                          placeholder={'NAme'}
                          className="txtFormField wdth_100_per" />
                  </td>
                </tr>
                <tr>
                  <td colSpan={2}>
                    <DialogFooter>
                      <Stack horizontal={true} gap={15} style={{ width: '100%' }}>                        
                        <div style={{ flexGrow: 1 }}></div>
                        <AlternateButton
                          onClick={ClosePartnerServiceDialog}
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
                          errorMessage={getFormErrorString(touched, errors, 'userId')}
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
                            id={`Role`}
                            onBlur={handleBlur}
                            placeHolder="Choose a Role"
                            errorMessage={getPermissionsFormErrorString(touched, errors, 'type')}
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