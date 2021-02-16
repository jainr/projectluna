from sqlalchemy import Column, Integer, String
from Agent import Base, Session, key_vault_helper
from Agent.Azure.AzureMLUtils import AzureMLUtils
import uuid

class AMLWorkspace(Base):
    """description of class"""
    
    __tablename__ = 'amlworkspaces'

    Id = Column(Integer, primary_key = True)

    WorkspaceName = Column(String)

    ResourceId = Column(String)

    AADApplicationId = Column(String)

    AADTenantId = Column(String)

    AADApplicationSecretName = Column(String)

    AADApplicationSecret = ""

    ComputeClusters = []

    DeploymentClusters = []

    def GetByIdWithSecrets(workspaceId):
        session = Session()
        workspace = session.query(AMLWorkspace).filter_by(Id = workspaceId).first()
        workspace.AADApplicationSecret = key_vault_helper.get_secret(workspace.AADApplicationSecretName)
        session.close()
        return workspace
