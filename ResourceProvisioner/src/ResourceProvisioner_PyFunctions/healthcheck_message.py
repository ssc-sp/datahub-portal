import json
import datetime

class HealthcheckMessage:
    def __init__(self, Type, Group, Name, Details):
        self.Type = Type
        self.Group = Group
        self.Name = Name
        self.Status = 5 # 5 = Undefined; 2 - Healthy; 4 - Unhealthy
        self.Details = Details
        self.HealthCheckTimeUtc = datetime.datetime.now()

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)