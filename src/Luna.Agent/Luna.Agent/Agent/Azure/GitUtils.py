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
import yaml


class GitUtils(object):
    _personal_access_token = ""
    _repo = None

    def __init__(self, repo):
        if repo.PersonalAccessToken:
            repo.PersonalAccessToken =key_vault_client.get_secret(repo.PersonalAccessTokenSecretName).value
        self._repo = repo

    def getEntryPoints(self, version):
        headers = {'Content-Type': 'application/vnd.github.v3.raw'}
        headers['Authorization'] = "token {}".format(self._repo.PersonalAccessToken)
        sections = self._repo.HttpUrl.split('/')
        owner = sections[3]
        repo = sections[4][:-4]
        requestUrl = "https://api.github.com/repos/{}/{}/contents/MLproject?ref={}".format(owner, repo, version)
        response = requests.get(requestUrl, headers=headers)
        base64_message = json.loads(response.content)["content"]
        result = base64.b64decode(base64_message)
        mlproject = yaml.load(result)
        operations = []
        for entry_point in mlproject['entry_points'].keys():
            operation = {
                'name': entry_point,
                'parameters': mlproject['entry_points'][entry_point]['parameters']
                }
            operations.append(operation)
        return operations

    def readFile(self, path):

        return ""