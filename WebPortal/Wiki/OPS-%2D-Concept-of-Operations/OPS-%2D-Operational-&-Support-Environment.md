## Operational Environment

### Environments
* Test: Test is a clone of production and is used to validate that the code doesn't depend on any specific data elements present in develop
* Develop: Develop represents the most up-to-date code. Code pushed to the develop branch gets automatically deployed to this infrastructure
* Production: Production is used for clients to process & access data.
* Production Stage: A separate slot in production for testing deployment before releasing to users.

### Git Process
::: mermaid
 graph LR;
 Develop --> Master;
:::

### New Feature Testing
::: mermaid
 graph LR;
 develop_build[Develop Build] --> develop[Develop Site];
 develop_build[Develop Build] --> test[Test Site];
:::

### Release Process
::: mermaid
 graph LR;
 master[Master Build] --> lndTests[Landing Tests];
 slot[Production Stage Slot] --> production[Production Site];
 lndTests --> slot;
:::

> Azure Automation Overview.docx
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/41788400>
>
> Azure Backup Overview.docx
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/41103514>
>
> Azure Client Onboarding Technical Overview.docx
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/44843520>
>
> Azure Network Overview.docx
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/41102711>
>
> Azure Policy Overview.docx
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/42726642>
>
> NRCan Azure Architecture.vsd
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/41098323>
>
> Sanitized NRCan Azure Architecture.vsd
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/48076642>
>
> NRCAN DataHub - QA - UAT Procedure.docx
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/54168312>

## Support Environment

> NRCAN DataHub - Mailbox Procedure.docx
>
> <https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/53744236>
