using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ScriptureMemorizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "LDS Scripture Memorizer";

            string jsonPath = "lds_scriptures_full.json";
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"ERROR: {jsonPath} not found. Run the Python builder first.");
                return;
            }

            var jsonText = File.ReadAllText(jsonPath);
            Dictionary<string, string> db;
            try
            {
                db = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse JSON: " + ex.Message);
                return;
            }

            var rnd = new Random();
            var keys = new List<string>(db.Keys);
            string key = keys[rnd.Next(keys.Count)];
            string text = db[key];

            Reference reference = Reference.Parse(key);
            Scripture scripture = new Scripture(reference, text);

            int hideCount = 3;
            while (true)
            {
                Console.Clear();
                Console.WriteLine(scripture.GetDisplayText());

                if (scripture.AllWordsHidden())
                {
                    Console.WriteLine("\nAll words hidden. Program finished.");
                    break;
                }

                Console.WriteLine("\nPress ENTER to hide words or type 'quit' to exit.");
                Console.Write("> ");
                var input = Console.ReadLine().Trim().ToLower();
                if (input == "quit") break;

                scripture.HideRandomWords(hideCount);
                hideCount = Math.Min(10, hideCount + 1); // increase difficulty gradually
            }
        }
    }

    class Scripture
    {
        private Reference _reference;
        private List<Word> _words;

        public Scripture(Reference reference, string text)
        {
            _reference = reference;
            _words = new List<Word>();
            foreach (var w in text.Split(' '))
            {
                _words.Add(new Word(w));
            }
        }

        public void HideRandomWords(int count)
        {
            var rnd = new Random();
            int hidden = 0;
            for (int i = 0; i < count; i++)
            {
                var candidates = _words.FindAll(w => !w.Hidden);
                if (candidates.Count == 0) break;
                var word = candidates[rnd.Next(candidates.Count)];
                word.Hide();
                hidden++;
            }
        }

        public bool AllWordsHidden()
        {
            foreach (var w in _words)
            {
                if (!w.Hidden) return false;
            }
            return true;
        }

        public string GetDisplayText()
        {
            string display = _reference.ToString() + "\n";
            foreach (var w in _words)
            {
                display += (w.Hidden ? w.Masked : w.Text) + " ";
            }
            return display.Trim();
        }
    }

    class Reference
    {
        public string Book { get; private set; }
        public int Chapter { get; private set; }
        public int VerseStart { get; private set; }
        public int VerseEnd { get; private set; }

        public Reference(string book, int chapter, int verseStart, int verseEnd = -1)
        {
            Book = book;
            Chapter = chapter;
            VerseStart = verseStart;
            VerseEnd = verseEnd < 0 ? verseStart : verseEnd;
        }

        public static Reference Parse(string key)
        {
            var parts = key.Split(' ', 2);
            string book = parts[0];
            string remainder = parts.Length > 1 ? parts[1] : "1:1";

            if (remainder.Contains("-"))
            {
                var chap = int.Parse(remainder.Split(':')[0]);
                var verses = remainder.Split(':')[1].Split('-');
                int start = int.Parse(verses[0]);
                int end = int.Parse(verses[1]);
                return new Reference(book, chap, start, end);
            }
            else
            {
                var chap = int.Parse(remainder.Split(':')[0]);
                var verse = int.Parse(remainder.Split(':')[1]);
                return new Reference(book, chap, verse);
            }
        }

        public override string ToString()
        {
            return VerseStart == VerseEnd ? $"{Book} {Chapter}:{VerseStart}" : $"{Book} {Chapter}:{VerseStart}-{VerseEnd}";
        }
    }

    class Word
    {
        public string Text { get; private set; }
        public bool Hidden { get; private set; }

        public Word(string text)
        {
            Text = text;
            Hidden = false;
        }

        public void Hide()
        {
            Hidden = true;
        }

        public string Masked => new string('_', Text.Length);
    }
}
