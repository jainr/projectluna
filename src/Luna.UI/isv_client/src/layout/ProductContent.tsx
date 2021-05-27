import React, { useEffect, useState } from 'react';
import { DefaultButton, Dialog, DialogFooter, DialogType, FontIcon, getTheme, MessageBar, MessageBarType, PrimaryButton, Stack, TextField, values } from 'office-ui-fabric-react';
import { useHistory, useLocation } from 'react-router';
//import { LaoutHelper, LayoutHelperMenuItem } from "./Layout";
import ProductService from "../services/ProductService";
import { initialProductValues, deleteProductValidator, IProductInfoFormValues } from '../routes/Products/formUtils/ProductFormUtils'
import { IProductDetailsModel, IProductModel } from "../models";
import { useGlobalContext } from "../shared/components/GlobalProvider";

import 'react-confirm-alert/src/react-confirm-alert.css';
import { handleSubmissionErrorsForForm } from "../shared/formUtils/utils";
import { toast } from "react-toastify";
import { DialogBox } from '../shared/components/Dialog';
import { Formik, useFormikContext } from 'formik';
import FormLabel from '../shared/components/FormLabel';
import AlternateButton from '../shared/components/AlternateButton';
import { initialDetailsFormValues, initialProductDetailsValues, IProductDetailsFormValues, productDetailsValidationSchema } from '../routes/Products/formUtils/ProductDetailsUtils';

type ProductProps = {
  productName: string | null;
};

const ProductContent: React.FunctionComponent<ProductProps> = (props) => {

  const { productName } = props;

  const history = useHistory();
  const location = useLocation();
  const globalContext = useGlobalContext();
  const [productModel, setProductModel] = useState<IProductModel>(initialProductValues);
  const [formError, setFormError] = useState<string | null>(null);

  const [ProductDeleteDialog, setProductDeleteDialog] = useState<boolean>(false);
  const [selectedProductName, setSelectedProductName] = useState<string>('');
  const [productDialogVisible, setProductDialogVisible] = useState<boolean>(false);
  const [productDetails, setProductDetails] = useState<IProductDetailsModel>();
  const [formState, setFormState] = useState<IProductDetailsFormValues>(initialDetailsFormValues);

  const theme = getTheme();

  const getProductInfo = async (productName: string) => {

    let response = await ProductService.get(productName);
    let details = await ProductService.getDetails(productName);

    //let response = initialProductList.filter(p=>p.productName==productName)[0];

    if (!response.hasErrors && response.value) {
      setProductModel({ ...response.value })
    }

    if (!details.hasErrors && details.value) {
      details.value.applicationName  = "abc";
      details.value.owner = "bb";
      setProductDetails({ ...details.value })
    }

  }

  useEffect(() => {
    if (productName)
      getProductInfo(productName);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [productName]);

  useEffect(() => {

    // setHideSave(location.pathname.toLowerCase().endsWith("/plans"));
    // setHideSave(location.pathname.toLowerCase().endsWith("/meters"));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [history.location, location.pathname]);

  const hideNewProductDialog = (): void => {
    setProductDialogVisible(false);
  };

  const handleProductDetails = (): void => {
    showNewProductDialog();
  };
  const showNewProductDialog = (): void => {
    setProductDialogVisible(true);
  };

  const handleFormSubmission = async (e) => {
    if (globalContext.saveForm)
      await globalContext.saveForm();
  };

  const handleProductDeletion = async (e) => {

    setSelectedProductName(productName as string);
    setProductDeleteDialog(true);

    // globalContext.showProcessing();

    // // determine if there are any deployments or aml workspaces, if there are, prevent the deletion
    // var deploymentsResponse = await ProductService.getDeploymentListByProductName(productName as string);

    // if (deploymentsResponse.success) {
    //   if (deploymentsResponse.value && deploymentsResponse.value.length > 0) {
    //     toast.error("You must delete all deployments for the product first.");
    //     globalContext.hideProcessing();
    //     return;
    //   }
    // }

    // const deleteResult = await ProductService.delete(productName as string);

    // if (handleSubmissionErrorsForForm((item) => {},(item) => {}, setFormError, 'product', deleteResult)) {
    //   toast.error(formError);
    //   globalContext.hideProcessing();
    //   return;
    // }

    // globalContext.hideProcessing();
    // toast.success("Product Deleted Successfully!");
    // history.push(`/products/`);
  };

  const OnCancel = async (e) => {
    history.push(`/products/`);
  };

  const CloseProductDeleteDialog = () => {
    setProductDeleteDialog(false);
  }

  const getDeleteProductErrorString = (touched, errors, property: string) => {
    return (touched.selectedProductId && errors.selectedProductId && touched[property] && errors[property]) ? errors[property] : '';
  };


  return (
    <Stack
      horizontal={true}
      horizontalAlign={"space-evenly"}
      styles={{
        root: {
          height: 'calc(100% - 57px)',
          backgroundColor: theme.palette.neutralLight
        }
      }}
    >
      <Stack
        horizontal={false}
        verticalAlign={"start"}
        verticalFill={true}
        styles={{
          root: {
            flexGrow: 1,
            maxWidth: 1234,
            backgroundColor: 'white'
          }
        }}
      >
        {/* Product Details Header */}
        <Stack
          horizontal={true}
          verticalAlign={"center"}
          verticalFill={true}
          className={"offer-details-header"}
          styles={{
            root: {
              height: 70,
              paddingLeft: 31,
              paddingRight: 31,
              width: '100%'
            }
          }}
        >
          <Stack.Item styles={{
            root: {
              flexGrow: 0
            }
          }}>
            <span style={{ fontWeight: 'bold', marginRight: 20, fontSize: 18 }}>
              AI Application Details
            </span>
            <span className={"offer-details-separator"}></span>
            <span style={{ fontWeight: 600 }}>
              Name:
            </span>
            <span style={{ marginLeft: 8 }}>
              {productModel.applicationName}
            </span>
            <span style={{ marginLeft: 100, fontWeight: 600 }}>
              Display Name:
            </span>
            <span style={{ marginLeft: 8 }}>
              {productModel.displayName}
            </span>
          </Stack.Item>
          <Stack.Item styles={{
            root: {
              flexGrow: 1
            }
          }}>
            <Stack
              horizontal={true}
              verticalAlign={"center"}
              verticalFill={true}
              horizontalAlign={"end"}
              gap={15}
            >
              <DefaultButton type="button" id="btnProperties" onClick={handleProductDetails}>Properties</DefaultButton>
              <DefaultButton onClick={handleProductDeletion} className="addbutton">
                <FontIcon iconName="Cancel" className="deleteicon" /> Delete
              </DefaultButton>
              <PrimaryButton text={"Go Back"} onClick={OnCancel} />

            </Stack>
          </Stack.Item>
        </Stack>
        {/* Offer navigation */}
        <Stack
          horizontal={true}
          verticalAlign={"center"}
          verticalFill={true}
          className={"nav-header Productnav-header"}
          styles={{
            root: {
              paddingLeft: 31,
              paddingRight: 31,
              height: 45,
              marginBottom: 20
            }
          }}
        >
          {/* {layoutHelper.menuItems.map((value: LayoutHelperMenuItem, idx) => {
            return (
              <Stack
                key={`menuItem_${idx}`}
                horizontal={true}
                verticalAlign={"center"}
                verticalFill={true}
                onClick={value.menuClick}
                styles={{
                  root: {
                    height: 40,
                    fontWeight: (isNavItemActive(value.paths) ? 600 : 'normal'),
                    borderBottom: (isNavItemActive(value.paths) ? 'solid 2px #0078d4' : 'none'),
                    marginTop: (isNavItemActive(value.paths) ? 2 : 0),
                    paddingLeft: 20,
                    paddingRight: 20,
                    minWidth: 94,
                    cursor: 'pointer'
                  }
                }}
              >
                <span style={{ textAlign: "center", whiteSpace: "nowrap", flexGrow: 1 }}>
                  {value.title}
                </span>
              </Stack>
            )
          })} */}
        </Stack>
        <div className="innercontainer">
          {props.children}
        </div>

      </Stack>

      <DialogBox keyindex='DeploymentVersionmodal' dialogVisible={ProductDeleteDialog}
        title="Delete AI Application" subText="" isDarkOverlay={true} className="" cancelButtonText="Cancel"
        submitButtonText="Submit" maxwidth={500}
        cancelonClick={() => {
          CloseProductDeleteDialog();
        }}
        submitonClick={() => {
          const btnsubmit = document.getElementById('btnProductDelete') as HTMLButtonElement;
          btnsubmit.click();
        }}
        children={
          <React.Fragment>
            <Formik
              initialValues={productModel}
              validationSchema={deleteProductValidator}
              enableReinitialize={true}
              validateOnBlur={true}
              onSubmit={async (values, { setSubmitting, setErrors }) => {

                globalContext.showProcessing();

                // determine if there are any deployments or aml workspaces, if there are, prevent the deletion
                var deploymentsResponse = await ProductService.getDeploymentListByProductName(productName as string);

                if (deploymentsResponse.success) {
                  if (deploymentsResponse.value && deploymentsResponse.value.length > 0) {
                    toast.error("You must delete all APIs in the AI Applications first.");
                    globalContext.hideProcessing();
                    return;
                  }
                }

                const deleteResult = await ProductService.delete(productName as string);

                if (handleSubmissionErrorsForForm((item) => {},(item) => {}, setFormError, 'product', deleteResult)) {
                  toast.error(formError);
                  globalContext.hideProcessing();
                  return;
                }

                globalContext.hideProcessing();
                toast.success("AI Application Deleted Successfully!");
                history.push(`/products/`);
              }}
            >
              {({ handleChange, values, handleBlur, touched, errors, handleSubmit }) => (
                <form autoComplete={"off"} onSubmit={handleSubmit}>
                  <input type="hidden" name={'aMLWorkSpace.workspaceName'} value={selectedProductName} />
                  <table>
                    <tbody>
                      <tr>
                        <td colSpan={2}>
                          <span> Are you sure you want to delete the AI Service?</span>
                        </td>
                      </tr>
                      <tr>
                        <td colSpan={2}>
                          {
                            <React.Fragment>
                              <span>Type the AI Application Id: {values.selectedProductId}</span>
                              <br />
                              <TextField
                                name={'selectedProductId'}
                                value={values.selectedProductId}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                errorMessage={getDeleteProductErrorString(touched, errors, 'selectedProductId')}
                                placeholder={'AI Application Id'}
                                className="txtFormField" />
                            </React.Fragment>
                          }
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  <div style={{ display: 'none' }}>
                    <PrimaryButton type="submit" id="btnProductDelete" text="Save" />
                  </div>
                </form>
              )}
            </Formik>
          </React.Fragment>
        } />
      <Dialog
        hidden={!productDialogVisible}
        onDismiss={hideNewProductDialog}

        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {
            }

          },
          type: DialogType.normal,
          title: 'Application-'
        }}
        modalProps={{
          isBlocking: true,
          styles: {

            main: {
              minWidth: '40% !important'
            }
          }
        }}
      >
        <Formik
          initialValues={formState}
          validationSchema={productDetailsValidationSchema}
          enableReinitialize={true}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {
            setFormError(null);
            setSubmitting(true);

            globalContext.showProcessing();
            // var CreateProductResult = await ProductService.create(values.product);
            // if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'product', CreateProductResult)) {
            //   toast.error(formError)
            //   globalContext.hideProcessing();
            //   return;
            // }

            setSubmitting(false);
            globalContext.hideProcessing();
            toast.success("Success!");
            // if (CreateProductResult.value != null)
            //   history.push(WebRoute.ProductDetail.replace(':applicationName', CreateProductResult.value.applicationName));
          }}
        >
          <ProductDetailsForm productDetails={productDetails} />
        </Formik> 
        <DialogFooter>
          <PrimaryButton
            onClick={handleFormSubmission}
            text="Update" />
          <AlternateButton
            onClick={hideNewProductDialog}
            text="Cancel" />
        </DialogFooter>
      </Dialog>
    </Stack>
  );
};

export type IProductDetailsFormFormProps = {
  formError?: string | null;
  productDetails?: IProductDetailsModel;
}

export const ProductDetailsForm: React.FunctionComponent<IProductDetailsFormFormProps> = (props) => {
  const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty, setFieldValue } = useFormikContext<IProductDetailsFormValues>(); // formikProps
  const { formError} = props;
  const globalContext = useGlobalContext();

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getProductFormErrorString = (touched, errors, property: string, dirty) => {
    setTimeout(() => { globalContext.setFormDirty(dirty); }, 500);

    return touched.productDetails && errors.productDetails && touched.productDetails[property] && errors.productDetails[property] ? errors.productDetails[property] : '';
  };

  const DisplayErrors = (errors) => {
    return null;
  };
  const getidlist = (): string => {
    let idlist = '123'
    return idlist;
  }
  return (
    <form style={{ width: '100%' }} autoComplete={"off"} onSubmit={handleSubmit}>
      {formError && <div style={{ marginBottom: 15 }}><MessageBar messageBarType={MessageBarType.error}>
        <div dangerouslySetInnerHTML={{ __html: formError }} style={{ textAlign: 'left' }}></div>
      </MessageBar></div>}
      <Stack
        verticalAlign="start"
        horizontal={false}
        gap={10}
        styles={{
          root: {}
        }}
      >
        
        <table>
              <tbody>
        <DisplayErrors errors={errors} />
          <React.Fragment>
                <tr>
                  <td>
                    <Stack className={"form_row"}>
                      <FormLabel title={"Owners:"} toolTip={"Owners"}/>
                      <input type="hidden" name={'productDetails.Idlist'} value={getidlist()} />
                      <TextField
                        name={'productDetails.owner'}
                        value={values.productDetails.owner}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'owner', dirty)}
                        placeholder={'Owners'}
                        />
                    </Stack>
                    </td>
                    <td>
                    <Stack className={"form_row"}>
                      <FormLabel title={"Logo Image Url:"} toolTip={"Logo Image Url"}/>
                      <TextField
                        name={'productDetails.logoImageUrl'}
                        value={values.productDetails.logoImageUrl}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'logoImageUrl', dirty)}
                        placeholder={'Logo Image Url'}
                        />
                    </Stack>
                  </td>
                </tr>
          </React.Fragment>

            <tr>
              <td>
              <Stack className={"form_row"}>
                      <FormLabel title={"Display Name:"} toolTip={"Display Name"}/>
                      <TextField
                        name={'productDetails.displayName'}
                        value={values.productDetails.displayName}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'displayName', dirty)}
                        placeholder={'Display Name'}
                        style={ {width: "100%"} } />
                    </Stack>
              </td>
              <td>
              <Stack className={"form_row"}>
                      <FormLabel title={"Documentation Url:"} toolTip={"Documentation Url"}/>
                      <TextField
                        name={'productDetails.documentationUrl'}
                        value={values.productDetails.documentationUrl}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'documentationUrl', dirty)}
                        placeholder={'Documentation Url'}
                        style={ {width: "100%"} } />
                    </Stack>
              </td>    
            </tr>        
            <tr>
                  <td>
                  <Stack className={"form_row"}>
                      <FormLabel title={"Description:"} toolTip={"Description"}/>
                      <TextField
                        name={'productDetails.description'}
                        value={values.productDetails.description}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'description', dirty)}
                        placeholder={'Description'}
                        style={ {width: "100%"} } />
                    </Stack>
                  </td>
                  <td>
                    <Stack className={"form_row"}>
                      <FormLabel title={"Tags:"} toolTip={"Tags"}/>
                      <TextField
                        name={'productDetails.tags'}
                        value={values.productDetails.tags}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'tags', dirty)}
                        placeholder={'tags'}
                        />
                    </Stack>
                  </td>
                </tr>
          </tbody>
        </table>
      </Stack>
    </form>
  );
}

export default ProductContent;