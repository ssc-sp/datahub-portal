import json

class BugReportMessage:
    def __init__(self, UserName, UserEmail, UserOrganization, PortalLanguage, PreferredLanguage, Timezone, Workspaces, Topics, URL, UserAgent, Resolution, LocalStorage, BugReportType, Description):
        self.UserName = UserName
        self.UserEmail = UserEmail
        self.UserOrganization = UserOrganization
        self.PortalLanguage = PortalLanguage
        self.PreferredLanguage = PreferredLanguage
        self.Timezone = Timezone
        self.Workspaces = Workspaces
        self.Topics = Topics
        self.URL = URL
        self.UserAgent = UserAgent
        self.Resolution = Resolution
        self.LocalStorage = LocalStorage
        self.BugReportType = BugReportType
        self.Description = Description

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)