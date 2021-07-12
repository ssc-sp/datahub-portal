import yaml
from yaml.loader import SafeLoader

from metadata_loader import loadMetadata

import unittest

# This is a generalized example, not specific to a test framework
class Test_MetadataLoader(unittest.TestCase):

    def test_validator_valid_string(self):
        # The exact assertion call depends on the framework as well
        assert(True, True)


    def test_loading_simple_definition(self):
        defs = loadMetadata(_loadTestData('simple_field_schema.yaml'), _loadTestData('simple_field_presets.yaml'))

        assert(len(defs), 1)

        target = defs[0]

        assert(target.nameEnglish, "Collection Type")
        assert(target.nameFrench, "Type de collection")


def _loadTestData(path):
    with open("./test_data/%s"%(path), 'r') as f:
        return yaml.load(f.read(), Loader=SafeLoader)
