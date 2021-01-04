from sqlalchemy import Column, Integer, String, DateTime, or_
from Agent import Base, Session, app, key_vault_helper
from Agent.Data.AMLWorkspace import AMLWorkspace
from Agent.Data.AgentUser import AgentUser
from Agent.Exception.LunaExceptions import LunaServerException, LunaUserException
from http import HTTPStatus
import os

class Subscription(Base):
    """description of class"""

    __tablename__ = 'vw_subscriptions'

    Id = 0

    SubscriptionId = Column(String, primary_key = True)

    AIServiceId = Column(Integer)

    AIServicePlanId = Column(Integer)

    Owner = Column(String)

    Name = Column(String)

    Status = Column(String)

    CreatedTime = Column(DateTime)

    BaseUrl = Column(String)

    PrimaryKeySecretName = Column(String)

    SecondaryKeySecretName = Column(String)

    AIServiceName = Column(String)
    
    AIServicePlanName = Column(String)
    
    AMLWorkspaceName = ""

    AvailablePlans = []

    Users = []

    Admins = []

    PrimaryKey = ""

    SecondaryKey = ""

    @staticmethod
    def Get(subscriptionId, objectId="Admin"):
        """ the function will should only be called in local mode, otherwise, the keys might be out of date! """
        #if objectId != "Admin":
        #    # validate the userId
        #    users = AgentUser.ListAllBySubscriptionId(subscriptionId)
        #    if not any(user.ObjectId == objectId for user in users):
        #        raise LunaUserException(HTTPStatus.FORBIDDEN, "The subscription {} doesn't exist or you don't have permission to access it.".format(subscriptionId))

        session = Session()
        subscription = session.query(Subscription).filter_by(SubscriptionId = subscriptionId).first()
        session.close()
        if not subscription:
            return None
        subscription.PrimaryKey = key_vault_helper.get_secret(subscription.PrimaryKeySecretName)
        subscription.SecondaryKey = key_vault_helper.get_secret(subscription.SecondaryKeySecretName)
        #if os.environ["AGENT_MODE"] == "LOCAL" and objectId == "Admin":
        #    subscription.Admins = AgentUser.ListAllAdmin()
        #    subscription.Users = AgentUser.ListAllBySubscriptionId(subscriptionId)
        #    subscription.AvailablePlans = ["Basic", "Premium"]
        return subscription

    @staticmethod
    def GetByKey(subscriptionKey):
        session = Session()
        secret_name = key_vault_helper.find_secret_name_by_value(subscriptionKey)
        app.logger.info(secret_name)
        if secret_name:
            subscription = session.query(Subscription).filter(or_(Subscription.PrimaryKeySecretName == secret_name, Subscription.SecondaryKeySecretName == secret_name)).first()            
        else:
            subscription = None
        session.close()
        return subscription

    @staticmethod
    def ListAll():
        session = Session()
        subscriptions = session.query(Subscription).all()
        session.close()
        return subscriptions