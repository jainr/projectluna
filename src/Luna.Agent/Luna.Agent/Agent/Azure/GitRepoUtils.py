from uuid import uuid4
from luna.utils import ProjectUtils
from Agent import key_vault_client
import json
import tempfile
import zipfile
import os
import requests
from datetime import date, datetime
from Agent.Exception.LunaExceptions import LunaServerException, LunaUserException
from http import HTTPStatus
from adal import AuthenticationContext
import base64
from Agent.Data.GitRepo import GitRepo


class AzureDatabricksUtils(object):
    _personal_access_token = ""
    _repo = None

    def __init__(self, repo):
        if repo.PersonalAccessToken:
            repo.PersonalAccessToken =key_vault_client.get_secret(repo.PersonalAccessTokenSecretName).value
        self._repo = repo

    def readFile(self, path):

        return ""