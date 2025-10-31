using System;

class Program
{
    static void Main()
    {
        // Display initial Exercise3 message
        Console.WriteLine("Hello World! This is the Exercise3 Project.\n");

        Random randomGenerator = new Random();
        bool playAgain = true;

        while (playAgain)
        {
            int magicNumber = randomGenerator.Next(1, 101);
            int guess = -1;
            int attempts = 0;

            Console.WriteLine("Guess My Number Game!");

            while (guess != magicNumber)
            {
                Console.Write("Enter your guess (1-100): ");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out guess))
                {
                    Console.WriteLine("Please enter a valid number.");
                    continue;
                }

                attempts++;

                if (guess < magicNumber)
                {
                    Console.WriteLine("Higher");
                }
                else if (guess > magicNumber)
                {
                    Console.WriteLine("Lower");
                }
                else
                {
                    Console.WriteLine($"You guessed it! The number was {magicNumber}.");
                    Console.WriteLine($"It took you {attempts} attempts.");
                }
            }

            Console.Write("Do you want to play again? (yes/no): ");
            string response = Console.ReadLine().ToLower();
            playAgain = (response == "yes");
        }

        Console.WriteLine("Thanks for playing!");
    }
}
