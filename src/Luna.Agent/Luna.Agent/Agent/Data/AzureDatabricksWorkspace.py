from sqlalchemy import Column, Integer, String
from Agent import Base, Session, key_vault_helper
from Agent.Azure.AzureDatabricksUtils import AzureDatabricksUtils
import uuid

class AzureDatabricksWorkspace(Base):
    """description of class"""
    
    __tablename__ = 'AzureDatabricksWorkspaces'

    Id = Column(Integer, primary_key = True)

    WorkspaceName = Column(String)

    ResourceId = Column(String)

    WorkspaceUrl = Column(String)

    AADApplicationId = Column(String)

    AADTenantId = Column(String)

    AADApplicationSecretName = Column(String)

    AADApplicationSecret = ""

    def GetByIdWithSecrets(workspaceId):
        session = Session()
        workspace = session.query(AzureDatabricksWorkspace).filter_by(Id = workspaceId).first()
        workspace.AADApplicationSecret = key_vault_helper.get_secret(workspace.AADApplicationSecretName)
        session.close()
        return workspace
