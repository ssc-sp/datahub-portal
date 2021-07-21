# Steps to run in TST

## Prereq
1. Key vault datahub-key-env has been created under the desired Resource Group
1. Key vault access policy has been created for the user running terraform
1. Secrets have been created with the name "datahubportal-client-id" and "datahubportal-client-secret"
1. Terraform workspace has been created and switched
1. Environment specific variable file has been created (e.g. tst-variables.tf)
1. No other environment env-variables.tf exists in the terraform root folder

## Procedure for "Tst" environment

1. Login by running "bash dh-login-tst.sh"
1. Select workspace: terraform workspace select tst
1. Variable file: rm *-variables.tf; cp terraform.tfstate.d/tst/tst-variables.tf .
1. Import key vault: bash dh-import-keyvault.sh
1. View plan: terraform plan
1. Apply: terraform apply
1. Move variable file: mv tst-variables.tf terraform.tfstate.d/tst/tst-variables.tf

## Post Install Steps
1. Manually add App Service log: App Service > App Service Logs > Storage Settings under Application Logging (Blob) > Pick the storage container
1. Repeat the above step for Web server logging