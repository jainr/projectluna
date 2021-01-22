import React, { useEffect, useState } from 'react';
import {
  Dialog,
  DialogFooter,
  DialogType,
  Dropdown,
  FontIcon,
  IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField
} from 'office-ui-fabric-react';
import FormLabel from "../../shared/components/FormLabel";
import { useHistory } from "react-router";
import { WebRoute } from "../../shared/constants/routes";
import { IError, IProductModel } from '../../models';
import { Loading } from "../../shared/components/Loading";
import AlternateButton from "../../shared/components/AlternateButton";
import { initialInfoFormValues, IProductInfoFormValues, productInfoValidationSchema } from "./formUtils/ProductFormUtils";
import { Formik, useFormikContext } from "formik";
import { useGlobalContext } from "../../shared/components/GlobalProvider";
import { toast } from "react-toastify";
import { handleSubmissionErrorsForForm } from "../../shared/formUtils/utils";
import ProductService from '../../services/ProductService';
import { ProductMessages } from '../../shared/constants/infomessages';
import {CopyToClipboard} from 'react-copy-to-clipboard';
import ReactHtmlParser from 'react-html-parser';

export type IProductFormFormProps = {
  isNew: boolean;
  formError?: string | null;
  products: IProductModel[];
}

export const ProductForm: React.FunctionComponent<IProductFormFormProps> = (props) => {
  const { values, handleChange, handleBlur, touched, errors, handleSubmit, submitForm, dirty, setFieldValue } = useFormikContext<IProductInfoFormValues>(); // formikProps
  const { formError, isNew} = props;

  const globalContext = useGlobalContext();

  useEffect(() => {
    globalContext.modifySaveForm(async () => {
      await submitForm();
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getProductFormErrorString = (touched, errors, property: string, dirty) => {
    setTimeout(() => { globalContext.setFormDirty(dirty); }, 500);

    return touched.product && errors.product && touched.product[property] && errors.product[property] ? errors.product[property] : '';
  };

  const DisplayErrors = (errors) => {
    return null;
  };
  const getidlist = (): string => {
    let idlist = ''
    props.products.map((values, index) => {
      idlist += values.aiServiceName + ',';
      return idlist;
    })
    values.product.Idlist = idlist.substr(0, idlist.length - 1);
    return idlist.substr(0, idlist.length - 1);
  }

  const selectOnChange = (fieldKey: string, event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number) => {
    if (option) {
      setFieldValue(fieldKey, option.key, false);
    }
  };

  const textboxClassName = (props.isNew ? "form_textboxmodal" : "form_textbox");

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
        {isNew &&
          <React.Fragment>
                <tr>
                  <td>
                    <Stack className={"form_row"}>
                      <FormLabel title={"Name:"} toolTip={ProductMessages.product.ProductId} />
                      <input type="hidden" name={'product.Idlist'} value={getidlist()} />
                      <TextField
                        name={'product.aiServiceName'}
                        value={values.product.aiServiceName}
                        maxLength={50}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'aiserviceName', dirty)}
                        placeholder={'name'}
                        className={textboxClassName} />
                    </Stack>
                  </td>
                  <td>
                    <Stack className={"form_row"}>
                      <FormLabel title={"Owner:"} toolTip={ProductMessages.product.Owner} />
                      <TextField
                        name={'product.owner'}
                        value={values.product.owner}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'owner', dirty)}
                        placeholder={'Owner'}
                        className={textboxClassName} />
                    </Stack>
                  </td>
                </tr>
          </React.Fragment>
        }

            <tr>
              <td colSpan={2}>
              <Stack className={"form_row"}>
                      <FormLabel title={"Display Name (64 characters max):"} toolTip={ProductMessages.product.DisplayName} />
                      <TextField
                        name={'product.displayName'}
                        value={values.product.displayName}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'displayName', dirty)}
                        placeholder={'Display Name'}
                        style={ {width: "100%"} } />
                    </Stack>
              </td>
            </tr>
            <tr>
              <td colSpan={2}>
              <Stack className={"form_row"}>
                      <FormLabel title={"Description (120 characters max):"} toolTip={ProductMessages.product.Description} />
                      <TextField
                        name={'product.description'}
                        value={values.product.description}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'description', dirty)}
                        placeholder={'description'}
                        style={ {width: "100%"} } />
                    </Stack>
              </td>
            </tr>
            <tr>
              <td colSpan={2}>
              <Stack className={"form_row"}> 
                      <FormLabel title={"Logo image Url (90px x 90px):"} toolTip={ProductMessages.product.LogoImageUrl} />
                      <TextField
                        name={'product.logoImageUrl'}
                        style={ {width: "100%"} }
                        value={values.product.logoImageUrl}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'logoImageUrl', dirty)}
                        placeholder={'logo image url'} />
                    </Stack>
              </td>
            </tr>
            <tr>
              <td colSpan={2}>
              <Stack className={"form_row"}> 
                      <FormLabel title={"Documentation Url:"} toolTip={ProductMessages.product.DocumentationUrl} />
                      <TextField
                        name={'product.documentationUrl'}
                        style={ {width: "100%"} }
                        value={values.product.documentationUrl}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        errorMessage={getProductFormErrorString(touched, errors, 'documentationUrl', dirty)}
                        placeholder={'documentation url'} />
                    </Stack>
              </td>
            </tr>
          </tbody>
        </table>
      </Stack>
    </form>
  );
}

const Products: React.FunctionComponent = () => {
  const history = useHistory();
  const globalContext = useGlobalContext();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formState, setFormState] = useState<IProductInfoFormValues>(initialInfoFormValues);
  const [products, setProducts] = useState<IProductModel[]>([]);
  const [loadingProducts, setLoadingProducts] = useState<boolean>(true);
  const [productDialogVisible, setProductDialogVisible] = useState<boolean>(false);
  const [productTypeDropdownOptions, setProductTypeDropdownOptions] = useState<IDropdownOption[]>([]);
  const [hostTypeDropdownOptions, setHostTypeDropdownOptions] = useState<IDropdownOption[]>([]);
  const [LunaWebhookUrlv2DialogVisible, setLunaWebhookUrlv2DialogVisible] = useState<boolean>(false);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [formError, setFormError] = useState<string | null>(null);

  const getProducts = async () => {

    globalContext.showProcessing();
    setLoadingProducts(true);
    const [
      productResponse
    ] = await Promise.all([
      await ProductService.list()
    ]);
    setLoadingProducts(false);
    globalContext.hideProcessing();

    if (productResponse.success) {

      if (productResponse.value)
        setProducts(productResponse.value);
      else
        setProducts([]);

      let productTypeOptions: IDropdownOption[] = [];
      productTypeOptions.push({ key: '', text: 'Select...' });

    } else {
      let errorMessages: IError[] = [];

      errorMessages.concat(productResponse.errors);

      if (errorMessages.length > 0) {
        toast.error(errorMessages.join(', '));
      }
    }
  }

  const editItem = (productName: string): void => {
    history.push(WebRoute.ProductDetail.replace(':aiServiceName', productName));
  };

  const Products = ({ products }) => {
    if (!products || products.length === 0) {
      return <tr>
        <td colSpan={4}><span>No AI Services</span></td>
      </tr>;
    } else {
      return (
        products.map((value: IProductModel, idx) => {
          return (
            <tr key={idx}>
              <td>
                <span style={{ width: 200 }}>{value.aiServiceName}</span>
              </td>
              <td>
                <span style={{ width: 200 }}>{value.displayName}</span>
              </td>
              <td>
                <span style={{ width: 100 }}>{value.owner}</span>
              </td>
              <td>
                <span style={{ width: 300 }}>{value.description}</span>
              </td>
              <td>
                <Stack
                  verticalAlign="center"
                  horizontalAlign={"space-evenly"}
                  gap={15}
                  horizontal={true}
                  styles={{
                    root: {
                      width: '40%'
                    },
                  }}
                >
                  <FontIcon iconName="Edit" className="deleteicon" onClick={() => { editItem(value.aiServiceName) }} />
                </Stack>
              </td>
            </tr>
          );
        })
      );
    }
  }

  useEffect(() => {
    getProducts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const hideNewProductDialog = (): void => {
    setProductDialogVisible(false);
  };

  const handleFormSubmission = async (e) => {
    if (globalContext.saveForm)
      await globalContext.saveForm();
  };

  // const handleSubmissionErrors = (result: Result<any>, setSubmitting: any): boolean => {
  //   if (result.hasErrors) {
  //     // TODO - display the errors here
  //     alert(result.errors.join(', '));
  //     setSubmitting(false);
  //     return true;
  //   }
  //   return false;
  // }

  // const getFormErrorString = (touched, errors, property: string) => {
  //   return touched && errors && touched[property] && errors[property] ? errors[property] : '';
  // };

  const showNewProductDialog = (): void => {
    setProductDialogVisible(true);
  };

  const handleNewProduct = (): void => {
    showNewProductDialog();
  };

  const hideLunaWebhookUrlv2Dialog = (): void => {
    setLunaWebhookUrlv2DialogVisible(false);
  };

  const showLunaWebhookUrlv2Dialog = (): void => {
    setLunaWebhookUrlv2DialogVisible(true);
  };

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
          <PrimaryButton text={"New AI Service"} onClick={handleNewProduct} />
          <PrimaryButton text={"Usage Reports"} style={{ left: '10%', bottom: '50%' }}  onClick={showLunaWebhookUrlv2Dialog} />
        </Stack>
        <table className="noborder offergrid" style={{ marginTop: 20, width: '100%' }} cellPadding={5} cellSpacing={0}>
          <thead>
            <tr style={{ fontWeight: 'normal' }}>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Name"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Display Name"} /></th>
              <th style={{ width: 100, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Owner"} /></th>
              <th style={{ width: 300, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Description"} /></th>
              <th style={{ width: 200, borderBottom: '1px solid #e8e8e8' }}><FormLabel title={"Operations"} /></th>
            </tr>
          </thead>
          <tbody>
            {loadingProducts ?
              (
                <tr>
                  <td colSpan={4} align={"center"}>
                    <Stack verticalAlign={"center"} horizontalAlign={"center"} horizontal={true}>
                      <Loading />
                    </Stack>
                  </td>
                </tr>
              )
              : <Products products={products} />}
          </tbody>
        </table>
      </Stack>
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
          title: 'New AI Service'
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
          validationSchema={productInfoValidationSchema}
          validateOnBlur={true}
          onSubmit={async (values, { setSubmitting, setErrors }) => {

            setFormError(null);
            setSubmitting(true);

            globalContext.showProcessing();
            var CreateProductResult = await ProductService.create(values.product);
            if (handleSubmissionErrorsForForm(setErrors, setSubmitting, setFormError, 'product', CreateProductResult)) {
              toast.error(formError)
              globalContext.hideProcessing();
              return;
            }

            setSubmitting(false);
            globalContext.hideProcessing();
            toast.success("Success!");
            if (CreateProductResult.value != null)
              history.push(WebRoute.ProductDetail.replace(':aiServiceName', CreateProductResult.value.aiServiceName));
          }}
        >
          <ProductForm isNew={true} products={products}/>
        </Formik>
        <DialogFooter>
          <AlternateButton
            onClick={hideNewProductDialog}
            text="Cancel" />
          <PrimaryButton
            onClick={handleFormSubmission}
            text="Save" />
        </DialogFooter>
      </Dialog>
      <Dialog
        hidden={!LunaWebhookUrlv2DialogVisible}
        onDismiss={hideLunaWebhookUrlv2Dialog}

        dialogContentProps={{
          styles: {
            subText: {
              paddingTop: 0
            },
            title: {}

          },
          type: DialogType.close,
          title: 'Usage Reports',
          subText:''
        }}
        modalProps={{
          isDarkOverlay: true,
          isBlocking: true,
          styles: {
            main: {
              minWidth: '60% !important'
            }
          }
        }}
      >
        <React.Fragment>
          <div id="subscriptionv2">
            
      <iframe width="1140" height="541.25" src="https://msit.powerbi.com/reportEmbed?reportId=1150275e-5d29-4a62-b880-2a88a06deb3c&autoAuth=true&ctid=72f988bf-86f1-41af-91ab-2d7cd011db47&config=eyJjbHVzdGVyVXJsIjoiaHR0cHM6Ly9kZi1tc2l0LXNjdXMtcmVkaXJlY3QuYW5hbHlzaXMud2luZG93cy5uZXQvIn0%3D" frameBorder="0">

</iframe>
          </div>
        </React.Fragment>
      </Dialog>
    </Stack>
  );
}

export default Products;