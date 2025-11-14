#!/usr/bin/env python3
"""
build_lds_json.py
Downloads public-domain scripture texts and builds lds_scriptures_full.json

IMPORTANT:
 - This script attempts to download public-domain sources only.
 - It extracts raw verse text and avoids copying church editorial matter (headings/footnotes).
 - Review outputs before sharing publicly to ensure no copyrighted study aids were included.

Usage:
  python3 build_lds_json.py
Outputs:
  lds_scriptures_full.json
"""

import json
import re
import sys
from pathlib import Path
from urllib.parse import urljoin
import requests
from bs4 import BeautifulSoup

OUT_FILE = Path("lds_scriptures_full.json")

def fetch_text(url):
    print("Fetching:", url)
    r = requests.get(url, timeout=30)
    r.raise_for_status()
    return r.text

def parse_kjv_from_gutenberg():
    """
    Download KJV from Project Gutenberg (public domain) and extract verses.
    Project Gutenberg KJV example: https://www.gutenberg.org/ebooks/10
    The exact text extraction may need minor tuning depending on source formatting.
    """
    print("Parsing KJV (Project Gutenberg)...")
    # Use Gutenberg plain text mirror link for the King James Version (example id 10)
    base = "https://www.gutenberg.org/cache/epub/10/pg10.txt"
    txt = fetch_text(base)

    verses = {}
    # Very simple extraction approach: find lines that look like "Chapter N" and verse numbers.
    # KJV text format varies; this is a best-effort parser. After running, please spot-check.
    current_book = None
    current_chap = None

    # A crude split into lines
    for line in txt.splitlines():
        line = line.strip()
        if not line:
            continue

        # Detect book headings: many Gutenberg KJV texts include ALL CAPS book names
        # e.g., "THE FIRST BOOK OF MOSES: CALLED GENESIS"
        mbook = re.match(r'^(THE|THE BOOK OF|BOOK OF|THE FIRST BOOK OF|THE SECOND BOOK OF)\s+(.+)$', line, re.I)
        if mbook and len(line) < 100:
            # Simplify to last word(s)
            current_book = re.sub(r'[^A-Za-z0-9 ]', '', line).title()
            continue

        # Detect lines with verse numbers "1 In the beginning God..."
        m = re.match(r'^(\d+)\s+(.*)$', line)
        if m and current_book:
            # naive; treat as verse in current chapter if chapter known
            verse_num = m.group(1)
            verse_text = m.group(2).strip()
            if current_chap is None:
                # assume chapter 1 until we detect chapter headings
                current_chap = 1
            key = f"{current_book} {current_chap}:{verse_num}"
            verses[key] = verse_text
            continue

        # Detect chapter lines "CHAPTER I"
        mch = re.match(r'^(CHAPTER|Chapter)\s+([IVXLCDM0-9]+)\b', line)
        if mch:
            # convert roman numerals or digits
            num = mch.group(2)
            try:
                chap = int(num)
            except:
                # convert Roman -> int (basic)
                roman_map = {'I':1,'V':5,'X':10,'L':50,'C':100,'D':500,'M':1000}
                total = 0
                for ch in num.upper():
                    total += roman_map.get(ch,0)
                chap = total or None
            current_chap = chap
            continue

    print(f"Parsed {len(verses)} KJV verses (approx).")
    return verses

def fetch_book_of_mormon_from_gutenberg():
    print("Parsing Book of Mormon (Project Gutenberg)...")
    # Gutenberg has Book of Mormon: e.g., id 17 plain text
    url = "https://www.gutenberg.org/cache/epub/17/pg17.txt"
    txt = fetch_text(url)

    verses = {}
    current_book = None
    current_chap = None
    for line in txt.splitlines():
        line = line.strip()
        if not line:
            continue
        # Book headings often appear like "FIRST BOOK OF NEPHI"
        if line.isupper() and len(line) < 60:
            # reduce
            current_book = re.sub(r'[^A-Za-z0-9 ]', '', line).title()
            continue

        # Lines like "1:1" or "1:1 And it came to pass..."
        m = re.match(r'^(\d+):(\d+)\s+(.*)$', line)
        if m and current_book:
            chap = int(m.group(1))
            verse = int(m.group(2))
            text = m.group(3).strip()
            key = f"{current_book} {chap}:{verse}"
            verses[key] = text
            continue

        # Alternate formats can exist; naive approach above captures many verses.
    print(f"Parsed {len(verses)} Book of Mormon verses (approx).")
    return verses

def fetch_dandc_and_pearl_from_official():
    """
    The Church of Jesus Christ hosts scripture HTML pages.
    We will selectively fetch Doctrine & Covenants and Pearl of Great Price pages,
    parse the verse text elements, and add them. This avoids copying church editorial content.
    """
    print("Fetching Doctrine & Covenants and Pearl of Great Price (official site)...")
    base = "https://www.churchofjesuschrist.org/study/scriptures"
    # For simplicity fetch D&C index page and poematically fetch sections.
    # NOTE: this step depends on site structure and might need updates.
    # We'll attempt a few common pages as example; for a full run you'd enumerate all sections.
    pages = [
        "dc/4?lang=eng",
        "dc/6?lang=eng",
        "pgp/moses/1?lang=eng",
        "pgp/abr/3?lang=eng"
    ]
    verses = {}
    for p in pages:
        url = urljoin(base + "/", p)
        try:
            html = fetch_text(url)
            soup = BeautifulSoup(html, "html.parser")
            # On church site verses are inside <span class="verse"> or <p class="verse"> etc.
            for v in soup.select(".verse, span.verse, p.verse"):
                # verse number
                num_tag = v.select_one(".verse-number")
                num = num_tag.get_text(strip=True) if num_tag else None
                # verse text content
                text = v.get_text(" ", strip=True)
                # remove the leading verse number portion
                if num:
                    text = re.sub(r'^\s*' + re.escape(num), '', text).strip()
                # create a key using URL fragement as book/section
                # This is a simplistic key; for full program you'd map to canonical book names
                key = f"{p.split('?')[0].upper()} {num or ''}".strip()
                if key and text:
                    verses[key] = text
        except Exception as e:
            print("Failed to fetch", url, ":", e)
    print(f"Parsed {len(verses)} verses from sample D&C/PoGP pages.")
    return verses

def main():
    all_verses = {}
    # KJV
    try:
        all_verses.update(parse_kjv_from_gutenberg())
    except Exception as e:
        print("KJV parse failed:", e)
    # Book of Mormon
    try:
        all_verses.update(fetch_book_of_mormon_from_gutenberg())
    except Exception as e:
        print("Book of Mormon fetch failed:", e)
    # D&C and Pearl (sample pages)
    try:
        all_verses.update(fetch_dandc_and_pearl_from_official())
    except Exception as e:
        print("D&C/PGP fetch failed:", e)

    # Save JSON (warning: may be large)
    print(f"Writing {len(all_verses)} entries to {OUT_FILE}")
    with open(OUT_FILE, "w", encoding="utf-8") as f:
        json.dump(all_verses, f, indent=2, ensure_ascii=False)

    print("Done.")

if __name__ == "__main__":
    main()
