from sqlalchemy import Column, Integer, String, or_
from Agent import Base, Session

class AMLPipelineEndpoint(Base):
    """description of class"""
    
    __tablename__ = 'AMLPipelineEndpoints'

    Id = Column(Integer, primary_key = True)

    APIVersionId = Column(Integer)

    PipelineEndpointName = Column(String)

    PipelineEndpointId = Column(String)

    @staticmethod
    def ListAll(apiVersionId):
        session = Session()
        pipelines = session.query(AMLPipelineEndpoint).filter_by(APIVersionId = apiVersionId).all()
        session.close()
        return pipelines
    
    @staticmethod
    def Get(apiVersionId, pipelineName):
        session = Session()
        model = session.query(AMLPipelineEndpoint).filter_by(APIVersionId = apiVersionId, PipelineEndpointName = pipelineName).first()

        session.close()
        return model