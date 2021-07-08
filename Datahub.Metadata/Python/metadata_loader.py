class FieldDefinition:
    '''Field definition entity'''
    
    def __init__(self):
        self.fieldName = ""
        self.preset = ""
        self.nameEnglish = ""
        self.nameFrench = ""
        self.descriptionEnglish = ""
        self.descriptionFrench = ""
        self.required = False
        self.choices = []


class FieldChoice:
    '''Field choice entity'''

    def __init__(self):
        self.value = ""
        self.labelEnglish = ""
        self.labelFrench = ""


def loadMetadata(schemaData, presetsData):
    '''loads the metadata from schema and presets'''    

    presetHash = _hashPresets(_getValue(presetsData, 'presets', []))
        
    existing = dict()

    defs = []
    for f in _getValue(schemaData, 'dataset_fields'):
        for fd in _readSchemaField(f, presetHash):
            if not(fd.fieldName in existing):
                existing[fd.fieldName] = fd
                defs.append(fd)

    return defs  


def _getValue(data, path, default = ""):
    '''reads a value from a dictionary-like structure given its path'''

    sp = path.split('/')
    value = data
    for p in sp:
        if isinstance(value, dict):
            if p in value:
                value = value[p]
            else:
                return default
    return value


def _convertFieldChoice(data):
    '''reads a field choice into a FieldChoice instnace'''

    choice = FieldChoice()

    value = _getValue(data, 'value')
    choice.value = value
    choice.labelEnglish = _getValue(data, 'label/en', value)
    choice.labelFrench = _getValue(data, 'label/fr', value)

    return choice


def _convertFieldChoices(dataChoices, options):
    '''reads the field choices if exist'''
    
    choices = []
    for c in dataChoices:
        choice = _convertFieldChoice(c)
        if (options == None) or (choice.value in options):
            choices.append(choice)

    return choices


def _isFluentDefinition(preset):
    '''tells whether a definition is fluent or not'''    

    return _getValue(preset, 'values/fluent_form_label', None) != None 


def _readFluentFieldDefinition(preset, choices, lang):
    '''reads a fluent definition given the language'''
    
    d = FieldDefinition()

    d.fieldName = '%s_%s'%(_getValue(preset, 'values/field_name'), lang)
    d.preset = _getValue(preset, 'preset_name')
    
    d.nameEnglish = _getValue(preset, 'values/fluent_form_label/%s/en'%(lang))
    d.nameFrench  = _getValue(preset, 'values/fluent_form_label/%s/fr'%(lang))

    d.descriptionEnglish = _getValue(preset, 'values/fluent_help_text/%s/en'%(lang))
    d.descriptionFrench  = _getValue(preset, 'values/fluent_help_text/%s/fr'%(lang))

    d.required = _getValue(preset, 'values/required') == 'true'

    d.choices = _convertFieldChoices(_getValue(preset, 'values/choices', []), choices)

    return d


def _readNonFluentFieldDefinition(preset, choices):
    '''reads a non-fluent definition'''

    d = FieldDefinition()

    d.fieldName = _getValue(preset, 'values/field_name')
    d.preset = _getValue(preset, 'preset_name')

    d.nameEnglish = _getValue(preset, 'values/label/en')
    d.nameFrench = _getValue(preset, 'values/label/fr')

    d.descriptionEnglish = _getValue(preset, 'values/help_text/en')
    d.descriptionFrench = _getValue(preset, 'values/help_text/fr')

    d.required = _getValue(preset, 'values/required') == 'true'

    d.choices = _convertFieldChoices(_getValue(preset, 'values/choices', []), choices)
    
    return d


def _readFieldDefinitionSet(preset, choices):
    '''parses a preset and returns one or two definitions depending on the nature of the field'''

    defs = []

    if _isFluentDefinition(preset):
        defs.append(_readFluentFieldDefinition(preset, choices, 'en'))
        defs.append(_readFluentFieldDefinition(preset, choices, 'fr'))
    else:
        defs.append(_readNonFluentFieldDefinition(preset, choices))

    return defs


def _hashPresets(presets):
    '''hashed the preset using the preset name as a key'''

    hash = dict()
    for p in presets:
        hash[p['preset_name']] = p

    return hash


def _readSchemaFieldDefinition(data):
    '''reads a schema field definition'''
    d = FieldDefinition()
    
    d.fieldName = _getValue(data, 'field_name')
    d.nameEnglish = _getValue(data, 'label/en')
    d.nameFrench = _getValue(data, 'label/fr')

    d.choices = _convertFieldChoices(_getValue(data, 'choices'), None)

    return d


def _readSchemaField(data, presetHash):
    '''reads a schema field (or fields) from the preset or from the schema definition'''

    presetId = _getValue(data, 'preset', None)
    if presetId in presetHash:
        return _readFieldDefinitionSet(presetHash[presetId], _getValue(data, 'form_restrict_choices_to', None))
    else:
        return [_readSchemaFieldDefinition(data)]