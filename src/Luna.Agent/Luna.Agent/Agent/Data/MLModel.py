from sqlalchemy import Column, Integer, String, or_
from Agent import Base, Session

class MLModel(Base):
    """description of class"""
    
    __tablename__ = 'mlmodels'

    Id = Column(Integer, primary_key = True)

    APIVersionId = Column(Integer)

    ModelName = Column(String)

    ModelDisplayName = Column(String)

    ModelVersion = Column(String)

    @staticmethod
    def ListAll(apiVersionId):
        session = Session()
        models = session.query(MLModel).filter_by(APIVersionId = apiVersionId).all()
        session.close()
        return models
    
    @staticmethod
    def Get(apiVersionId, modelName):
        session = Session()
        # Find the model by modelName first
        model = session.query(MLModel).filter_by(APIVersionId = apiVersionId, ModelName = modelName).first()

        # If doesn't exist, find model by alternative name
        if not model:
            model = session.query(MLModel).filter_by(APIVersionId = apiVersionId, ModelAlternativeName = modelName).first()
        session.close()
        return model