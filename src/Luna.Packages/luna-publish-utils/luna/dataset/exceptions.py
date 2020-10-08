
class LunaDatasetNotSupportedException(Exception):

    def __init__(self, message):
        self.message = message

class LunaInvalidConfigException(Exception):

    missing_property_list = []

    def __init__(self, message, missing_property_list=[]):
        self.message = message
        self.missing_property_list = missing_property_list