// Program.cs
// Eternal Quest - CSE 210
// Author: (Your Name)
// Description: Console program to manage goals (Simple, Eternal, Checklist, Negative).
// Creative extras (documented here to exceed requirements):
//  - NegativeGoal type (penalties for bad habits).
//  - Level shown (score / 1000).
//  - Save/Load includes completion state for Simple and progress for Checklist.
//  - Clear encapsulation, inheritance and polymorphism used throughout.

using System;
using System.Collections.Generic;
using System.IO;

namespace EternalQuest
{
    // ==========================================
    // BASE CLASS
    // ==========================================
    abstract class Goal
    {
        private string _name;
        private string _description;
        protected int _points;

        protected Goal(string name, string description, int points)
        {
            _name = name;
            _description = description;
            _points = points;
        }

        public string GetName() => _name;
        public string GetDescription() => _description;

        // Returns points awarded (can be negative)
        public abstract int RecordEvent();

        // Whether the goal is complete (some are never complete)
        public abstract bool IsComplete();

        // Status string for display
        public abstract string GetStatus();

        // Save string (single-line) to persist. Derived classes implement format.
        public abstract string SaveData();

        // Factory helper to parse saved line and return a Goal instance.
        public static Goal LoadFromData(string line)
        {
            // Expected formats:
            // Simple|Name|Description|points|completed
            // Eternal|Name|Description|points
            // Checklist|Name|Description|points|target|bonus|completed
            // Negative|Name|Description|points
            string[] parts = line.Split('|');
            string type = parts[0];

            switch (type)
            {
                case "Simple":
                    {
                        string name = parts[1];
                        string desc = parts[2];
                        int points = int.Parse(parts[3]);
                        bool completed = bool.Parse(parts[4]);
                        var g = new SimpleGoal(name, desc, points);
                        if (completed) g.RecordEvent(); // mark completed
                        return g;
                    }
                case "Eternal":
                    {
                        string name = parts[1];
                        string desc = parts[2];
                        int points = int.Parse(parts[3]);
                        return new EternalGoal(name, desc, points);
                    }
                case "Checklist":
                    {
                        string name = parts[1];
                        string desc = parts[2];
                        int points = int.Parse(parts[3]);
                        int target = int.Parse(parts[4]);
                        int bonus = int.Parse(parts[5]);
                        int completed = int.Parse(parts[6]);
                        return new ChecklistGoal(name, desc, points, target, bonus, completed);
                    }
                case "Negative":
                    {
                        string name = parts[1];
                        string desc = parts[2];
                        int points = int.Parse(parts[3]);
                        return new NegativeGoal(name, desc, points);
                    }
                default:
                    throw new FormatException("Unknown goal type in save file: " + type);
            }
        }
    }

    // ==========================================
    // SIMPLE GOAL
    // ==========================================
    class SimpleGoal : Goal
    {
        private bool _completed = false;

        public SimpleGoal(string name, string desc, int points)
            : base(name, desc, points) { }

        public override int RecordEvent()
        {
            if (_completed) return 0;
            _completed = true;
            return _points;
        }

        public override bool IsComplete() => _completed;

        public override string GetStatus() => IsComplete() ? "[X]" : "[ ]";

        // include completion state so we can restore it on load
        public override string SaveData() => $"Simple|{GetName()}|{GetDescription()}|{_points}|{_completed}";
    }

    // ==========================================
    // ETERNAL GOAL (Never Ends)
    // ==========================================
    class EternalGoal : Goal
    {
        public EternalGoal(string name, string desc, int points)
            : base(name, desc, points) { }

        public override int RecordEvent() => _points;

        public override bool IsComplete() => false;

        public override string GetStatus() => "[∞]";

        public override string SaveData() => $"Eternal|{GetName()}|{GetDescription()}|{_points}";
    }

    // ==========================================
    // CHECKLIST GOAL
    // ==========================================
    class ChecklistGoal : Goal
    {
        private int _target;
        private int _completed;
        private int _bonus;

        // Normal constructor
        public ChecklistGoal(string name, string desc, int points, int target, int bonus)
            : base(name, desc, points)
        {
            if (target <= 0) target = 1;
            _target = target;
            _bonus = bonus;
            _completed = 0;
        }

        // Constructor when loading with existing completed count
        public ChecklistGoal(string name, string desc, int points, int target, int bonus, int completed)
            : base(name, desc, points)
        {
            if (target <= 0) target = 1;
            _target = target;
            _bonus = bonus;
            _completed = completed;
            if (_completed < 0) _completed = 0;
            if (_completed > _target) _completed = _target;
        }

        public override int RecordEvent()
        {
            if (IsComplete()) return 0;

            _completed++;
            int award = _points;

            if (_completed >= _target)
            {
                // give bonus on completion
                award += _bonus;
            }

            return award;
        }

        public override bool IsComplete() => _completed >= _target;

        public override string GetStatus()
        {
            return IsComplete()
                ? $"[X] Completed {_completed}/{_target}"
                : $"[ ] Completed {_completed}/{_target}";
        }

        public override string SaveData() => $"Checklist|{GetName()}|{GetDescription()}|{_points}|{_target}|{_bonus}|{_completed}";
    }

    // ==========================================
    // NEGATIVE GOAL (Lose Points)
    // ==========================================
    class NegativeGoal : Goal
    {
        // _points stores the penalty magnitude (positive)
        public NegativeGoal(string name, string desc, int points)
            : base(name, desc, Math.Abs(points)) { }

        // Recording returns negative value (penalty)
        public override int RecordEvent() => -_points;

        public override bool IsComplete() => false;

        public override string GetStatus() => "[Bad Habit]";

        public override string SaveData() => $"Negative|{GetName()}|{GetDescription()}|{_points}";
    }

    // ==========================================
    // MAIN PROGRAM
    // ==========================================
    class Program
    {
        static List<Goal> goals = new List<Goal>();
        static int score = 0;
        const string SaveFileName = "goals.txt";

        static void Main(string[] args)
        {
            // Try to auto-load if file exists
            if (File.Exists(SaveFileName))
            {
                try
                {
                    LoadGoals(SaveFileName);
                    Console.WriteLine($"Auto-loaded save from {SaveFileName}.");
                }
                catch
                {
                    Console.WriteLine("Auto-load failed. Starting fresh.");
                }
            }

            while (true)
            {
                Console.WriteLine("\n===== ETERNAL QUEST =====");
                Console.WriteLine($"Score: {score}  |  Level: {GetLevel()}");
                Console.WriteLine("1. Create New Goal");
                Console.WriteLine("2. List Goals");
                Console.WriteLine("3. Record Event");
                Console.WriteLine("4. Save");
                Console.WriteLine("5. Load");
                Console.WriteLine("6. Quit");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        CreateGoal();
                        break;
                    case "2":
                        ListGoals();
                        break;
                    case "3":
                        RecordEvent();
                        break;
                    case "4":
                        try
                        {
                            SaveGoals(SaveFileName);
                            Console.WriteLine($"Saved to {SaveFileName}.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Save failed: " + ex.Message);
                        }
                        break;
                    case "5":
                        try
                        {
                            LoadGoals(SaveFileName);
                            Console.WriteLine($"Loaded from {SaveFileName}.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Load failed: " + ex.Message);
                        }
                        break;
                    case "6":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Unknown selection. Try again.");
                        break;
                }
            }
        }

        static int GetLevel() => (score / 1000) + 1;

        // ==========================================
        // CREATE GOAL
        // ==========================================
        static void CreateGoal()
        {
            Console.WriteLine("\nChoose goal type:");
            Console.WriteLine("1. Simple Goal (one-time)");
            Console.WriteLine("2. Eternal Goal (repeatable)");
            Console.WriteLine("3. Checklist Goal (complete N times + bonus)");
            Console.WriteLine("4. Negative Goal (lose points for bad habit)");
            Console.Write("Choice: ");
            string type = Console.ReadLine()?.Trim();

            Console.Write("Name: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("Description: ");
            string desc = Console.ReadLine() ?? "";

            int points = PromptInt("Points (positive number): ", min: 0);

            switch (type)
            {
                case "1":
                    goals.Add(new SimpleGoal(name, desc, points));
                    Console.WriteLine("Simple goal created.");
                    break;
                case "2":
                    goals.Add(new EternalGoal(name, desc, points));
                    Console.WriteLine("Eternal goal created.");
                    break;
                case "3":
                    int target = PromptInt("Times required to complete: ", min: 1);
                    int bonus = PromptInt("Completion bonus: ", min: 0);
                    goals.Add(new ChecklistGoal(name, desc, points, target, bonus));
                    Console.WriteLine("Checklist goal created.");
                    break;
                case "4":
                    goals.Add(new NegativeGoal(name, desc, points));
                    Console.WriteLine("Negative goal created.");
                    break;
                default:
                    Console.WriteLine("Unknown type. Creation cancelled.");
                    break;
            }
        }

        static int PromptInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (int.TryParse(s, out int value))
                {
                    if (value >= min && value <= max) return value;
                    Console.WriteLine($"Please enter a number between {min} and {max}.");
                }
                else
                {
                    Console.WriteLine("Invalid number. Try again.");
                }
            }
        }

        // ==========================================
        // LIST GOALS
        // ==========================================
        static void ListGoals()
        {
            Console.WriteLine("\n=== YOUR GOALS ===");
            if (goals.Count == 0)
            {
                Console.WriteLine("(No goals yet.)");
                return;
            }

            for (int i = 0; i < goals.Count; i++)
            {
                var g = goals[i];
                Console.WriteLine($"{i + 1}. {g.GetStatus()} {g.GetName()} - {g.GetDescription()}");
            }
        }

        // ==========================================
        // RECORD EVENT
        // ==========================================
        static void RecordEvent()
        {
            if (goals.Count == 0)
            {
                Console.WriteLine("No goals to record.");
                return;
            }

            ListGoals();
            int choice = PromptInt("Enter goal number to record (0 to cancel): ", min: 0, max: goals.Count);
            if (choice == 0) return;

            Goal g = goals[choice - 1];
            int awarded = g.RecordEvent();
            score += awarded;

            if (awarded > 0) Console.WriteLine($"You earned {awarded} points!");
            else if (awarded < 0) Console.WriteLine($"You lost {Math.Abs(awarded)} points (penalty).");
            else Console.WriteLine("No points awarded (goal might already be complete).");

            Console.WriteLine($"Total Score: {score}  Level: {GetLevel()}");
        }

        // ==========================================
        // SAVE GOALS
        // ==========================================
        static void SaveGoals(string filename)
        {
            var lines = new List<string>();
            lines.Add(score.ToString());

            foreach (var g in goals)
            {
                lines.Add(g.SaveData());
            }

            File.WriteAllLines(filename, lines);
        }

        // ==========================================
        // LOAD GOALS
        // ==========================================
        static void LoadGoals(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("Save file not found", filename);

            string[] lines = File.ReadAllLines(filename);
            if (lines.Length == 0) throw new FormatException("Save file is empty or invalid.");

            // First line is score
            score = int.Parse(lines[0]);

            goals.Clear();
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                Goal g = Goal.LoadFromData(lines[i]);
                goals.Add(g);
            }
        }
    }
}
