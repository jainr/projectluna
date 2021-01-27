from azureml.core import Workspace, Run, Experiment
from azureml.core.compute import ComputeTarget, AmlCompute
from azureml.core.environment import Environment
from azureml.core.model import Model, InferenceConfig
from azureml.core.runconfig import RunConfiguration
from azureml.core.webservice import AksWebservice, AciWebservice, Webservice
from azureml.pipeline.core.graph import PipelineParameter
from azureml.pipeline.steps import PythonScriptStep
from azureml.pipeline.core import Pipeline

from luna.baseLunaUtils import BaseLunaUtils
from luna.logging.azuremlLunaLogger import AzureMLLunaLogger

import os
import yaml
import tempfile
import json

class AzureMLLunaUtils(BaseLunaUtils):

    def Init(self, luna_config, run_mode, args, userInput):
        super().Init(luna_config, run_mode, args, userInput)
        self._logger = AzureMLLunaLogger()

    def GetAMLWorkspace(self):
        try:
            run = Run.get_context(allow_offline=False)
            return run.experiment.workspace
        except:
            return Workspace.from_config(path=self._luna_config["azureml"]["workspace_config_path"], 
                _file_name=self._luna_config["azureml"]["workspace_config_file_name"])
    
    def RegisterModel(self, model_path, description, luna_python_model=None):
        ws = self.GetAMLWorkspace()

        Model.register(model_path = model_path,
                       model_name = self._args.operationId,
                       description = description,
                       workspace = ws,
                       tags={'userId': self._args.userId, 
                        'productName': self._args.productName, 
                        'deploymentName': self._args.deploymentName, 
                        'apiVersion':self._args.apiVersion,
                        'subscriptionId':self._args.subscriptionId,
                        'modelId': self._args.operationId})

    def GetDeploymentConfig(self, tags, deployment_target='default', aks_cluster='default'):

        # Read default deployment target and aks cluster info from the config files
        
        workspace_full_path = os.path.join(self._luna_config['azureml']['workspace_config_path'], self._luna_config['azureml']['workspace_config_file_name'])
        with open(workspace_full_path) as file:
            documents = json.load(file)
            if deployment_target == 'default':
                deployment_target = documents['DeploymentTarget']
            if aks_cluster == 'default' and deployment_target == 'aks':
                aks_cluster = documents['AksCluster']

        with open(self._luna_config['deploy_config']) as file:
            documents = yaml.full_load(file)

            if deployment_target == 'aci':
                deployment_config = AciWebservice.deploy_configuration()
                deployment_config.__dict__.update(documents['azureContainerInstance'])
                deployment_config.dns_name_label = self._userInput["dns_name_label"]
            elif deployment_target == 'aks':
                deployment_config = AksWebservice.deploy_configuration()
                deployment_config.__dict__.update(documents['kubernetes'])
                deployment_config.compute_target_name = aks_cluster
                deployment_config.namespace = self._userInput["dns_name_label"]

            deployment_config.tags = tags
        return deployment_config

    def DeployModel(self):
        
        ws = self.GetAMLWorkspace()
        model = Model(ws, self._args.predecessorOperationId)
        myenv = Environment.from_conda_specification('scoring', self.luna_config['conda_env'])

        inference_config = InferenceConfig(entry_script=self.luna_config['code']['inference_entry_script'], source_directory = os.getcwd(), environment=myenv)

        deployment_config = self.GetDeploymentConfig(
            tags={'userId': self._args.userId, 
                'productName': self._args.productName, 
                'deploymentName': self._args.deploymentName, 
                'apiVersion':self._args.apiVersion,
                'subscriptionId':self._args.subscriptionId,
                'modelId': self._args.predecessorOperationId,
                'endpointId': self._args.operationId},
                deployment_target=self._args.deploymentTarget,
                aks_cluster=self._args.aksCluster)
        
        service = Model.deploy(ws, self._args.operationId, [model], inference_config, deployment_config)
        service.wait_for_deployment(show_output = True)

    def DownloadModel(self, model_path=""):
        ws = self.GetAMLWorkspace()
        model = Model(ws, self._args.predecessorOperationId)
        full_model_path = os.path.join(os.getcwd(), model_path, "models/artifacts")
        
        os.makedirs(full_model_path, exist_ok=True)
        model.download(target_dir = full_model_path, exist_ok=True)
        return full_model_path

    def FindPredecessorRun(self):
        run = Run.get_context(allow_offline=False)
        experiment = run.experiment
        run.tags.
        
        tags={'userId': self._args.userId,
              'subscriptionId':self._args.subscriptionId,
              'operationId': self._args.predecessorOperationId}

        runs = experiment.get_runs(type='azureml.PipelineRun', tags=tags)
        try:
            return next(runs)
        except StopIteration:
            return None

    def GetJsonOutputFromPredecessorRun(self):
        """
        Get JSON output from predecessor run
        Return None if the run is not found
        """
        predecessorRun = self.FindPredecessorRun()
        if predecessorRun:
            with tempfile.TemporaryDirectory() as tmp:
                path = os.path.join(tmp, self._args.predecessorOperationId, 'output.json')
                predecessorRun.download_file('/outputs/output.json', path)
                with open(path) as file:
                    return json.load(file)

        return None

    def DownloadOutputFilesFromPredecessorRun(self, targetFolder):
        """
        Download output files from predecessor run
        Return all file names if the run is found. Otherwise, return None
        """
        predecessorRun = self.FindPredecessorRun()
        if predecessorRun:
            childRuns = predecessorRun.get_children()
            try:
                childRun = next(childRuns)
            except StopIteration:
                return None

            if not os.path.exists(targetFolder):
                os.makedirs(targetFolder)
            childRun.download_files(prefix = 'outputs', output_directory=targetFolder, append_prefix=False)
            files = childRun.get_file_names()
            return [file for file in files if file.startswith("outputs/")]
        
        return None
        

    def WriteJsonOutput(self, content):
        """
        Write json output to current run
        """
        with tempfile.TemporaryDirectory() as tmp:
            path = os.path.join(tmp, self._args.predecessorOperationId, 'output.json')
            with open(path, 'w') as outfile:
                json.dump(content, outfile)

        self._logger.upload_artifacts(path, 'outputs/output.json')
    
    def UploadOutputFiles(self, sourceFolder):
        """
        Upload files to output of current run
        """
        self._logger.upload_artifacts(sourceFolder, 'outputs')