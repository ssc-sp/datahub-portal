@IWebHostEnvironment
Feature: DatahubAuthView
    Validates the DatahubAuthView for both external and Entra users

    Scenario: A Workspace Lead DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceLead
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym |
            | WorkspaceLead | ABC |

    Scenario: A Workspace Lead DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceLead
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   |
            | WorkspaceLead  | XYZ |
            | WorkspaceAdmin | ABC |
            | WorkspaceAdmin | XYZ |
            | WorkspaceCollaborator | ABC |
            | WorkspaceCollaborator | XYZ |
            | WorkspaceGuest | ABC |
            | WorkspaceGuest | XYZ |
            | ExternalUserWebApp | ABC |
            | ExternalUserWebApp | XYZ |
            | ExternalUserStorage | ABC |
            | ExternalUserStorage | XYZ |

    Scenario: A Workspace Admin DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceAdmin
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym |
            | WorkspaceLead | ABC |
            | WorkspaceAdmin | ABC |

    Scenario: A Workspace Admin DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceAdmin
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   |
            | WorkspaceLead  | XYZ |
            | WorkspaceAdmin | XYZ |
            | WorkspaceCollaborator | ABC |
            | WorkspaceCollaborator | XYZ |
            | WorkspaceGuest | ABC |
            | WorkspaceGuest | XYZ |
            | ExternalUserWebApp | ABC |
            | ExternalUserWebApp | XYZ |
            | ExternalUserStorage | ABC |
            | ExternalUserStorage | XYZ |

    Scenario: A Workspace Collaborator DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceCollaborator
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym |
            | WorkspaceLead | ABC |
            | WorkspaceAdmin | ABC |
            | WorkspaceCollaborator | ABC |

    Scenario: A Workspace Collaborator DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceCollaborator
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   |
            | WorkspaceLead  | XYZ |
            | WorkspaceAdmin | XYZ |
            | WorkspaceCollaborator | XYZ |
            | WorkspaceGuest | ABC |
            | WorkspaceGuest | XYZ |
            | ExternalUserWebApp | ABC |
            | ExternalUserWebApp | XYZ |
            | ExternalUserStorage | ABC |
            | ExternalUserStorage | XYZ |

    Scenario: A Workspace Guest DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceGuest
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym |
            | WorkspaceLead | ABC |
            | WorkspaceAdmin | ABC |
            | WorkspaceCollaborator | ABC |
            | WorkspaceGuest | ABC |

    Scenario: A Workspace Guest DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceGuest
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   |
            | WorkspaceLead  | XYZ |
            | WorkspaceAdmin | XYZ |
            | WorkspaceCollaborator | XYZ |
            | WorkspaceGuest | XYZ |
            | ExternalUserWebApp | ABC |
            | ExternalUserWebApp | XYZ |
            | ExternalUserStorage | ABC |
            | ExternalUserStorage | XYZ |
            
    Scenario: An AllWorkspaceUsers DatahubAuthView should allow an authorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel AllWorkspaceUsers
        When the user views the component
        Then they should be able to view it
        Examples:
            | role | acronym |
            | WorkspaceLead | ABC |
            | WorkspaceAdmin | ABC |
            | WorkspaceCollaborator | ABC |
            | WorkspaceGuest | ABC |
            | ExternalUserWebApp | ABC |
            | ExternalUserStorage | ABC |

    Scenario: An AllWorkspaceUsers DatahubAuthView should not allow an unauthorized user to access the content
        Given a <role> user for workspace <acronym>
        And a DatahubAuthView for workspace ABC and AuthLevel AllWorkspaceUsers
        When the user views the component
        Then they should not be able to view it
        Examples: 
            | role    | acronym   |
            | WorkspaceLead  | XYZ |
            | WorkspaceAdmin | XYZ |
            | WorkspaceCollaborator | XYZ |
            | WorkspaceGuest | XYZ |
            | ExternalUserWebApp | XYZ |
            | ExternalUserStorage | XYZ |