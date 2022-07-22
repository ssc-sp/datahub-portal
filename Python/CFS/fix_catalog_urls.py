import csv 

with open('./data/catalog_urls.csv', newline='') as csvfile:
    rows = csv.reader(csvfile, delimiter=',', quotechar='|')
    for r in rows:
        id = r[0]
        en_url = r[1]
        fr_url = en_url.replace("/en/", "/fr/")
        print(f"update CatalogObjects set Url_English_TXT = '{en_url}', Url_French_TXT = '{fr_url}' where CatalogObjectId = {id}")

