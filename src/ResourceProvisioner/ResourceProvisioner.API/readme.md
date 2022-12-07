# Getting Started

## Create an App Registration

Create an app registration `datahub-resourcing-provisioner` in the Azure Portal.  The app registration will be used to authenticate the client.

Click on manifest and set the `accessTokenAcceptedVersion` to `2` to enable the use of the v2.0 endpoint.

Expose delegated permissions (scopes), follow the steps in Configure an application to expose a web API.

```
Application ID URI: Accept the proposed application ID URI (api://<clientId>) (if prompted)
Scope name: access_as_user
Who can consent: Admins and users
Admin consent display name: Access DataHub Resourcing Provisioner as a user
Admin consent description: Accesses the DataHub Resourcing Provisioner web API as a user
User consent display name: Access DataHub Resourcing Provisioner as a user
User consent description: Accesses the DataHub Resourcing Provisioner web API as a user
State: Enabled
```


To generate the access token on behalf of the app registration