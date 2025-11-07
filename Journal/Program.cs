using System;
using System.Collections.Generic;
using System.IO;

class JournalEntry
{
    public string Date { get; set; }
    public string Prompt { get; set; }
    public string Response { get; set; }

    public override string ToString()
    {
        return $"Date: {Date}\nPrompt: {Prompt}\nResponse: {Response}\n";
    }
}

class Program
{
    static void Main()
    {
        List<JournalEntry> journalEntries = new List<JournalEntry>();
        Random random = new Random();

        List<string> prompts = new List<string>()
        {
            "What are you grateful for today?",
            "Describe a challenge you overcame recently.",
            "What is one goal you want to achieve this week?",
            "Write about a person who inspires you.",
            "What made you smile today?"
        };

        bool running = true;

        while (running)
        {
            Console.WriteLine("\n--- Personal Journal ---");
            Console.WriteLine("1. Write a new journal entry");
            Console.WriteLine("2. Display all journal entries");
            Console.WriteLine("3. Save the journal to a file");
            Console.WriteLine("4. Load the journal from a file");
            Console.WriteLine("5. Exit");
            Console.Write("Choose an option (1-5): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // Write a new entry
                    string prompt = prompts[random.Next(prompts.Count)];
                    Console.WriteLine($"\nPrompt: {prompt}");
                    Console.Write("Your response: ");
                    string response = Console.ReadLine();

                    JournalEntry newEntry = new JournalEntry
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Prompt = prompt,
                        Response = response
                    };

                    journalEntries.Add(newEntry);
                    Console.WriteLine("✅ Entry added successfully!");
                    break;

                case "2":
                    // Display all entries
                    if (journalEntries.Count == 0)
                        Console.WriteLine("No entries yet.");
                    else
                        foreach (var entry in journalEntries)
                            Console.WriteLine(entry);
                    break;

                case "3":
                    // Save journal
                    Console.Write("Enter filename to save (e.g., myjournal.csv): ");
                    string saveFile = Console.ReadLine();
                    SaveToCsv(saveFile, journalEntries);
                    Console.WriteLine($"Journal saved to {saveFile}");
                    break;

                case "4":
                    // Load journal
                    Console.Write("Enter filename to load (e.g., myjournal.csv): ");
                    string loadFile = Console.ReadLine();
                    if (File.Exists(loadFile))
                    {
                        journalEntries = LoadFromCsv(loadFile);
                        Console.WriteLine($"Journal loaded from {loadFile} (replaced current entries).");
                    }
                    else
                    {
                        Console.WriteLine("File not found.");
                    }
                    break;

                case "5":
                    running = false;
                    Console.WriteLine("Exiting. Goodbye!");
                    break;

                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }

    // Save entries to CSV file
    static void SaveToCsv(string filename, List<JournalEntry> entries)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            writer.WriteLine("Date,Prompt,Response"); // Header
            foreach (var entry in entries)
            {
                string date = $"\"{entry.Date}\"";
                string prompt = $"\"{entry.Prompt.Replace("\"", "\"\"")}\"";   // Escape quotes
                string response = $"\"{entry.Response.Replace("\"", "\"\"")}\"";
                writer.WriteLine($"{date},{prompt},{response}");
            }
        }
    }

    // Load entries from CSV file
    static List<JournalEntry> LoadFromCsv(string filename)
    {
        List<JournalEntry> entries = new List<JournalEntry>();
        string[] lines = File.ReadAllLines(filename);

        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            string line = lines[i];
            string[] parts = ParseCsvLine(line);
            if (parts.Length == 3)
            {
                entries.Add(new JournalEntry
                {
                    Date = parts[0],
                    Prompt = parts[1],
                    Response = parts[2]
                });
            }
        }

        return entries;
    }

    // Helper to handle commas inside quotes
    static string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        bool insideQuotes = false;
        string currentField = "";

        foreach (char c in line)
        {
            if (c == '\"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        fields.Add(currentField);
        return fields.ToArray();
    }
}
