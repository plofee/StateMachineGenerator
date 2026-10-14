namespace StateMachineGenerator;

using System.Text;

public readonly struct InputOutput(string name, string? parameters)
{
    public string Name { get; init; } = name;
    public string? Parameters { get; init; } = parameters;
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("\t\tpublic readonly record struct ");
        sb.Append(Name);
        sb.Append(string.IsNullOrEmpty(Parameters) ? "" : Parameters);
        sb.Append(';');
        return sb.ToString();
    }
}

public class StateMachineGenerator
{
    public string? Namespace { get; set; }
    public string Name { get; set; }
    public string? InitialState
    {
        get;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            field = value;
            States["State"].Add(field);
            States[value] = new ();
        }
    }
    public Dictionary<string, List<string>> States { get; private set; }
    public List<InputOutput> Inputs { get; private set; }
    public List<InputOutput> Outputs { get; private set; }
    public bool HasSettings { get; set; }
    public bool HasSavingLogic { get; set; }

    public StateMachineGenerator()
    {
        States = new Dictionary<string, List<string>>();
        States["State"] = [];
        Inputs = new ();
        Outputs = new ();
    }

    public void AddState(string name, string parent)
    {
        if (!States.ContainsKey(parent))
        {
            States[parent] = [name];
            if (States["State"].Contains(parent) == false)
            {
                States["State"].Add(parent);
            }
        }
        else
        {
            States[parent].Add(name);
        }
        Console.WriteLine($"State {name} with a parent {parent} has been added");
    }
    public void AddInput(string name, string @args) => Inputs.Add(new InputOutput(name, @args));
    public void AddOutput(string name, string @args) => Outputs.Add(new InputOutput(name, @args));

    public void GenerateFiles()
    {
        // TODO
    }
}
