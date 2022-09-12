# [UC001] New User
## Sequence Diagram
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
## Mockups
![user sign up button](./images/user-sign-up-button.jpg)
Sign-in button appears in top right of screen

![user registration](./images/user-information.jpg)
A similar user registration form is shown

![user registration gov](./images/user-information-gov.jpg)
If email entered is a GC email, request department

![email validation](./images/email-verification.webp)
An email verification will be sent to the user

# [UC002] Project Creation (GC Users only)
As a **GC user**, I want to create a new project (Initiative)

## Sequence Diagram
```mermaid
sequenceDiagram
    actor U as User
    participant P as DataHub Portal
    actor D as DataHub Admin

    U->>P: request new project with project name
    activate P
    P->>P: provision limited project
    P->>P: add user as admin to project
    P-->>U: show limited project page
    deactivate P
    activate U
    U->>P: submit DMP form
    deactivate U
    P->>D: request approval
    D-->>P: approve
    P->>P: provision full-access project
    P-->>U: email notification
```

## Mockups
![new initiative](./images/create-new%20initative.jpg)
A card for new project will be displayed, amongst an other projects the user has.

The user will be required to enter the name of the project.

![project unconfirmed page](./images/unconfirmed-project.jpg)
Once the user fills out the name of the project, they will be required to fill out the DMP form before gaining full access.
# [UC003] New user and existing project

As a new user, I want to sign up to an existing project in DataHub

## Sequence Diagram

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



# [UC004] Existing user and existing project
As an existing user, I want to join an existing project in DataHub

## Sequence Diagram

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
