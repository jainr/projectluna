from luna.dataset.luna_dataset import LunaDataset
from luna.dataset.exceptions import LunaDatasetNotSupportedException, LunaInvalidConfigException
import pandas as pd 
import requests
import tempfile
import os

class LunaTabularDataset(LunaDataset):

    def download(self):
        """
        Download function is not supported in LunaTabularDataset. Use LunaFileDataset instead.
        """
        raise LunaDatasetNotSupportedException("Download function is not supported in LunaTabularDataset. Use LunaFileDataset instead.")

    def upload(self, path):
        """
        Upload function is not supported in LunaTabularDataset. Use LunaFileDataset instead.
        """
        raise LunaDatasetNotSupportedException("Upload function is not supported in LunaTabularDataset. Use LunaFileDataset instead.")

    @staticmethod
    def create(config):
        """
        Create a LunaTabularDataset from config dict
        """
        if "url" in config:
            return LunaUrlTabularDataset(config["url"])
        elif "azureml_dataset_name" in config:
            return LunaAzureMLTabularDataset(config["azureml_dataset_name"])
        elif "sql_connection_string" in config:
            table_name = ""
            query = ""
            if "table_name" in config:
                table_name = config["table_name"]
            if "query" in config:
                query = config["query"]
            # Need to specify either table name or query
            if table_name == "" and query == "":
                raise LunaInvalidConfigException("Either table_name or query property needs to be provided for LunaSQLTabularDataset.", ["table_name", "query"])
            return LunaSQLTabularDataset(config["sql_connection_string"], table_name = table_name, query = query)
        else:
            raise LunaDatasetNotSupportedException("The config does not match any tabular dataset.")

class LunaAzureMLTabularDataset(LunaTabularDataset):
    """
    Read tabular data from Azure ML tabular dataset.
    Only supported when running in an Azure ML workspace.
    """

    _azureml_dataset_name = ""

    def to_pandas_dataframe(self):
        """
        Read tabular data from Azure ML tabular dataset and return as a Pandas dataframe
        """
        pass
    
    def write(self, df):
        """
        Write function is not supported in LunaAzureMLTabularDataset.
        """
        raise LunaDatasetNotSupportedException("Write function is not supported in LunaAzureMLTabularDataset.")
    
    def __init__(self, azureml_dataset_name):
        self._azureml_dataset_name = azureml_dataset_name

class LunaSQLTabularDataset(LunaTabularDataset):

    _connection_string = ""
    _table_name = ""
    _query = ""

    def to_pandas_dataframe(self):
        """
        Read tabular data from SQL Server and return as a Pandas dataframe
        """
        pass
    
    def write(self, df):
        """
        Write tabular data to SQL Server
        """
        pass

    def __init__(self, connection_string, table_name='', query=''):
        self._connection_string = connection_string
        self._table_name = table_name
        self._query = query


class LunaUrlTabularDataset(LunaTabularDataset):

    _url = ""

    def to_pandas_dataframe(self, delimiter=",", header="infer", names=None, skiprows=None, nrows=None, index_col=None, low_memory=True):
        """
        Read tabular data from an csv file url and return as a Pandas dataframe
        """

        return pd.read_csv(self._url, delimiter=delimiter, header=header, names=names, skiprows=skiprows, nrows=nrows, index_col=index_col, low_memory=low_memory)
    
    def write(self, df, header=False):
        """
        Write tabular data to an csv file url
        Right now it only works for a public Azure block blob url or private blob url with sas key
        """
        with tempfile.TemporaryDirectory() as tmp:
            temp_filename = os.path.join(tmp, "tmp_result.csv")
            with open(temp_filename, "wt") as temp_file:
                df.to_csv(temp_file, header=header)

            with open(temp_filename , 'rb') as fh:
                requests.put(self._url,
                                data=fh,
                                headers={
                                            'content-type': 'text/csv',
                                            'x-ms-blob-type': 'BlockBlob'
                                        }
                                )

        return

    def __init__(self, url):
        self._url = url
