@IWebHostEnvironment
Feature: Localizer
  For internationalization, we use a localizer to retrieve strings that are displayed to users in English or French

  Scenario: Strings should be retrieved in the appropriate language
  Given a text field with localized content for <key> from <file>
  When a user views the text in <language>
  Then the user should see localized text <expectedoutput>
  Examples:
    | key | language | expectedoutput | file |
    | MainTitle | en | Federal Science DataHub | localization.json |
    | MainTitle | fr | Plateforme fédérale de données scientifiques | localization.fr.json |
    | Usage | en | Usage | cbr.json |
    | Usage | fr | Utilisation | cbr.fr.json |
    | Enable ML | en | Enable ML | databricks.json |
    | Enable ML | fr | Activer l'apprentissage machine | databricks.fr.json |
    | Parole Board of Canada | en | Parole Board of Canada | depts.json |
    | Parole Board of Canada | fr | Commission des libérations conditionnelles du Canada | depts.fr.json |
    | Lead Email | en | Lead Email | gchosting.json |
    | Lead Email | fr | Courriel du responsable | gchosting.fr.json |
    | /locked-lt | en | /verrouille | url.json |
    | /verrouille-lt | fr | /locked | url.fr.json |
    | https://documentation.sds.canada.ca/ | en | https://documentation.sds.canada.ca/en/ | url.json |
    | https://documentation.sds.canada.ca/ | fr | https://documentation.sds.canada.ca/fr/ | url.fr.json |
    | Current FSDH Version | en | Current FSDH Version | workspace_versions.json |
    | Current FSDH Version | fr | Version actuelle de la PFDS | workspace_versions.fr.json |
