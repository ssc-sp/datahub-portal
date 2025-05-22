# Changelog

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
