from sqlalchemy import Column, Integer, String
from Agent import Base, Session

class PlanApplicationAPI(Base):
    """description of class"""
    
    __tablename__ = 'vw_planapplicationapis'
    PlanId = Column(Integer, primary_key = True)

    ApplicationName = Column(String, primary_key = True)

    APIName = Column(String, primary_key = True)

    APIType = Column(String)

    @staticmethod
    def Exists(planId, applicationName, apiName):
        session = Session()
        publisher = session.query(PlanApplicationAPI).filter_by(PlanId = planId, ApplicationName = applicationName, APIName = apiName).first()
        session.close()
        return publisher
