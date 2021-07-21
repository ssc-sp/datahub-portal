# Terraform does
1. Create the "datahub" file system with other permission (--x) and default (--x)
2. Create the folder "NRCan-RNCan.gc.ca" and set permission to other (r-x) and default (--x)

# Application Code
1. The user folder (e.g. simon.wang) is created, set permission to other --x and default --x
1. When a new file is created, set permission to other --- and default ---
1. When share files for view only, set sharee's permission to r-x
1. When share files for write, set sharee's permission to rwx
1. Do not allow sharees to delete