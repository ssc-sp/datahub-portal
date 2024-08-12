import uuid
import json
import datetime

class MassTransitMessage:

    TYPE_BUG_REPORT = "BugReportMessage"
    TYPE_HEALTH_CHECK_RESULT = "InfrastructureHealthCheckResultMessage"

    def __generate_dest_addr(self, client_namespace, queue_name):
        return f"sb://{client_namespace}/{queue_name}"
    
    def __generate_src_addr(self):
        # TODO make a more legit version of this
        return "n/a"

    def __init__(self, message, client_namespace, queue_name, message_type):
        my_id = str(uuid.uuid4())
        self.messageId = my_id
        self.conversationId = my_id
        self.sourceAddress = self.__generate_src_addr()
        self.destinationAddress = self.__generate_dest_addr(client_namespace, queue_name)
        self.message = message
        self.messageType = [
            f"urn:message:Datahub.Infrastructure.Queues.Messages:{message_type}",
            "urn:message:MediatR:IRequest",
            "urn:message:MediatR:IBaseRequest"
        ]

    def to_json(self):
        def json_default(value):
            if isinstance(value, datetime.date):
                return value.isoformat()
            else:
                return value.__dict__            
        return json.dumps(self, default=json_default, sort_keys=True, indent=4)