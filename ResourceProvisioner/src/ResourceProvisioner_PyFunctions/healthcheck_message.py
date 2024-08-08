import json
import datetime

class HealthcheckMessage:
    # 5 = Undefined; 2 - Healthy; 4 - Unhealthy
    STATUS_HEALTHY = 2
    STATUS_UNHEALTHY = 4
    STATUS_UNDEFINED = 5

    TYPE_WORKSPACE_SYNC = 8

    def __init__(self, Type, Group, Name, Details = None, Status = None):
        self.Type = Type
        self.Group = Group
        self.Name = Name
        self.Status = Status or self.STATUS_UNDEFINED 
        self.Details = Details or ""
        self.HealthCheckTimeUtc = datetime.datetime.now()

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)