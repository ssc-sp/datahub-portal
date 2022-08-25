# [UC001] New user and new project

## User Creation
### Sequence Diagram
```mermaid
sequenceDiagram
    actor U as New User
    participant P as DataHub Portal
    participant F as Graph Function

    U->>P: /register/new
    P-->>U: show sign up page

    U->>P: submit account info (email, password)
    opt is GC email
    P-->>U: request department info
    U->>P: submit department info
    end

    P->>+F: create user
    F->>U: email validation
    activate U
    F-->>-P: notify success
    P-->>+U: notify to confirm account

    U->>-P: select email validation link
    P->>F: confirm account
    F-->>P: notify success
    P-->>U: notify success
```
### Mockups
![user sign up button](./images/user-sign-up-button.jpg)
![user registration](./images/user-information.jpg)
![user registration gov](./images/user-information-gov.jpg)
![email validation](./images/email-verification.webp)


# [UC002] New user and existing project
As a new user, I want to sign up to an existing project in DataHub

```mermaid
sequenceDiagram
    actor U as  User
    actor A as Project Admin
    participant P as DataHub Portal
    actor C as Cloud Admin

    A->>+P: request send invite link (email + project)
    activate A
    P->>P: insert registration request (invite)
    P->>U: email invite link (guid)
    activate U
    P-->>A: notify email sent
    deactivate A

    deactivate P
    U->>+P: /register/{guid}
    P->>P: update registration request (create)
    P-->>U: notify pending
    deactivate U
    
    P->>+C: request user creation
    deactivate P
    C->>C: create user in AAD
    C->>-P: notify DataHub

    activate P
    P->>P: add user to project 
    P->>U: email notification
    deactivate P

    activate U
    U->>-P: login to portal
    activate P
    P->>-P: update registration status (logged in)
```

# [UC003] Existing user and new project
As an existing user, I want to create a new project in DataHub

```mermaid
sequenceDiagram
    actor U as User
    participant P as DataHub Portal
    actor D as DataHub Admin

    U->>P: request new project
    activate U
    activate P
    P->>+D: request approval
    P-->>U: notify pending
    deactivate U
    D-->>-P: approve
    P->>P: provision project
    P->>P: add user as admin to project
    P->>-U: email notification
```

# [UC003] Existing user and existing project
As an existing user, I want to join an existing project in DataHub

```mermaid
sequenceDiagram
    actor U as User
    actor A as Project Admin
    participant P as DataHub Portal

    U->>P: request to join existing project
    activate U
    activate P
    P->>P: create join request
    P-->>U: notify pending
    deactivate U

    P->>A: notify of join request
    deactivate P
    activate A
    A-->>P: approve join request
    deactivate A

    activate P
    P->>P: add user to project
    P->>U: email notification
    deactivate P
```

### Old (TEMP!)
```mermaid
sequenceDiagram
    actor A as DataHub Admin
    actor U as User
    participant P as DataHub Portal
    participant F as Graph Function

    U->>+P: /register/new
    P->>P: insert registration request (requested)
    P-->>U: notify pending
    P-->>A: notify pending
    deactivate P
    activate A

    
    A->>-P: approve
    activate P
    P->>P: create project
    P->>P: create storage request for project
    P->>+F: create user
    F->>U: email notification
    activate U
    F->>-P: notify success

    P->>P: set user as project admin
    P->>P: update registration status (created) 
    deactivate P

    U->>-P: login to portal
    activate P
    P->>-P: update registration status (logged in)
```