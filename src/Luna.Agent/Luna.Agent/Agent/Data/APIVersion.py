from sqlalchemy import Column, Integer, String, Boolean
from Agent import Base, Session

class APIVersion(Base):
    """description of class"""
    
    __tablename__ = 'vw_apiversions'
    Id = Column(Integer, primary_key = True) 
    LunaAPIId = Column(Integer)
    VersionName = Column(String)
    AMLWorkspaceId = Column(Integer)
    AzureDatabricksWorkspaceId = Column(Integer)
    AzureSynapseWorkspaceId = Column(Integer)
    GitRepoId = Column(Integer)
    ModelDisplayName = Column(String)
    ModelName = Column(String)
    ModelVersion = Column(Integer)
    EndpointName = Column(String)
    EndpointVersion = Column(String)
    IsManualInputEndpoint = Column(Boolean)
    EndpointUrl = Column(String)
    EndpointSwaggerUrl = Column(String)
    EndpointAuthType = Column(String)
    EndpointAuthKey = Column(String)
    EndpointAuthAddTo = Column(String)
    EndpointAuthSecretName = Column(String)
    EndpointAuthTenantId = Column(String)
    EndpointAuthClientId = Column(String)
    GitVersion = Column(String)
    LinkedServiceType = Column(String)
    RunConfigFile = Column(String)
    IsUseDefaultRunConfig = Column(Boolean)
    IsRunProjectOnManagedCompute = Column(Boolean)
    LinkedServiceComputeTarget = Column(String)
    AdvancedSettings = Column(String)
    CreatedTime = Column(String)
    LastUpdatedTime = Column(String)
    ApplicationName = Column(String)
    APIName = Column(String)
    APIType = Column(String)

    @staticmethod
    def Get(applicationName, apiName, versionName):
        session = Session()
        version = session.query(APIVersion).filter_by(ApplicationName = applicationName, APIName = apiName, VersionName = versionName).first()
        session.close()
        return version