# ingest the catalog

import csv
import json
from dataclasses import dataclass
from typing import Callable

class Entity:
   def __init__(self, row, header):
        self.__dict__ = dict(zip(header, row))

@dataclass
class Subject:
    id: str
    name_en: str
    name_fr: str

@dataclass
class CatalogObject:
    id: str	
    name_en: str
    name_fr: str
    desc_en: str
    desc_fr: str
    contact: str
    subjects: list[Subject]	
    programs: str
    keywords_en: list[str]
    keywords_fr: list[str]
    url_en: str
    url_fr: str

def map_path(fileName: str) -> str:
    return f"./data/{fileName}"

def load_table(path: str) -> list[Entity]:
    data = list(csv.reader(open(map_path(path), newline='', encoding="UTF8")))    
    rows = [Entity(i, data[0]) for i in data[1:]]        
    return rows

def load_thresaurus(lang: str) -> dict:
    data = load_table(f'thesaurus-keywords-{lang}.csv')
    thresaurus = dict()
    for row in data:
        thresaurus[row.tema_id] = row.tema
    return thresaurus

def index_table(table: list[Entity], key: str) -> dict:
    index = dict()
    for row in table:
        index[getattr(row, key, "")] = row
    return index

def build_url(lang: str, id: str) -> str:
    return f"https://applications.cfl.scf.rncan.gc.ca/ids/{lang}/data/view/{id}"

# load all tables

datasheets = load_table('datasheet.csv')
subjects = index_table(load_table('datasubjects.csv'), "id")
ds_subjects = load_table('datasheet_subjects.csv')
ds_keywords = load_table('datasheet_keywords.csv')
threasaurus_en = load_thresaurus("en")
threasaurus_fr = load_thresaurus("fr")

def get_subjects(id: str) -> list[Subject]:
    joined = filter(lambda e: e.datID == id, ds_subjects)
    mapped = map(lambda e: map_to_subject(subjects[e.subject_id]), joined)
    return list(mapped)

def get_datasheet_keywords(id: str) -> list[Entity]:
    joined = filter(lambda e: e.datID == id, ds_keywords)
    return list(joined)

def get_keywords(ids: list, threasaurus: dict) -> list[str]:
    valid_ids = list(filter(lambda e: e != "", ids))
    return list(map(lambda id: threasaurus[id], valid_ids))    

def map_to_subject(e: Entity) -> dict:
    #return Subject(id=e.id, name_en=e.name_en, name_fr=e.name_fr)    
    return { "id": e.id, "name_en": e.name_en, "name_fr": e.name_fr }

def filter_pas_disponible(text: str) -> str:
    return text if text != "pas disponible" else ""

output = list()
for e in datasheets:
    keyword_ids = get_datasheet_keywords(e.id)
    ds = CatalogObject(
        id=f"CFS_ID_{e.id}", 
        name_en=e.name_en, 
        name_fr=filter_pas_disponible(e.name_fr),
        desc_en=e.datDescEN,
        desc_fr=filter_pas_disponible(e.datDesc),
        contact=e.datLeadContact,
        subjects=get_subjects(e.id),
        programs=e.datRelProg,
        keywords_en=get_keywords(list(map(lambda kw: kw.keyword_id_en, keyword_ids)), threasaurus_en),
        keywords_fr=get_keywords(list(map(lambda kw: kw.keyword_id_fr, keyword_ids)), threasaurus_fr),
        url_en=build_url("en", e.id),
        url_fr=build_url("fr", e.id))
    output.append(ds)
    update_row = f"update CatalogObjects set Url_English_TXT='{ds.url_en}', Url_French_TXT='{ds.url_fr}' where "

# output 
with open("cfs_catalog_ex.json", 'w') as f:
    json.dump([o.__dict__ for o in output], f, indent=4)
    
# Closing file
f.close()
