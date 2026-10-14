namespace StateMachineGenerator;

class Program
{
    static void Main(string[] args)
    {
        StateMachineGenerator stateMachineGenerator = new StateMachineGenerator();
        Console.WriteLine("State Machine generation process will start after you provide necessary information so the process can be stopped before that point without adverse effects.");
        
        PrintWithSeparator("Enter the name of the state machine:");
        stateMachineGenerator.Name = Console.ReadLine();
        if (string.IsNullOrEmpty(stateMachineGenerator.Name))
        {
            Console.WriteLine("State Machine name is required!\nShutting down.");
            return;
        }
        
        PrintWithSeparator("Do you want the generated files in a namespace? (enter name if you do or leave blank if you don't)");
        stateMachineGenerator.Namespace = Console.ReadLine();
        
        PrintWithSeparator("Do you want a particular initial state? (enter the state name or leave blank if you don't)");
        string stateName = Console.ReadLine() ?? ""; 
        stateMachineGenerator.InitialState = stateName;
        
        PrintWithSeparator("Start entering possible states. When you are finished with the states leave the line blank and hit enter to continue.");
        Console.WriteLine("If you just enter the name of the state, that state will be a child of State (the default parent for states).");
        Console.WriteLine("You can create states that are children of other states by following the convention: ParentState.ChildState");
        Console.WriteLine("Additionally, if a state is a child of the previously entered state start with a dot: .ChildState");
        Console.WriteLine("If a state has the same parent as the previously entered state start with two dots: ..ChildState");
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
        
        PrintWithSeparator("Start entering State Machine inputs. Leave name blank when finished.");
        do
        {
            PrintWithSeparator("Enter input name:");
            entered = Console.ReadLine() ?? "";
            if (entered.Length > 0)
            {
                Console.WriteLine("Enter arguments for input or leave blank:");
                stateMachineGenerator.AddInput(entered, Console.ReadLine());
            }
        } while (string.IsNullOrEmpty(entered));
        
        PrintWithSeparator("Start entering State Machine outputs. Leave blank when finished.");
        do
        {
            PrintWithSeparator("Enter output name:");
            entered = Console.ReadLine() ?? "";
            if (entered.Length > 0)
            {
                Console.WriteLine("Enter arguments for output or leave blank:");
                stateMachineGenerator.AddOutput(entered, Console.ReadLine());
            }
        } while (string.IsNullOrEmpty(entered));
        
        PrintWithSeparator("Generate saving logic? (y/N)");
        entered = Console.ReadLine() ?? "";
        stateMachineGenerator.HasSavingLogic = entered.ToLower().Contains('y');
        
        PrintWithSeparator("Generate settings file? (y/N)");
        entered = Console.ReadLine() ?? "";
        stateMachineGenerator.HasSettings = entered.ToLower().Contains('y');
        
        stateMachineGenerator.GenerateFiles();
        Console.WriteLine("Files generated successfully.");
    }

    private static void PrintWithSeparator(string line)
    {
        Console.WriteLine("");
        Console.WriteLine(new string('-', line.Length));
        Console.WriteLine(line);
    }
}
