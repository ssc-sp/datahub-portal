# Welcome to DataHub 

This repository contains the source code the Federal Science DataHub. 


| [View Priority Bugs](https://github.com/ssc-sp/datahub-portal/issues?q=is%3Aissue+is%3Aopen+label%3Abug%2Cpriority) | [View Milestone Bugs](https://github.com/ssc-sp/datahub-portal/issues?q=is%3Aissue+is%3Aopen+label%3Abug+milestone%3A%22Pilot+2.0.0%22) | 
| --- | --- |
| ![GitHub issue custom search](https://img.shields.io/github/issues-search?color=red&label=priority%20bugs&query=repo%3Assc-sp%2Fdatahub-portal%20is%3Aissue%20is%3Aopen%20label%3Abug%2Cpriority) | ![GitHub issue custom search](https://img.shields.io/github/issues-search?color=red&label=milestone%20bugs&query=repo%3Assc-sp%2Fdatahub-portal%20is%3Aissue%20is%3Aopen%20label%3Abug%20milestone%3A%22Pilot%202.0.0%22) |

| [View Milestone Planning](https://github.com/orgs/ssc-sp/projects/4/views/25) |
| --- |


## Learn more about DataHub

[See our documentation](https://ssc-sp.github.io/datahub-docs/#/)

[Our documentation repository](https://github.com/ssc-sp/datahub-docs)

# Github Structure

This project includes multiple projects
- **DataHub**, contains the code for the application and presentation layer of the modules.
- **Resource Provisioner** contains the code to handle the terraform infrastructure for the Datahub
- **Shared** contains any code that is shared between the projects

## Commit Messages

The commit messages must loosely follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification. This enables to automatically generate the changelog and release notes.

The scopes are represented by the following table


| Type | Emoji | Code |
| --- | --- | --- |
| feat | ✨ | `:sparkles:` |
| fix | 🐛 | `:bug:` |
| docs | 📚 | `:books:` |
| style | 💎 | `:gem:` |
| refactor | 🔨 | `:hammer:` |
| deploy | 🚀 | `:rocket:` |
| test | :test_tube: | `:test_tube:` |
| build | 📦 | `:package:` |
| ci | 👷 | `:construction_worker:` |
| chore | 🔧 | `:wrench:` |
| work in progress (WIP) | 🚧 | `:construction:` |
| accessibility | 🦜 | `:parrot:` |

## Branching

Branches are created from the develop branch and merged back into the develop branch. The master branch is used for releases only. Pull requests are strongly encouraged and should be reviewed by at least one other developer.

--------------------------------

# Bienvenue à DataHub 

Ce dépôt contient le code source du DataHub fédéral. 

## En savoir plus sur le DataHub

[Notre documentation](https://ssc-sp.github.io/datahub-docs/#/)

[Notre dépôt pour la documentation](https://github.com/ssc-sp/datahub-docs)

## Structure de Github

Ce projet comprend plusieurs projets
- **DataHub**, contient le code pour l'application et la couche de présentation des modules.
- **Resource Provisioner** contient le code pour gérer l'infrastructure terraform pour le Datahub.
- **Shared** contient le code qui est partagé entre les projets.

## Messages de commit

Les messages de validation doivent suivre de près la spécification [Conventional Commits] (https://www.conventionalcommits.org/en/v1.0.0/). Cela permet de générer automatiquement le changelog et les notes de version.

Les champs d'application sont représentés par le tableau suivant


| Type | Emoji | Code |
| --- | --- | --- |
| fonctionnalité | ✨ | `:sparkles:` |
| correction | 🐛 | `:bug:` |
| | docs | 📚 | `:books:` |
| | style | 💎 | `:gem:` | style | 💎 | `:gem:` | refactor
| | refactor | 🔨 | `:hammer:` | refactoring
| | deploy | 🚀 | `:rocket:` |
| test | :test_tube : | `:test_tube:` |
| compilation | 📦 | `:package:` |
| | ci | 👷 | `:construction_worker:` |
| corvée | 🔧 | `:wrench:`
| travail en cours (WIP) | 🚧 | `:construction:` | travaux en cours (WIP)
| accessibilité | 🦜 | `:parrot:` |

## Branches

Les branches sont créées à partir de la branche de développement et fusionnées dans la branche de développement. La branche master est utilisée uniquement pour les versions. Les Pull requests sont fortement encouragées et doivent être revues par au moins un autre développeur.
