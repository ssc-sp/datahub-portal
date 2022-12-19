import csv

def _escape(s):
    return s.replace("'", "''")

subjects_file = 'IDS - datasubjects.csv'

print(f'-- file: {subjects_file}')

print('SET IDENTITY_INSERT Subjects ON')

with open(subjects_file, mode='r', encoding='utf-8') as csv_file:
    csv_reader = csv.DictReader(csv_file)
    for row in csv_reader:
        line = '''INSERT INTO Subjects(SubjectId, Subject_TXT) VALUES (%s, '%s')'''%(row["id"], row["acronym"])
        print(line)

print('SET IDENTITY_INSERT Subjects OFF')

subsubjects_file = 'IDS - datasubsubjects.csv'

print(f'-- file: {subsubjects_file}')

print('SET IDENTITY_INSERT SubSubjects ON')

with open(subsubjects_file, mode='r', encoding='utf-8') as csv_file:
    csv_reader = csv.DictReader(csv_file)
    for row in csv_reader:
        line = '''INSERT INTO SubSubjects(SubSubjectId, Name_English_TXT, Name_French_TXT) VALUES (%s, '%s', '%s')'''%(row["id"], _escape(row["name_en"]), _escape(row["name_fr"]))
        print(line)

print('SET IDENTITY_INSERT SubSubjects OFF')

join_subjects_file = 'IDS - datasubjects_subsubjects.csv'

print(f'-- file: {join_subjects_file}')

print('--SET IDENTITY_INSERT SubSubjectSubject ON')

with open(join_subjects_file, mode='r') as csv_file:
    csv_reader = csv.DictReader(csv_file)
    for row in csv_reader:
        line = '''INSERT INTO SubSubjectSubject(SubjectsSubjectId, SubSubjectsSubSubjectId) VALUES (%s, %s)'''%(row["subject_id"], row["subsubject_id"])
        print(line)

print('--SET IDENTITY_INSERT SubSubjectSubject OFF')
