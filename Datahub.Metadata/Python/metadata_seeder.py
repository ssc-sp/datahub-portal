from io import StringIO
from metadata_loader import loadMetadata
import pyodbc

import yaml
from yaml.loader import SafeLoader

import requests
import hashlib

from datetime import datetime

def _seedDatabase(connectionString, chechsum, definitions, commit = True):
    '''seed the database with the schema definitions'''

    db = pyodbc.connect(connectionString)
    cursor = db.cursor()

    versionId = _addVersionData(cursor, checksum)

    order = 1
    for definition in definitions:
        if definition.fieldName:
            fieldId = _addDefinitionToDatabase(cursor, definition, versionId, order)
            order = order + 1

            for c in definition.choices:
                _addFieldChoice(cursor, fieldId, c)

    if commit:
            db.commit()

    db.close()

def _executeQueryAndGetIdentity(cursor, query):
    '''executes a query and returns the new row identity'''
    
    cursor.execute(query)
    cursor.execute('SELECT @@IDENTITY')

    return cursor.fetchone()[0]

def _addVersionData(cursor, versionData):
    '''add the version record for this load'''
    
    source = 'OpenData' #Open Metadata source
    lastUpdate = datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S.%f")

    query = '''
        INSERT INTO MetadataVersions (Source_TXT, Last_Update_DT, Version_Info_TXT) 
        VALUES (%s, '%s', '%s')'''%(source, lastUpdate, versionData)

    return _executeQueryAndGetIdentity(cursor, query)


def _addDefinitionToDatabase(cursor, definition, versionId, order):
    '''add a field definition and return the new row id'''

    query = '''
        INSERT INTO FieldDefinitions (MetadataVersionId, Field_Name_TXT, Sort_Order_NUM, Name_English_TXT, Name_French_TXT, English_DESC, French_DESC, Required_FLAG, MultiSelect_FLAG) 
        VALUES (%s, '%s', %s, '%s', '%s', '%s', '%s', %s, %s)'''%(
            versionId,
            _escape(definition.fieldName), 
            order, 
            _escape(definition.nameEnglish), 
            _escape(definition.nameFrench), 
            _escape(definition.descriptionEnglish), 
            _escape(definition.descriptionFrench), 
            _boolToInt(definition.required),
            _boolToInt(definition.multiselect))

    return _executeQueryAndGetIdentity(cursor, query)


def _addFieldChoice(cursor, fieldId, choice):
    '''adds a choice record to the db'''

    query = '''INSERT INTO FieldChoices (FieldDefinitionId, Value_TXT, Label_English_TXT, Label_French_TXT) VALUES (%s, '%s', '%s', '%s')'''%(
        fieldId, 
        _escape(choice.value),
        _escape(choice.labelEnglish),
        _escape(choice.labelFrench))

    cursor.execute(query)


def _escape(s):
    return s.replace("'", "''")


def _boolToInt(f):
    return 1 if f else 0


def _readConnectionString(path):
    with open(path, 'r') as f:
        return f.read()


def _downloadFileContent(url):
    '''download and decodes a file content given a url'''

    r = requests.get(url, verify = False)
    return r.content.decode() #return yaml.load(r.content, Loader=SafeLoader)


def _parseYamlData(data):
    return yaml.load(data, Loader=SafeLoader)


def _calcCheckSum(data):
    '''calculates a SHA256 checksum from the provided string'''
    sha256 = hashlib.sha256(data.encode('utf-8'))
    return sha256.hexdigest() 


#seeding begin

urbBase = "https://raw.githubusercontent.com/open-data/ckanext-canada/master/ckanext/canada/schemas/%s.yaml"
datasetUrl = urbBase%("dataset")
presetsUrl = urbBase%("presets")

#datasetData = _loadYamlFile("dataset.yaml")
#presetsData = _loadYamlFile("presets.yaml")

datasetData = _downloadFileContent(datasetUrl)
presetsData = _downloadFileContent(presetsUrl)

checksum = _calcCheckSum(_calcCheckSum(datasetData) + _calcCheckSum(presetsData))

definitions = loadMetadata(_parseYamlData(datasetData), _parseYamlData(presetsData))

connectionString = _readConnectionString('/temp/.connectionstring')

_seedDatabase(connectionString, checksum, definitions)

# just for fun print the french names to verify the text encoding is right, check for accents
for d in definitions:
    print(d.nameFrench)

#seeding end