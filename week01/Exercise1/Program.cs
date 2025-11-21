using System;

class Program
{
    static void Main(string[] args)
    {
        // Original hello message
        Console.WriteLine("Hello World! This is the Exercise1 Project.");

        // Prompt the user for their first name
        Console.Write("What is your first name? ");
        string firstName = Console.ReadLine();

        // Prompt the user for their last name
        Console.Write("What is your last name? ");
        string lastName = Console.ReadLine();

        // Display the Bond-style introduction
        Console.WriteLine($"\nYour name is {lastName}, {firstName} {lastName}.");
    }
}
