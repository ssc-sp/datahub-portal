[*La version française suit*](#la-plateforme-f%C3%A9d%C3%A9rale-de-donn%C3%A9es-scientifiques)

<div align="center">

![The Federal Science DataHub](./docs/images/fsdh-2.jpg)

![MIT Licence](https://img.shields.io/badge/licence-mit-blue) ![GitHub contributors](https://img.shields.io/github/contributors/ssc-sp/datahub-portal) ![GitHub Repo stars](https://img.shields.io/github/stars/ssc-sp/datahub-portal) ![GitHub Release](https://img.shields.io/github/v/release/ssc-sp/datahub-portal) ![GitHub branch status](https://img.shields.io/github/checks-status/ssc-sp/datahub-portal/develop?label=build)

[Documentation](https://github.com/ssc-sp/datahub-docs) | [Developer Guidelines](developer-guidelines.md) | [Contributing](CONTRIBUTING.md) | [Security](SECURITY.md)

</div>

# The Federal Science DataHub

The **Federal Science DataHub (FSDH)** is a cloud-based platform tailored to support science, research, and data collaboration within the Government of Canada. It provides secure data storage, analytics, collaboration, and AI capabilities, while ensuring that data ownership remains with federal organizations.

This repository hosts the *portal* and supporting infrastructure code for DataHub, enabling scientists and research staff to access, manage, and work with data in a shared environment.

[Learn more about the Federal Science DataHub.](https://www.canada.ca/en/shared-services/services/tools-to-equip-gc-workers/tools-science/federal-science-datahub.html)

## Features & Capabilities

- Security assessment and authorization
- Accessibility (WCAG 2.1 AA)
- Official languages support
- Tool provisioning
- User management and authentication
- Configuration for cross-departmental collaboration

## Repository Structure

```
├── Portal                # The front-end/back-end web application code (UI, APIs)
├── ResourceProvisioner   # Infrastructure-as-code (Terraform, deployment scripts)
├── Shared                # Shared libraries and modules used across components
├── ServerlessOperations  # Event-driven / serverless components
├── infra                 # Infrastructure definitions or scaffolding
├── pipelines             # CI/CD and release pipelines
├── scripts               # Utility or automation scripts
├── utils                 # Utility code/helpers
```

## Release & Versioning

This project uses [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) for commit messages. This helps automate the release process and generate changelogs.

* Types of commits:
  - `feat`: A new feature
  - `fix`: A bug fix
  - `docs`: Documentation changes
  - `style`: Code style changes (formatting, missing semicolons, etc.)
  - `refactor`: Code changes that neither fix a bug nor add a feature
  - `test`: Adding or updating tests
  - `chore`: Changes to the build process or auxiliary tools
  - `perf`: A code change that improves performance

Pull requests should merge into the `develop` branch. Releases are automatically created from this branch using the commit messages to determine the version bump (major, minor, patch).

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests.

## Security

If you discover a security vulnerability within this project, please consult our [SECURITY.md](SECURITY.md) policy for reporting. Please do not create a public issue for security vulnerabilities.

## Documentation & Support

For detailed documentation, setup guides, and FAQs, please refer to the [DataHub Documentation](https://github.com/ssc-sp/datahub-docs).

For developer support, you can open an issue in this repository or reach out to the maintainers directly.

For general information about the Federal Science DataHub, visit the [Canada.ca site](https://www.canada.ca/en/shared-services/services/tools-to-equip-gc-workers/tools-science/federal-science-datahub.html). Government of Canada employees can also visit our [GCXchange page](https://gcxgce.sharepoint.com/teams/10002160/SitePages/Home.aspx) (accessible within the GC network).


---

[*English version above*](#the-federal-science-datahub)

<div align="center">

![La Plateforme fédérale de données scientifiques](./docs/images/fsdh-fr.png)

![Licence MIT](https://img.shields.io/badge/licence-mit-blue) ![GitHub contributors](https://img.shields.io/github/contributors/ssc-sp/datahub-portal?label=contributeurs) ![GitHub Repo stars](https://img.shields.io/github/stars/ssc-sp/datahub-portal?label=%C3%89toiles) ![GitHub Release](https://img.shields.io/github/v/release/ssc-sp/datahub-portal?label=version) ![GitHub branch status](https://img.shields.io/github/checks-status/ssc-sp/datahub-portal/develop?label=compilation)

[Documentation](https://github.com/ssc-sp/datahub-docs) | [Consignes pour les développeurs](developer-guidelines.md) | [Contribution](CONTRIBUTING.md) | [Sécurité](SECURITY.md)

</div>

# La Plateforme fédérale de données scientifiques

La **Plateforme fédérale de données scientifiques (PFDS)** est une plateforme basée sur le nuage conçue pour soutenir la science, la recherche et la collaboration en matière de données au sein du gouvernement du Canada. Elle offre un stockage sécurisé des données, des capacités d'analyse, de collaboration et d'IA, tout en garantissant que la propriété des données reste aux organisations fédérales.

Ce repo héberge le *portail* et le code d'infrastructure de soutien pour la PFDS, permettant aux scientifiques et au personnel de recherche d'accéder, de gérer et de travailler avec les données dans un environnement partagé.

[En savoir plus sur la Plateforme fédérale de données scientifiques.](https://www.canada.ca/fr/services-partages/services/outils-pour-personnes-travaillant-gc/outils-scientifique/plateforme-federale-donnees-scientifiques.html)

## Fonctionnalités et capacités

- Évaluation et autorisation de la sécurité
- Accessibilité (WCAG 2.1 AA)
- Support des langues officielles
- Provisionnement d'outils
- Gestion des utilisateurs et authentification
- Configuration pour la collaboration inter-départementale  

## Structure du dépôt

```
├── Portal                 # Le code de l'application web front-end/back-end (UI, APIs)
├── ResourceProvisioner    # Infrastructure en tant que code (Terraform, scripts de déploiement)
├── Shared                 # Bibliothèques et modules partagés utilisés dans les composants
├── ServerlessOperations   # Composants événementiels / sans serveur
├── infra                  # Définitions ou échafaudages d'infrastructure
├── pipelines              # Pipelines CI/CD et de publication
├── scripts                # Scripts utilitaires ou d'automatisation
├── utils                  # Code utilitaire/aides
```

## Publication et versionnage

Ce projet utilise les [Commits Conventionnels](https://www.conventionalcommits.org/fr/v1.0.0/) pour les messages de commit. Cela aide à automatiser le processus de publication et à générer des journaux de modifications.

* Types de commits :
  - `feat` : Une nouvelle fonctionnalité
  - `fix` : Une correction de bug
  - `docs` : Modifications de la documentation
  - `style` : Modifications du style de code (formatage, points-virgules manquants, etc.)
  - `refactor` : Modifications du code qui ne corrigent ni ne ajoutent une fonctionnalité
  - `test` : Ajout ou mise à jour des tests
  - `chore` : Modifications du processus de construction ou des outils auxiliaires
  - `perf` : Une modification du code qui améliore les performances

Les demandes de tirage doivent fusionner dans la branche `develop`. Les versions sont créées automatiquement à partir de cette branche en utilisant les messages de commit pour déterminer l'augmentation de version (majeure, mineure, patch).

## Contribution

Les contributions sont les bienvenues! Veuillez consulter le fichier [CONTRIBUTING.md](CONTRIBUTING.md) pour plus de détails sur notre code de conduite et le processus de soumission des demandes de tirage.

## Sécurité

Si vous découvrez une vulnérabilité de sécurité dans ce projet, veuillez consulter notre politique [SECURITY.md](SECURITY.md) pour le signalement. Veuillez ne pas créer de problème public pour les vulnérabilités de sécurité.

## Documentation et support

Pour une documentation détaillée, des guides d'installation et des FAQ, veuillez consulter la [Documentation de la PFDS](https://github.com/ssc-sp/datahub-docs).

Pour le support aux développeurs, vous pouvez ouvrir un problème dans ce dépôt ou contacter directement les mainteneurs.

Pour des informations générales sur la Plateforme fédérale de données scientifiques, visitez le [site Canada.ca](https://www.canada.ca/fr/services-partages/services/outils-pour-personnes-travaillant-gc/outils-scientifique/plateforme-federale-donnees-scientifiques.html). Les employés du gouvernement du Canada peuvent également visiter notre [site GCÉchange](https://gcxgce.sharepoint.com/teams/10002160/SitePages/Home.aspx) (accessible au sein du réseau GC).