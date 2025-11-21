using System;

class Program
{
    static void Main()
    {
        // Display initial Exercise2 message
        Console.WriteLine("Hello World! This is the Exercise2 Project.\n");

        // Ask the user for their grade percentage
        Console.Write("Enter your grade percentage: ");
        string input = Console.ReadLine();
        int grade;
        if (!int.TryParse(input, out grade))
        {
            Console.WriteLine("Invalid input. Please enter a number.");
            return;
        }

        string letter = "";
        string sign = "";

        // Determine letter grade
        if (grade >= 90)
            letter = "A";
        else if (grade >= 80)
            letter = "B";
        else if (grade >= 70)
            letter = "C";
        else if (grade >= 60)
            letter = "D";
        else
            letter = "F";

        // Determine sign for A-D
        if (letter != "F")
        {
            int lastDigit = grade % 10;

            if (letter == "A" && grade == 100)
                sign = ""; // No A+
            else if (letter == "A" && lastDigit >= 7)
                sign = ""; // No A+
            else if (lastDigit >= 7)
                sign = "+";
            else if (lastDigit < 3)
                sign = "-";
            else
                sign = "";
        }

        // Output the letter grade with sign
        Console.WriteLine($"Your grade is: {letter}{sign}");

        // Determine pass/fail
        if (grade >= 70)
            Console.WriteLine("Congratulations! You passed the course.");
        else
            Console.WriteLine("Don't worry! Keep trying and you will improve.");
    }
}
