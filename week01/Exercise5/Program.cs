using System;

class Program
{
    static void Main(string[] args)
    {
        // Display initial Exercise5 message
        Console.WriteLine("Hello World! This is the Exercise5 Project.\n");

        // Run the main program workflow
        DisplayWelcome();

        string userName = PromptUserName();
        int userNumber = PromptUserNumber();
        int squaredNumber = SquareNumber(userNumber);

        DisplayResult(userName, squaredNumber);
    }

    // Function 1: Display welcome message
    static void DisplayWelcome()
    {
        Console.WriteLine("Welcome to the Program!");
    }

    // Function 2: Prompt for user's name
    static string PromptUserName()
    {
        Console.Write("Please enter your name: ");
        return Console.ReadLine();
    }

    // Function 3: Prompt for user's favorite number
    static int PromptUserNumber()
    {
        int number;
        while (true)
        {
            Console.Write("Please enter your favorite number: ");
            string input = Console.ReadLine();
            if (int.TryParse(input, out number))
            {
                break;
            }
            Console.WriteLine("Invalid input. Please enter a valid integer.");
        }
        return number;
    }

    // Function 4: Square the number
    static int SquareNumber(int number)
    {
        return number * number;
    }

    // Function 5: Display result
    static void DisplayResult(string name, int squaredNumber)
    {
        Console.WriteLine($"{name}, the square of your number is {squaredNumber}");
    }
}
