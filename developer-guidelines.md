# Developer Guidelines

Welcome to our developer guide! This document provides guidelines and best practices for developers working on this project.

## Coding Standards

- Follow the [Microsoft .NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).
- Use descriptive variable and method names and follow the [C# Naming Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names).
- Follow SOLID principles where applicable.
- Write clear and concise comments for non-obvious code sections.

## Folder Structure

- Separate projects based on domain centric boundaries.
- Follow a consistent naming convention for projects and test projects.

> Note: Projects marked with an asterisk (*) are part of the previous NRCan solution but are not actively maintained and will be moved or integrated into the new solution architecture.

```plaintext
/
├───Desktop
│   ├───Desktop.SharedCode
│   └───Desktop.Uploader
├───docs
├───infra
├───pipelines
├───Portal
│   ├───src
│   │   ├───Datahub.Achievements*
│   │   ├───Datahub.Application
│   │   ├───Datahub.CatalogSearch*
│   │   ├───Datahub.CKAN*
│   │   ├───Datahub.Core
│   │   ├───Datahub.Functions
│   │   ├───Datahub.GeoCore*
│   │   ├───Datahub.Infrastructure
│   │   ├───Datahub.Infrastructure.Offline
│   │   ├───Datahub.Maintenance*
│   │   ├───Datahub.Metadata*
│   │   ├───Datahub.Portal
│   │   ├───Datahub.Portal.Metadata*
│   │   ├───Datahub.PowerBI*
│   │   ├───Datahub.ProjectTools*
│   │   ├───Datahub.Stories
│   │   └───modules
│   │       ├───Datahub.Finance*
│   │       ├───Datahub.LanguageTraining*
│   │       ├───Datahub.M365Forms*
│   │       └───Datahub.PIP*
│   ├───test
│   │   ├───Datahub.Functions.UnitTests
│   │   ├───Datahub.Infrastructure.UnitTests
│   │   ├───Datahub.SpecflowTests
│   │   ├───Datahub.Specs
│   │   └───Datahub.Tests*
│   └───utils
│       ├───CatalogIngestTool*
│       ├───Datahub.MissingTranslations
│       ├───JsonTranslator*
│       ├───SyncDbUsers*
│       ├───SyncDBUsersConsole*
│       └───SyncDocs
├───ResourceProvisioner
│   ├───src
│   │   ├───ResourceProvisioner.API
│   │   ├───ResourceProvisioner.Application
│   │   ├───ResourceProvisioner.Functions
│   │   ├───ResourceProvisioner.Infrastructure
│   │   └───ResourceProvisioner_PyFunctions
│   └───test
│       ├───ResourceProvisioner.Application.IntegrationTests
│       ├───ResourceProvisioner.Application.UnitTests
│       ├───ResourceProvisioner.Domain.UnitTests
│       ├───ResourceProvisioner.Infrastructure.UnitTests
│       └───ResourceProvisioner_PyFunctions_Tests
├───scripts
├───Shared
│   └───src
│       ├───Datahub.Markdown
│       └───Datahub.Shared
├───SyncLocalization
└───utils
```


## Version Control

- Write clear and descriptive commit messages.
- Use initials for branch names to identify the developer working on the branch (e.g., `yr/task-name` or `yr/hotfix-name`).
- Use feature branches for long-lived feature development (with regular PRs from task branches for code review).
- Use hotfix branches for critical bug fixes.
- Use pull requests for code reviews to merge branches.
- Use the pull request template to provide a summary of changes, testing details, and checklist items.
- Follow [Git Flow](https://nvie.com/posts/a-successful-git-branching-model/) branching model.

## Dependency Management

- Use NuGet for managing third-party dependencies.
- Avoid adding unnecessary transitive dependencies and only add dependencies that are required.
- Verify the license of third-party dependencies to ensure compliance (MIT, Apache, etc.)
- Use the latest stable version of dependencies when possible.
- Package versions should use a `*` on minor versions to simplify updating dependencies.

## Testing

- Write unit tests for all critical business logic.
- Write specflow unit tests for all new code, when applicable.
- Use bUnit and specflow for Blazor component testing.
- Use a mocking framework like NSubstitute for unit testing dependencies.
- Aim for high code coverage but prioritize meaningful tests over coverage percentage.

## Documentation

- Keep [documentation](https://github.com/ssc-sp/datahub-docs) up-to-date with code changes. 

## Continuous Integration/Continuous Deployment (CI/CD)

- Use the ADO CI/CD pipelines to automate build, test, and deployment processes.
- Include and validate automated tests are passing in the CI pipeline for all pull requests.

## Security

- Follow OWASP guidelines for web application security.
- Store sensitive data securely in Azure Key Vaults, following best practices.
- Ensure that .githooks are running on the repository to prevent secrets from being committed.

## Miscellaneous

- Use `Localizer` for localizing strings in the application. Can leverage the `Datahub.MissingTranslations` tool to identify missing translations.
- Use the appropriate configuration objects for storing configuration, (i.e., `DatahubPortalConfiguration`, `ResourceProvisionerConfiguration`, etc).
- Encourage collaboration and communication among team members, and reach out to the team in the dev channel for any questions or concerns.
- When applicable, reference the Azure Devops work item number in the commit message (with `AB#{ID}`) to link them.

## Additional Resources

- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [C# Language Reference](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Specflow Documentation](https://specflow.org/documentation/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)

