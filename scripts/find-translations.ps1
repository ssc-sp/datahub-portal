# A short script to find all the missing translations in the project

# Get all the translation files

$translations = Get-ChildItem -Path ..\Portal\src\Datahub.Portal\i18n\ -Filter *.json -Recurse