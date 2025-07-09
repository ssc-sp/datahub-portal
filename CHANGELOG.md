# Changelog

## [6.0.4](https://github.com/ssc-sp/datahub-portal/compare/v6.0.3...v6.0.4) (2025-07-09)


### Bug Fixes

* fix for updated db names ([#1695](https://github.com/ssc-sp/datahub-portal/issues/1695)) ([e29366e](https://github.com/ssc-sp/datahub-portal/commit/e29366e55f9ac4e78a9a1c199374037520f71c52))
* storage report bug when no usage data is available ([#1688](https://github.com/ssc-sp/datahub-portal/issues/1688)) ([94aad9d](https://github.com/ssc-sp/datahub-portal/commit/94aad9da2169b9238f754037f8e96a8afc909c68))

## [6.0.3](https://github.com/ssc-sp/datahub-portal/compare/v6.0.2...v6.0.3) (2025-07-08)


### Bug Fixes

* Fixing placement of announcement header on home page ([#1691](https://github.com/ssc-sp/datahub-portal/issues/1691)) ([816154c](https://github.com/ssc-sp/datahub-portal/commit/816154ca51ec5cb24cf34cd96313a0e11452ad69))

## [6.0.2](https://github.com/ssc-sp/datahub-portal/compare/v6.0.1...v6.0.2) (2025-07-07)


### Reverts

* Revert "fix: Improve announcement card on Home page to include button in article markup" ([#1689](https://github.com/ssc-sp/datahub-portal/issues/1689)) ([46a1ce8](https://github.com/ssc-sp/datahub-portal/commit/46a1ce8b4e9c7d5019620808bb9c97fea5ebc43c))

## [6.0.1](https://github.com/ssc-sp/datahub-portal/compare/v6.0.0...v6.0.1) (2025-07-07)


### Bug Fixes

* Improve announcement card on Home page to include button in article markup ([#1684](https://github.com/ssc-sp/datahub-portal/issues/1684)) ([b626945](https://github.com/ssc-sp/datahub-portal/commit/b62694546f54dcde9246ba024b7cb3f1d499cdb4))
* local time for healthcheck ([#1682](https://github.com/ssc-sp/datahub-portal/issues/1682)) ([882fcfd](https://github.com/ssc-sp/datahub-portal/commit/882fcfdc4d9e7f07ca4445dcd51b4b9633291d5d))
* Minor try/catch for better error message on costing refresh in production ([#1685](https://github.com/ssc-sp/datahub-portal/issues/1685)) ([7c408d6](https://github.com/ssc-sp/datahub-portal/commit/7c408d67e17dd9ff27eabc0db9edbac4edb0248a))

## [6.0.0](https://github.com/ssc-sp/datahub-portal/compare/v5.0.0...v6.0.0) (2025-06-24)


### ⚠ BREAKING CHANGES

* Database cleanup and documentation ([#1672](https://github.com/ssc-sp/datahub-portal/issues/1672))
* Migration fix and updated database schema and doc ([#1667](https://github.com/ssc-sp/datahub-portal/issues/1667))
* Spring cleanup ([#1664](https://github.com/ssc-sp/datahub-portal/issues/1664))

### Features

* Added 'access from databricks' link to workspace storage ([#1671](https://github.com/ssc-sp/datahub-portal/issues/1671)) ([2b7554b](https://github.com/ssc-sp/datahub-portal/commit/2b7554bdf421c359d32b4d77ccc2ceb5234384cf))
* Automating version updates for build (green light) changes ([#1670](https://github.com/ssc-sp/datahub-portal/issues/1670)) ([e5e7bcd](https://github.com/ssc-sp/datahub-portal/commit/e5e7bcdef570a6493314e84fcacfcf03d63093b9))
* CBR Budget Report ([#1668](https://github.com/ssc-sp/datahub-portal/issues/1668)) ([e1a8c73](https://github.com/ssc-sp/datahub-portal/commit/e1a8c73b484eec2798c076a1a14907d9b637d286))
* disable inactive users ([#1659](https://github.com/ssc-sp/datahub-portal/issues/1659)) ([4c22868](https://github.com/ssc-sp/datahub-portal/commit/4c22868a6ec9259b559f3832998084479e2fc658))
* In-portal statistics and reports ([#1638](https://github.com/ssc-sp/datahub-portal/issues/1638)) ([af049ce](https://github.com/ssc-sp/datahub-portal/commit/af049ce094382755d26287b2c73a92a8f2eb1328))
* updated code to handle group file delete operation ([#1666](https://github.com/ssc-sp/datahub-portal/issues/1666)) ([e6cb433](https://github.com/ssc-sp/datahub-portal/commit/e6cb43309db91b6042ff5917d10940ab80e71d9a))


### Bug Fixes

* Announcement section is now hidden on the home page when there's nothing to announce ([#1675](https://github.com/ssc-sp/datahub-portal/issues/1675)) ([a838407](https://github.com/ssc-sp/datahub-portal/commit/a8384071c8128c8fa4b0fe7447c45bd88b6e965c))
* Deleted workspaces and removed users no longer appear on the Explore page ([#1676](https://github.com/ssc-sp/datahub-portal/issues/1676)) ([bbe5be3](https://github.com/ssc-sp/datahub-portal/commit/bbe5be3ddfb874dc3eef457357365ad8ce16246a))
* Fix notification records not being saved, and logging ([#1654](https://github.com/ssc-sp/datahub-portal/issues/1654)) ([0529848](https://github.com/ssc-sp/datahub-portal/commit/0529848ca7f39007d4ae4be709b2b1ea98643352))
* Fixed an issue with the metadata editor ([#1674](https://github.com/ssc-sp/datahub-portal/issues/1674)) ([79b46a7](https://github.com/ssc-sp/datahub-portal/commit/79b46a74c32b29f2b6162d64dc8e7f506058c017))
* Migration fix and updated database schema and doc ([#1667](https://github.com/ssc-sp/datahub-portal/issues/1667)) ([96eaf30](https://github.com/ssc-sp/datahub-portal/commit/96eaf30c90912a5048a63f65e7abdf582868ca34))
* Minor fixes for non logged users ([#1663](https://github.com/ssc-sp/datahub-portal/issues/1663)) ([960d701](https://github.com/ssc-sp/datahub-portal/commit/960d701d24cb75f422a5cb0cd5235ea848f436d4))
* Removed the ability to set a user's role as disabled and removed the ability to set a new user as a removed user ([#1665](https://github.com/ssc-sp/datahub-portal/issues/1665)) ([e41594d](https://github.com/ssc-sp/datahub-portal/commit/e41594d3bf8e2a1d1d278e35ece00069e6e6ed0b))
* updated rules to allow only single workspace lead ([#1681](https://github.com/ssc-sp/datahub-portal/issues/1681)) ([2ae7e7c](https://github.com/ssc-sp/datahub-portal/commit/2ae7e7c5722fc4180aa66af7604b2f6b5fcc91b8))
* Updated the text on the register page to explain how to get started ([#1678](https://github.com/ssc-sp/datahub-portal/issues/1678)) ([a9a7d03](https://github.com/ssc-sp/datahub-portal/commit/a9a7d03ffab59cd3ace730e05af63665e53c133c))
* Website is missing a robots.txt file ([#1658](https://github.com/ssc-sp/datahub-portal/issues/1658)) ([84deccc](https://github.com/ssc-sp/datahub-portal/commit/84deccc56d7d856c34a7cecf5789dd70676ba62f))


### Miscellaneous Chores

* Database cleanup and documentation ([#1672](https://github.com/ssc-sp/datahub-portal/issues/1672)) ([4f36ab7](https://github.com/ssc-sp/datahub-portal/commit/4f36ab70fa3040ac694c4af55c55de6db5a23d4f))
* Spring cleanup ([#1664](https://github.com/ssc-sp/datahub-portal/issues/1664)) ([f0ca491](https://github.com/ssc-sp/datahub-portal/commit/f0ca49177f01b4f84477f53becfbbe44dbea6a3a))

## [5.0.0](https://github.com/ssc-sp/datahub-portal/compare/v4.0.10...v5.0.0) (2025-05-21)


### ⚠ BREAKING CHANGES

* Changed GC Hosting Controller output to dictionary to simplify oci integration ([#1648](https://github.com/ssc-sp/datahub-portal/issues/1648))
* Versioning and Redeployment of Deleted Resources ([#1633](https://github.com/ssc-sp/datahub-portal/issues/1633))

### Features

* Add to environment group at portal user creation ([#1639](https://github.com/ssc-sp/datahub-portal/issues/1639)) ([d4870ed](https://github.com/ssc-sp/datahub-portal/commit/d4870eda7c5a3ba30864a02cc934b912b7be690e))
* Add unit tests and validation to AddToGroupFunction ([#1643](https://github.com/ssc-sp/datahub-portal/issues/1643)) ([5edeb99](https://github.com/ssc-sp/datahub-portal/commit/5edeb99b124bda4a1a204f117ab131658a14bf23))
* CBR and workspace creation ([#1615](https://github.com/ssc-sp/datahub-portal/issues/1615)) ([e1c0388](https://github.com/ssc-sp/datahub-portal/commit/e1c038833990198f0a7d8842f93b894ccab7adf1))
* CBR Budget Management  ([#1640](https://github.com/ssc-sp/datahub-portal/issues/1640)) ([0ca640f](https://github.com/ssc-sp/datahub-portal/commit/0ca640ff150aa23b113167e1f109bcee2bf0aab6))
* Codifying the release migration develop to prod ([#1650](https://github.com/ssc-sp/datahub-portal/issues/1650)) ([ca67a4f](https://github.com/ssc-sp/datahub-portal/commit/ca67a4f886ddf004b78decd76e0463ef33fc9c2e))
* Convetional Commit Messages ([#1628](https://github.com/ssc-sp/datahub-portal/issues/1628)) ([5c8f7e7](https://github.com/ssc-sp/datahub-portal/commit/5c8f7e792d59184ca554876b690552a4edce9540))
* create page where admins can update/add versions ([#1655](https://github.com/ssc-sp/datahub-portal/issues/1655)) ([c3d877b](https://github.com/ssc-sp/datahub-portal/commit/c3d877bcab2376289ad0fd8445bebcc21a5a205f))
* new rule for workspaces lead limit and misc blazor improvements ([#1651](https://github.com/ssc-sp/datahub-portal/issues/1651)) ([012bb36](https://github.com/ssc-sp/datahub-portal/commit/012bb36358da2616bf3e8eb2f0caa5b557fcaaa2))
* replace Elemental with Mud components on metadata pages ([#1627](https://github.com/ssc-sp/datahub-portal/issues/1627)) ([6864179](https://github.com/ssc-sp/datahub-portal/commit/686417997d0e7e8a823eaeaa3e69b90287aa6a9c))
* select/de-select all files in File Explorer ([#1653](https://github.com/ssc-sp/datahub-portal/issues/1653)) ([b55f57b](https://github.com/ssc-sp/datahub-portal/commit/b55f57bf839d7daa7795e8940bd572fbb5d624cc))
* Versioning and Redeployment of Deleted Resources ([#1633](https://github.com/ssc-sp/datahub-portal/issues/1633)) ([f314282](https://github.com/ssc-sp/datahub-portal/commit/f3142825ad5abf15b33a001c00ab7c99b81d0a81))


### Bug Fixes

* added permission to create labels ([cb59872](https://github.com/ssc-sp/datahub-portal/commit/cb598726cae8772e569505a69e2e201b145f37f9))
* fixed usercard crashing when looking for missing json file ([#1647](https://github.com/ssc-sp/datahub-portal/issues/1647)) ([f79bfcf](https://github.com/ssc-sp/datahub-portal/commit/f79bfcf3906d72a83f58d8c056558cd26e75cc2f))
* Fixing issue where the "Added to Workspace" date is not reset after re-adding a user to the workspace ([#1631](https://github.com/ssc-sp/datahub-portal/issues/1631)) ([7c28823](https://github.com/ssc-sp/datahub-portal/commit/7c288230da716c6fbf626e09a05582e646fb5022))
* Resolving an exception on the home page when a user's language hasn't been selected yet ([#1652](https://github.com/ssc-sp/datahub-portal/issues/1652)) ([3430e9f](https://github.com/ssc-sp/datahub-portal/commit/3430e9f0becb373d2447ddc6c2a7bdbab381c9b7))
* resource provisioner failing ([#1656](https://github.com/ssc-sp/datahub-portal/issues/1656)) ([c2a63f9](https://github.com/ssc-sp/datahub-portal/commit/c2a63f9948ba2ad63a93b7932a670569279312eb))
* restore PR template ([352da74](https://github.com/ssc-sp/datahub-portal/commit/352da74e84871e9b02f9f21dcd88c76c31ccf7b6))
* robust handling of user time zones ([#1646](https://github.com/ssc-sp/datahub-portal/issues/1646)) ([bc45404](https://github.com/ssc-sp/datahub-portal/commit/bc45404c467e6da8372495a59eafd1f98996d6ba))
* terraform output handler ([#1657](https://github.com/ssc-sp/datahub-portal/issues/1657)) ([e5b060b](https://github.com/ssc-sp/datahub-portal/commit/e5b060be2dc0ce57584e391e123e73622d729c19))


### Code Refactoring

* Changed GC Hosting Controller output to dictionary to simplify oci integration ([#1648](https://github.com/ssc-sp/datahub-portal/issues/1648)) ([baa6334](https://github.com/ssc-sp/datahub-portal/commit/baa63347a6122520669848f03b65d74f167bce0e))
