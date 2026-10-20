class Program
{
    static void Main(string[] args)
    {
        StateMachineGenerator stateMachineGenerator = new StateMachineGenerator();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("INFO: ");
        Console.ResetColor();
        Console.WriteLine("State Machine file generating process will start after you provide all of the necessary information so the process can be stopped before that point without adverse effects.");
        
        PrintWithSeparator("Enter the name of the state machine:");
        stateMachineGenerator.Name = Console.ReadLine();
        if (string.IsNullOrEmpty(stateMachineGenerator.Name))
        {
            Console.WriteLine("State Machine name is required!\nShutting down.");
            return;
        }
        
        PrintWithSeparatorAndHighlightedText("Should generated files be in a namespace?", "enter name if you do or leave blank");
        stateMachineGenerator.Namespace = Console.ReadLine();
        
        PrintWithSeparatorAndHighlightedText("Do you want a particular initial state?", "enter the state name or leave blank");
        string stateName = Console.ReadLine() ?? ""; 
        stateMachineGenerator.InitialState = stateName;
        
        PrintWithSeparator("Start entering possible states. When you are finished with the states submit a blank line to continue.");
        string parentOdPrevious = "State";
        string previous = stateName;
        string entered;
        
        do
        {
            entered = Console.ReadLine() ?? "";
            if (entered.Contains('.'))
            {
                if (entered.StartsWith(".."))
                {
                    previous = entered[2..];
                }
                else if (entered.StartsWith('.'))
                {
                    parentOdPrevious = previous;
                    previous = entered[1..];
                }
                else
                {
                    try
                    {
                        previous = entered[(entered.IndexOf('.') + 1)..];
                        parentOdPrevious = entered[..entered.IndexOf('.')];
                    }
                    catch (ArgumentOutOfRangeException exception)
                    {
                        Console.WriteLine("State name after '.' is missing!");
                        continue;
                    }
                }
                stateMachineGenerator.AddState(previous, parentOdPrevious);
            }
            else if (entered.Length > 0)
            {
                previous = entered;
                parentOdPrevious = "State";
                stateMachineGenerator.AddState(entered, parentOdPrevious);
            }
        } while (!string.IsNullOrEmpty(entered));
        
        PrintWithSeparatorAndHighlightedText("Should state file names start with 'StateMachineNameLogic.State.'", "y/N");
        stateMachineGenerator.UseExtendedStateFileName = (Console.ReadLine() ?? "").ToLower().Contains('y');
        
        PrintWithSeparatorAndHighlightedText("Start entering State Machine inputs.", "leave name blank when finished");
        do
        {
            PrintWithSeparator("Enter input name:");
            entered = Console.ReadLine() ?? "";
            if (entered.Length > 0)
            {
                Console.WriteLine("Enter arguments for input or leave blank:");
                stateMachineGenerator.AddInput(entered, Console.ReadLine());
            }
        } while (!string.IsNullOrEmpty(entered));
        
        PrintWithSeparatorAndHighlightedText("Start entering State Machine outputs.", "leave name blank when finished");

        do
        {
            PrintWithSeparator("Enter output name:");
            entered = Console.ReadLine() ?? "";
            if (entered.Length > 0)
            {
                Console.WriteLine("Enter arguments for output or leave blank:");
                stateMachineGenerator.AddOutput(entered, Console.ReadLine());
            }
        } while (!string.IsNullOrEmpty(entered));
        
        PrintWithSeparatorAndHighlightedText("Generate saving logic?", "y/N");
        entered = Console.ReadLine() ?? "";
        stateMachineGenerator.HasSavingLogic = entered.ToLower().Contains('y');
        if (stateMachineGenerator.HasSavingLogic)
        {
            PrintWithSeparatorAndHighlightedText("Enter the name of the chunk you want the save data to be part of", "if you leave blank the default 'App' will be used");
            stateMachineGenerator.SavingChunk = Console.ReadLine();
        }
        
        PrintWithSeparatorAndHighlightedText("Generate settings file?", "y/N");
        entered = Console.ReadLine() ?? "";
        stateMachineGenerator.HasSettings = entered.ToLower().Contains('y');

        try
        {
            stateMachineGenerator.GenerateFiles();
            Console.WriteLine("Files generated successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"File generation failed because of: {e.Message}");
        }
    }

    private static void PrintWithSeparator(string line)
    {
        Console.WriteLine(new string('-', line.Length));
        Console.WriteLine(line);
    }

    private static void PrintWithSeparatorAndHighlightedText(string line, string highlightedText)
    {
        Console.WriteLine(new string('-', line.Length + highlightedText.Length + 1));
        Console.Write(line);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($" ({highlightedText})");
        Console.ResetColor();
        Console.WriteLine();
    }
}
