Patrick Little - product owner Open Data portal

1. Open source - Metadata schema published on github.
  - Based on ISO 1995 Harmonized
  - Commonly used schema
  - https://github.com/open-data/ckanext-canada/blob/master/ckanext/canada/schemas/dataset.yaml
1. Frequency of updates
  - Core schema - almost never changes
  - Controlled lists - come from other sources
     - Subject area comes from library and archives
     - Country applicable - updated in real time
1. CKAN Platform
  - API - straightforward to use
  - https://docs.ckan.org/en/2.8/api/index.html
  - Token/secret - authentication. Two instances of CKAN running
     - open.canada.ca/data -> instance where public getting data
     - second version registry.open.canada.ca -> depts go to publish data. Manage data sets. This platform would be used to submit. API Key in profile

Staging environment. Prod accounts give you access. Credential can be copied from prod to staging environment. Copy cred to staging.