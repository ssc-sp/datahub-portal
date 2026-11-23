import contextvars
import logging


_workspace_id = contextvars.ContextVar("workspace_id", default=None)
_base_log_record_factory = logging.getLogRecordFactory()


def _workspace_log_record_factory(*args, **kwargs):
    record = _base_log_record_factory(*args, **kwargs)
    workspace_id = _workspace_id.get()
    if workspace_id:
        record.workspace_id = workspace_id
        if not str(record.msg).startswith("[workspace_id="):
            record.msg = "[workspace_id=%s] %s" % (workspace_id, record.getMessage())
            record.args = ()
    return record


logging.setLogRecordFactory(_workspace_log_record_factory)


def set_workspace_id(workspace_definition):
    workspace = workspace_definition.get("Workspace", {}) if isinstance(workspace_definition, dict) else {}
    if not isinstance(workspace, dict):
        workspace = {}

    workspace_id = None
    for key in ("Id", "WorkspaceId", "WorkspaceID", "ProjectId", "Acronym"):
        value = workspace.get(key)
        if value is not None:
            value = str(value).strip()
            if value:
                workspace_id = value
                break

    return _workspace_id.set(workspace_id or "unknown")


def reset_workspace_id(token):
    _workspace_id.reset(token)


class WorkspaceContextFilter(logging.Filter):
    def filter(self, record):
        workspace_id = _workspace_id.get()
        if workspace_id:
            record.msg = "[workspace_id=%s] %s" % (workspace_id, record.getMessage())
            record.args = ()
        return True
