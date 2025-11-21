using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

// Comment class
class Comment
{
    public string CommenterName { get; private set; }
    public string Text { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Comment(string commenterName, string text)
    {
        if (string.IsNullOrWhiteSpace(commenterName))
        {
            throw new ArgumentException("Commenter name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Comment text cannot be empty.");
        }

        CommenterName = commenterName.Trim();
        Text = text.Trim();
        CreatedAt = DateTime.Now;
    }

    public void Display()
    {
        Console.WriteLine("- " + CommenterName + " (" +
                          CreatedAt.ToString("yyyy-MM-dd HH:mm") + "): " + Text);
    }

    public override string ToString()
    {
        return CommenterName + ": " + Text;
    }

    public bool ContainsKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return false;
        }

        return Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
               || CommenterName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

// Video class
class Video
{
    public string Title { get; private set; }
    public string Author { get; private set; }
    public int LengthSeconds { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public int Views { get; private set; }

    private readonly List<Comment> Comments;

    public Video(string title, string author, int lengthSeconds)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("Author cannot be empty.");
        }

        if (lengthSeconds <= 0)
        {
            throw new ArgumentException("Length must be greater than zero seconds.");
        }

        Title = title.Trim();
        Author = author.Trim();
        LengthSeconds = lengthSeconds;
        UploadedAt = DateTime.Now;
        Views = 0;
        Comments = new List<Comment>();
    }

    public void AddComment(Comment comment)
    {
        if (comment == null)
        {
            throw new ArgumentNullException("comment");
        }

        Comments.Add(comment);
    }

    public int GetCommentCount()
    {
        return Comments.Count;
    }

    public void AddView()
    {
        Views++;
    }

    public string GetFormattedLength()
    {
        int minutes = LengthSeconds / 60;
        int seconds = LengthSeconds % 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public IEnumerable<Comment> SearchComments(string keyword)
    {
        foreach (Comment comment in Comments)
        {
            if (comment.ContainsKeyword(keyword))
            {
                yield return comment;
            }
        }
    }

    public void DisplayInfo(bool showComments = true)
    {
        Console.WriteLine("Title: " + Title);
        Console.WriteLine("Author: " + Author);
        Console.WriteLine("Length: " + GetFormattedLength() + " (" +
                          LengthSeconds + " seconds)");
        Console.WriteLine("Uploaded: " + UploadedAt.ToString("yyyy-MM-dd HH:mm"));
        Console.WriteLine("Views: " + Views);
        Console.WriteLine("Number of comments: " + GetCommentCount());

        if (showComments)
        {
            Console.WriteLine("Comments:");
            if (Comments.Count == 0)
            {
                Console.WriteLine("- No comments yet.");
            }
            else
            {
                foreach (Comment comment in Comments)
                {
                    comment.Display();
                }
            }
        }

        Console.WriteLine(new string('-', 60));
    }
}

// Program class
class Program
{
    static void Main()
    {
        // Create videos
        Video video1 = new Video("Introduction to C#", "Alice", 300);
        Video video2 = new Video("OOP Concepts", "Bob", 450);
        Video video3 = new Video("C# Abstraction Example", "Charlie", 400);

        // Simulate views
        Random rng = new Random();
        SimulateViews(video1, rng.Next(100, 300));
        SimulateViews(video2, rng.Next(50, 200));
        SimulateViews(video3, rng.Next(10, 150));

        // Add comments to video1
        video1.AddComment(new Comment("John", "Great tutorial!"));
        video1.AddComment(new Comment("Emma", "Very helpful, thanks!"));
        video1.AddComment(new Comment("Michael", "I learned a lot."));

        // Add comments to video2
        video2.AddComment(new Comment("Sophia", "Clear explanation."));
        video2.AddComment(new Comment("David", "Can you cover inheritance next?"));
        video2.AddComment(new Comment("Olivia", "Excellent examples."));

        // Add comments to video3
        video3.AddComment(new Comment("James", "This really clarifies abstraction."));
        video3.AddComment(new Comment("Isabella", "Well structured and easy to follow."));
        video3.AddComment(new Comment("Liam", "Thanks for this video!"));

        // Put videos into a list
        List<Video> videos = new List<Video> { video1, video2, video3 };

        // Display info for all videos
        Console.WriteLine("==== All Videos ====");
        foreach (Video video in videos)
        {
            video.DisplayInfo();
        }

        // Show most commented video
        Console.WriteLine();
        Console.WriteLine("==== Most Commented Video ====");
        Video mostCommented = GetMostCommentedVideo(videos);
        if (mostCommented != null)
        {
            mostCommented.DisplayInfo();
        }

        // Simple keyword search in comments
        Console.WriteLine();
        Console.Write("Enter a keyword to search in comments (or press Enter to skip): ");
        string keyword = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            Console.WriteLine();
            Console.WriteLine("==== Search Results for \"" + keyword + "\" ====");
            SearchCommentsAcrossVideos(videos, keyword);
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void SimulateViews(Video video, int count)
    {
        for (int i = 0; i < count; i++)
        {
            video.AddView();
        }
    }

    static Video GetMostCommentedVideo(List<Video> videos)
    {
        if (videos == null || videos.Count == 0)
        {
            return null;
        }

        int maxComments = -1;
        Video selected = null;

        foreach (Video video in videos)
        {
            int comments = video.GetCommentCount();
            if (comments > maxComments)
            {
                maxComments = comments;
                selected = video;
            }
        }

        return selected;
    }

    static void SearchCommentsAcrossVideos(List<Video> videos, string keyword)
    {
        bool anyFound = false;

        foreach (Video video in videos)
        {
            List<Comment> matches = new List<Comment>(video.SearchComments(keyword));

            if (matches.Count > 0)
            {
                anyFound = true;
                Console.WriteLine("Video: " + video.Title + " by " + video.Author);
                foreach (Comment comment in matches)
                {
                    comment.Display();
                }
                Console.WriteLine(new string('-', 60));
            }
        }

        if (!anyFound)
        {
            Console.WriteLine("No comments found containing the keyword \"" + keyword + "\".");
        }
    }
}