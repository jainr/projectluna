from abc import ABCMeta, abstractmethod

class LunaDataset(object):
    __metaclass__ = ABCMeta

    @abstractmethod
    def to_pandas_dataframe(self):
        """
        Read tabular data from the dataset and return a pandas dataframe
        """

    @abstractmethod
    def write(self, dataframe):
        """
        Write pandas dataframe to the dataset as tabular data
        """

    @abstractmethod
    def download(self, path):
        """
        Download the file(s) from the dataset
        """

    @abstractmethod
    def upload(self, path):
        """
        Upload file(s) to the dataset
        """
