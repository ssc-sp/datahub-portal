from metadata_loader import loadMetadata
import pyodbc

import yaml
from yaml.loader import SafeLoader

import requests

def _seedDatabase(defs, connectionString, commit = True):
    '''seed the database with the schema definitions'''

    db = pyodbc.connect(connectionString)

    cursor = db.cursor()

    order = 1
    for d in defs:
        if d.fieldName:
            fieldId = _addDefinitionToDatabase(d, order, cursor)
            order = order + 1

            for c in d.choices:
                _addFieldChoice(c, fieldId, cursor)

    if commit:
            db.commit()

    db.close()


def _addDefinitionToDatabase(d, order, db):
    query = '''
        INSERT INTO FieldDefinition (FieldName, SortOrder, NameEnglish, NameFrench, DescriptionEnglish, DescriptionFrench, Required, Source) 
        VALUES ('%s', %s, '%s', '%s', '%s', '%s', %s, 0)'''%(
            _escape(d.fieldName), 
            order, 
            _escape(d.nameEnglish), 
            _escape(d.nameFrench), 
            _escape(d.descriptionEnglish), 
            _escape(d.descriptionFrench), 
            _toInt(d.required))

    db.execute(query)
    db.execute('SELECT @@IDENTITY')

    id = db.fetchone()[0]

    return id


def _addFieldChoice(choice, fieldId, db):
    query = '''INSERT INTO FieldChoice (FieldDefinitionId, Value, LabelEnglish, LabelFrench) VALUES (%s, '%s', '%s', '%s')'''%(
        fieldId, 
        _escape(choice.value),
        _escape(choice.labelEnglish),
        _escape(choice.labelFrench))

    db.execute(query)


def _escape(s):
    return s.replace("'", "''")


def _toInt(f):
    return 1 if f else 0


def _loadYamlFile(path):
    '''loads a yaml field into a dictionary'''

    with open(path, encoding="utf-8") as f:
        data = yaml.load(f, Loader=SafeLoader)
        return data

def _readConnectionString(path):
    with open(path, 'r') as f:
        return f.read()

def _downloadYamlFile(url):
    r = requests.get(url, verify = False)
    return yaml.load(r.content, Loader=SafeLoader)
     

#seeding begin

urbBase = "https://raw.githubusercontent.com/open-data/ckanext-canada/master/ckanext/canada/schemas/%s.yaml"
datasetUrl = urbBase%("dataset")
presetsUrl = urbBase%("presets")

#datasetData = _loadYamlFile("dataset.yaml")
#presetsData = _loadYamlFile("presets.yaml")

datasetData = _downloadYamlFile(datasetUrl)
presetsData = _downloadYamlFile(presetsUrl)

defs = loadMetadata(datasetData, presetsData)

connectionString = _readConnectionString('.connectionstring')

#_seedDatabase(defs, connectionString)

for d in defs:
    print(d.nameFrench)

#seeding end