import sqlite3
import os
import json

db_locations = [
    'kotoba.db',
    os.path.expandvars(r'%LOCALAPPDATA%\KotobaOdyssey\kotoba.db')
]

with open('scripts/crawled_light_novels.json', 'r', encoding='utf-8') as f:
    novels = json.load(f)

jlpt_map = {
    'N5': 0,
    'N4': 1,
    'N3': 2,
    'N2': 3,
    'N1': 4
}

for db_path in db_locations:
    if not os.path.exists(db_path):
        print(f"Skipping {db_path} (file not found)")
        continue

    print(f"\nProcessing database: {db_path}")
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    for item in novels:
        # Check if already exists by title
        cur.execute("SELECT Id FROM Novels WHERE Title = ? OR JapaneseTitle = ?", (item['title'], item['title']))
        row = cur.fetchone()

        level_enum = jlpt_map.get(item['level'], 2)

        if row:
            novel_id = row[0]
            print(f"Novel '{item['title']}' already exists with ID: {novel_id}")
        else:
            cur.execute("""
                INSERT INTO Novels (Title, JapaneseTitle, RomajiTitle, Author, Description, CoverImagePath, JlptLevel, Genre, IsFavorite, IsDemoData, CreatedAt)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, 0, 0, datetime('now'))
            """, (item['title'], item['title'], item['romaji'], item['author'], item['synopsis'], item['cover'], level_enum, item['genre']))
            novel_id = cur.lastrowid
            print(f"Inserted Novel '{item['title']}' with ID: {novel_id}")

        # Check chapter 1
        cur.execute("SELECT Id FROM Chapters WHERE NovelId = ? AND ChapterNumber = 1", (novel_id,))
        chap_row = cur.fetchone()
        word_count = len(item['chapterContent'])

        if chap_row:
            cur.execute("""
                UPDATE Chapters SET Title = ?, Content = ?, WordCount = ?
                WHERE Id = ?
            """, (item['chapterTitle'], item['chapterContent'], word_count, chap_row[0]))
            print(f"  -> Updated Chapter 1 for novel ID {novel_id}")
        else:
            cur.execute("""
                INSERT INTO Chapters (NovelId, ChapterNumber, Title, Content, WordCount, IsCompleted, LastReadPosition)
                VALUES (?, 1, ?, ?, ?, 0, 0)
            """, (novel_id, item['chapterTitle'], item['chapterContent'], word_count))
            print(f"  -> Inserted Chapter 1 for novel ID {novel_id}")

    conn.commit()
    conn.close()
    print(f"Finished seeding {len(novels)} novels into {db_path}!")

