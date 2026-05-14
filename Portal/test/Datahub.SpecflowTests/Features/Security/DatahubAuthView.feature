@IWebHostEnvironment
Feature: DatahubAuthView
    Validates the DatahubAuthView for both external and Entra users

    Scenario: A Workspace Lead DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceLead and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym | supportaccess |
            | WorkspaceLead | ABC | false |
            | DatahubSupport | DHPGLIST | true |

    Scenario: A Workspace Lead DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceLead and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   | supportaccess |
            | WorkspaceLead  | XYZ | false |
            | WorkspaceAdmin | ABC | false |
            | WorkspaceAdmin | XYZ | false |
            | WorkspaceCollaborator | ABC | false |
            | WorkspaceCollaborator | XYZ | false |
            | WorkspaceGuest | ABC | false |
            | WorkspaceGuest | XYZ | false |
            | ExternalUserWebApp | ABC | false |
            | ExternalUserWebApp | XYZ | false |
            | ExternalUserStorage | ABC | false |
            | ExternalUserStorage | XYZ | false |
            | DatahubSupport | DHPGLIST | false |
            | DatahubSupportAsGuest | DHPGLIST | true |
            | DatahubSupportAsGuest | DHPGLIST | false |

    Scenario: A Workspace Admin DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceAdmin and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym | supportaccess |
            | WorkspaceLead | ABC | false |
            | WorkspaceAdmin | ABC | false |
            | DatahubSupport | DHPGLIST | true |

    Scenario: A Workspace Admin DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceAdmin and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   | supportaccess |
            | WorkspaceLead  | XYZ | false |
            | WorkspaceAdmin | XYZ | false |
            | WorkspaceCollaborator | ABC | false |
            | WorkspaceCollaborator | XYZ | false |
            | WorkspaceGuest | ABC | false |
            | WorkspaceGuest | XYZ | false |
            | ExternalUserWebApp | ABC | false |
            | ExternalUserWebApp | XYZ | false |
            | ExternalUserStorage | ABC | false |
            | ExternalUserStorage | XYZ | false |
            | DatahubSupport | DHPGLIST | false |
            | DatahubSupportAsGuest | DHPGLIST | true |
            | DatahubSupportAsGuest | DHPGLIST | false |

    Scenario: A Workspace Collaborator DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceCollaborator and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym | supportaccess |
            | WorkspaceLead | ABC | false |
            | WorkspaceAdmin | ABC | false |
            | WorkspaceCollaborator | ABC | false |
            | DatahubSupport | DHPGLIST | true |

    Scenario: A Workspace Collaborator DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceCollaborator and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   | supportaccess |
            | WorkspaceLead  | XYZ | false |
            | WorkspaceAdmin | XYZ | false |
            | WorkspaceCollaborator | XYZ | false |
            | WorkspaceGuest | ABC | false |
            | WorkspaceGuest | XYZ | false |
            | ExternalUserWebApp | ABC | false |
            | ExternalUserWebApp | XYZ | false |
            | ExternalUserStorage | ABC | false |
            | ExternalUserStorage | XYZ | false |
            | DatahubSupport | DHPGLIST | false |
            | DatahubSupportAsGuest | DHPGLIST | true |
            | DatahubSupportAsGuest | DHPGLIST | false |

    Scenario: A Workspace Guest DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceGuest and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym | supportaccess |
            | WorkspaceLead | ABC | false |
            | WorkspaceAdmin | ABC | false |
            | WorkspaceCollaborator | ABC | false |
            | WorkspaceGuest | ABC | false |
            | DatahubSupport | DHPGLIST | true |

    Scenario: A Workspace Guest DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceGuest and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   | supportaccess |
            | WorkspaceLead  | XYZ | false |
            | WorkspaceAdmin | XYZ | false |
            | WorkspaceCollaborator | XYZ | false |
            | WorkspaceGuest | XYZ | false |
            | ExternalUserWebApp | ABC | false |
            | ExternalUserWebApp | XYZ | false |
            | ExternalUserStorage | ABC | false |
            | ExternalUserStorage | XYZ | false |
            | DatahubSupport | DHPGLIST | false |
            | DatahubSupportAsGuest | DHPGLIST | true |
            | DatahubSupportAsGuest | DHPGLIST | false |
            
    Scenario: An AllWorkspaceUsers DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel AllWorkspaceUsers and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym | supportaccess |
            | WorkspaceLead | ABC | false |
            | WorkspaceAdmin | ABC | false |
            | WorkspaceCollaborator | ABC | false |
            | WorkspaceGuest | ABC | false |
            | ExternalUserWebApp | ABC | false |
            | ExternalUserStorage | ABC | false |
            | DatahubSupport | DHPGLIST | true |

    Scenario: An AllWorkspaceUsers DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel AllWorkspaceUsers and ElevatedWorkspaceAccessEnabled <supportaccess>
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   | supportaccess |
            | WorkspaceLead  | XYZ | false |
            | WorkspaceAdmin | XYZ | false |
            | WorkspaceCollaborator | XYZ | false |
            | WorkspaceGuest | XYZ | false |
            | ExternalUserWebApp | XYZ | false |
            | ExternalUserStorage | XYZ | false |
            | DatahubSupport | DHPGLIST | false |
            | DatahubSupportAsGuest | DHPGLIST | true |
            | DatahubSupportAsGuest | DHPGLIST | false |

    Scenario: A DatahubSupport DatahubAuthView should allow administrator access
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView with AuthLevel DatahubSupport
        When the user views the component
        Then they should be able to view it
        Examples:
            | role    | acronym   | supportaccess |
            | DatahubSupport | DHPGLIST | false |

    Scenario: A DatahubSupport DatahubAuthView should block non-administrator access
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView with AuthLevel DatahubSupport
        When the user views the component
        Then they should not be able to view it
        Examples:
            | role    | acronym   | supportaccess |
            | WorkspaceLead  | XYZ | false |
            | WorkspaceAdmin | XYZ | false |
            | WorkspaceCollaborator | XYZ | false |
            | WorkspaceGuest | XYZ | false |
            | ExternalUserWebApp | XYZ | false |
            | ExternalUserStorage | XYZ | false |
            | DatahubSupportAsGuest | DHPGLIST | false |

    Scenario: An Authenticated DatahubAuthView should allow anyone to access
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView with AuthLevel Authenticated
        When the user views the component
        Then they should be able to view it
        Examples:
            | role    | acronym   | supportaccess |
            | DatahubSupport | DHPGLIST | false |
            | WorkspaceLead  | XYZ | false |
            | WorkspaceAdmin | XYZ | false |
            | WorkspaceCollaborator | XYZ | false |
            | WorkspaceGuest | XYZ | false |
            | ExternalUserWebApp | XYZ | false |
            | ExternalUserStorage | XYZ | false |
            | DatahubSupportAsGuest | DHPGLIST | false |