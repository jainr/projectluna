from sqlalchemy import Column, Integer, String, or_
from Agent import Base, Session, key_vault_helper

class GitRepo(Base):
    """description of class"""
    
    __tablename__ = 'gitrepos'

    Id = Column(Integer, primary_key = True)

    RepoName = Column(String)

    Type = Column(String)

    HttpUrl = Column(String)

    PersonalAccessTokenSecretName = Column(String)

    PersonalAccessToken = ""

    @staticmethod
    def ListAll():
        session = Session()
        repos = session.query(GitRepo).all()
        session.close()
        return repos
    
    @staticmethod
    def GetById(id):
        session = Session()
        # Find the model by modelName first
        repo = session.query(GitRepo).filter_by(Id = id).first()
        session.close()

        repo.PersonalAccessToken = key_vault_helper.get_secret(repo.PersonalAccessTokenSecretName)
        return repo

    @staticmethod
    def Get(name):
        session = Session()
        # Find the model by modelName first
        repo = session.query(GitRepo).filter_by(RepoName = name).first()
        session.close()

        repo.PersonalAccessToken = key_vault_helper.get_secret(repo.PersonalAccessTokenSecretName)
        return repo