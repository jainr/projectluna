from luna.dataset.luna_dataset import LunaDataset
from luna.dataset.exceptions import LunaDatasetNotSupportedException, LunaInvalidConfigException

class LunaFileDataset(LunaDataset):

    def to_pandas_dataframe(self):
        """
        to_pandas_dataframe function is not supported in LunaFileDataset. Use LunaTabularDataset instead.
        """
        raise LunaDatasetNotSupportedException("to_pandas_dataframe function is not supported in LunaTabularDataset. Use LunaFileDataset instead.")

    def write(self, df):
        """
        Write function is not supported in LunaFileDataset. Use LunaTabularDataset instead.
        """
        raise LunaDatasetNotSupportedException("Write function is not supported in LunaTabularDataset. Use LunaFileDataset instead.")

    @staticmethod
    def create(config):
        """
        Create a LunaFileDataset from config dict
        """
        if "url" in config:
            return LunaUrlFileDataset(config["url"])
        elif "azureml_dataset_name" in config:
            return LunaAzureMLFileDataset(config["azureml_dataset_name"])
        elif "storage_account_name" in config and "path" in config and "storage_container_name" in config:
            sas_key = ""
            storage_account_key = ""
            if "storage_account_key" in config:
                storage_account_key = config["storage_account_key"]
            if "sas_key" in config:
                sas_key = config["sas_key"]
            # Need to specify either table name or query
            if sas_key == "" and storage_account_key == "":
                raise LunaInvalidConfigException("Either storage_account_key or sas_key property needs to be provided for LunaAzureStorageFileDataset.", ["storage_account_key", "sas_key"])
            return LunaAzureStorageFileDataset(config["storage_account_name"], config["storage_container_name"], config["path"], storage_account_key = storage_account_key, sas_key = sas_key)
        else:
            raise LunaDatasetNotSupportedException("The config does not match any tabular dataset.")

class LunaAzureMLFileDataset(LunaFileDataset):
    """
    Download file(s) from Azure ML file dataset.
    Only supported when running in an Azure ML workspace.
    """

    _azureml_dataset_name = ""

    def download(self, path):
        """
        Download file(s) from Azure ML file dataset
        """
        pass

    def upload(self, path):
        """
        Upload function is not supported in LunaAzureMLFileDataset 
        """
        raise LunaDatasetNotSupportedException("Upload function is not supported in LunaAzureMLFileDataset")

    def __init__(self, azureml_dataset_name):
        self._azureml_dataset_name = azureml_dataset_name

class LunaUrlFileDataset(LunaFileDataset):

    _url = ""
    def download(self, path):
        """
        Download a file from specifed url
        """
        pass

    def upload(self, path):
        """
        Upload a file to the specified url
        """
        pass

    def __init__(self, url):
        self._url = url

class LunaAzureStorageFileDataset(LunaFileDataset):

    _storage_account_name = ""
    _storage_account_key = ""
    _storage_container_name = ""
    _sas_key = ""
    _path = ""

    def download(self, path):
        """
        Download file(s) from an Azure storage account
        """
        pass

    def upload(self, path):
        """
        Upload file(s) to Azure storage account
        """
        pass

    def __init__(self, storage_account_name, storage_container_name, path, storage_account_key, sas_key):
        self._storage_account_name = storage_account_name
        self._storage_container_name = storage_container_name
        self._storage_account_key = storage_account_key
        self._sas_key = sas_key
        self._path = path
