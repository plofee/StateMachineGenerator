using System.Text;

public readonly struct InputOutput(string Name, string? parameters)
{
    public string Name { get; init; } = Name;
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
                field = "State";
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
    public bool HasSavingLogic { set; get; }
    public string SavingChunk { set; get; }

    private string _rootPath;
    private bool _overwriteFiles;

    public StateMachineGenerator()
    {
        States = new Dictionary<string, List<string>>();
        States["State"] = [];
        Inputs = new ();
        Outputs = new ();
        _rootPath = Environment.CurrentDirectory;
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
#region Root
        string path = $"{_rootPath}/{ToCamelCase(Name)}";
        CreateFolderIfMissing(path);
        CreateFileIfMissing($"{path}/{Name}.cs", GenerateAutoNodeLines());
        CreateFileIfMissing($"{path}/{Name}Logic.Settings.cs", GenerateSettingsLines());
        if (HasSavingLogic)
        {
            CreateFileIfMissing($"{path}/{Name}Data.cs", GenerateDataLines());
        }
#endregion

#region Root/State
        path = $"{_rootPath}/{ToCamelCase(Name)}/state";
        CreateFolderIfMissing(path);
        CreateFileIfMissing($"{path}/{Name}Logic.cs", GenerateLogicFileLines());
        CreateFileIfMissing($"{path}/{Name}Logic.Data.cs", GenerateLogicDataLines());
        CreateFileIfMissing($"{path}/{Name}Logic.Input.cs", GenerateInputLines());
        CreateFileIfMissing($"{path}/{Name}Logic.Output.cs", GenerateOutputLines());
#endregion

#region Root/State/States
        path = $"{_rootPath}/{ToCamelCase(Name)}/state/states";
        CreateFolderIfMissing(path);
        foreach (string key in States.Keys)
        {
            for (int i = 0; i < States[key].Count; i++)
            {
                string extendedPath;
                if (key == "State")
                {
                    extendedPath = $"{path}/{States[key][i]}.cs";
                }
                else
                {
                    extendedPath = $"{path}/{key}.{States[key][i]}.cs";
                }
                CreateFileIfMissing(extendedPath, GenerateStateLines(States[key][i], key));
            }
        }
#endregion
    }

    private List<string> GenerateAutoNodeLines()
    {
        List<string> lines = GetLineList();

        if (HasSavingLogic)
        {
            lines.Add("using Chickensoft.SaveFileBuilder;");
        }

        lines.AddRange([
            "using Chickensoft.LogicBlocks;",
            "using Chickensoft.Introspection;",
            "using Chickensoft.Collections;",
            "using Chickensoft.AutoInject;",
            "using Godot;",
            "",
            $"public interface I{Name}",
            "{",
            "}",
            "",
            "[Meta(typeof(IAutoNode))]",
            $"public partial class {Name} : Node, I{Name}",
            "{",
            "\tpublic override void _Notification(int what) => this.Notify(what);",
            "",
        ]);

        if (HasSavingLogic)
        {
            lines.AddRange([
                "#region Save",
                "\t[Dependency] public EntityTable<uint> EntityTable => this.DependOn<EntityTable<uint>>();",
                $"\t[Dependency] public ISaveChunk<{SavingChunk}Data> {SavingChunk}Chunk => this.DependOn<ISaveChunk<{SavingChunk}Data>>();",
                $"\tpublic ISaveChunk<{Name}Data> {Name}Chunk {{ get; set; }} = default!;",
                "#endregion Save",
                "",
                "#region  Dependencies",
                $"\t[Dependency] public {SavingChunk}Data {SavingChunk}Data => this.DependOn<{SavingChunk}Data>();",
                "#endregion Dependencies",
            ]);
        }
        else
        {
            lines.AddRange([
                "#region  Dependencies",
                "\t[Dependency] public EntityTable<uint> EntityTable => this.DependOn<EntityTable<uint>>();",
                "#endregion Dependencies",
            ]);
        }

        lines.AddRange([
            "",
            "#region Provisions",
            "#endregion",
            "",
            "#region  State",
            $"\tpublic {Name}Logic {Name}Logic {{ get; set; }} = default!;",
            $"\tpublic {Name}Logic.IBinding {Name}Binding {{ get; set; }} = default!;",
            "#endregion State",
            "",
            "\tpublic void Setup()",
            "\t{",
            $"\t\t{Name}Logic = new {Name}Logic();",
            $"\t\t{Name}Logic.Save(() => new {Name}Logic.Data() {{ }});",
            "",
        ]);

        if (HasSavingLogic)
        {
            lines.AddRange([
                $"\t\t{Name}Chunk = new SaveChunk<{Name}Data>(",
                $"\t\t\tonSave: (chunk) => new {Name}Data()",
                "\t\t\t{",
                $"\t\t\t\tStateMachine = {Name}Logic",
                "\t\t\t},",
                "\t\t\tonLoad: (chunk, data) =>",
                "\t\t\t{",
                $"\t\t\t\t{Name}Logic.RestoreFrom(data.StateMachine);",
                $"\t\t\t\t{Name}Logic.Start();",
                "\t\t\t}",
                "\t\t);",
                "\t}",
                "",
                "\tpublic void OnResolved()",
                "\t{",
                $"\t\t{SavingChunk}Chunk.AddChunk({Name}Chunk);",
            ]);
        }
        else
        {
            lines.AddRange([
                "\t}",
                "",
                "\tpublic void OnResolved()",
                "\t{",
            ]);
        }

        lines.AddRange([
            $"\t\t{Name}Binding = {Name}Logic.Bind();",
            "\t\tthis.Provide();",
            $"\t\t{Name}Logic.Start();",
            "\t}",
            "",
            "\tpublic void OnExitTree()",
            "\t{",
            $"\t\t{Name}Logic.Stop();",
            $"\t\t{Name}Binding.Dispose();",
            "\t}",
            "}",
        ]);
        
        return lines;
    }

    private List<string> GenerateDataLines()
    {
        List<string> lines = GetLineList();
        lines.AddRange(
        [
            "using System;",
            "using System.Collections.Generic;",
            "using Chickensoft.Collections;",
            "using Chickensoft.Introspection;",
            "using Chickensoft.Serialization;",
            "using Godot;",
            "",
            "[Meta, Id(\"" + ToSnakeCase(Name) + "_data\")]",
            $"public partial record {Name}Data",
            "{",
            "\t[Save(\"state_machine\")]",
            $"\tpublic required I{Name}Logic StateMachine {{ get; init; }}",
            "",
            "\t[Save(\"id\")]",
            "\tpublic required uint Id { get; init; }",
            "}",
        ]);
        return lines;
    }

    private List<string> GenerateLogicFileLines()
    {
        List<string> lines = GetLineList();
        lines.AddRange(
        [
            "using System.Collections.Generic;",
            "using Chickensoft.Introspection;",
            "using Chickensoft.LogicBlocks;",
            "using Chickensoft.Collections;",
            "",
            $"public interface I{Name}Logic : ILogicBlock<{Name}Logic.State>;",
            "",
            "[Meta, Id(\"" + ToSnakeCase(Name) + "_logic\")]",
            "[LogicBlock(typeof(State), Diagram = true)]",
            $"public partial class {Name}Logic : LogicBlock<{Name}Logic.State>, I{Name}Logic",
            "{"
        ]);

        if (InitialState == "State")
        {
            lines.Add($"\tpublic override Transition GetInitialState() => To<State>();");
        }
        else
        {
            lines.Add($"\tpublic override Transition GetInitialState() => To<State.{InitialState}>();");
        }
            
        lines.AddRange([
            "",
            "\t[Meta, Id(\"" + ToSnakeCase(Name) + "_logic_state\")]",
            "\tpublic partial record State : StateLogic<State>",
            "\t{",
            "\t\tpublic State()",
            "\t\t{",
            "\t\t}",
            "\t}",
            "}",
        ]);

        return lines;
    }

    private List<string> GenerateLogicDataLines()
    {
        List<string> lines = GetLineList();
        lines.AddRange(
        [
            "using System.Collections.Generic;",
            "using Chickensoft.Introspection;",
            "using Chickensoft.Serialization;",
            "using Chickensoft.Collections;",
            "using Godot;",
            "",
            $"public partial class {Name}Logic",
            "{",
            "\t[Meta, Id(\"" + ToSnakeCase(Name) + "_logic_data\")]",
            "\tpublic partial record Data",
            "\t{",
            "\t}",
            "}",
        ]);
        return lines;
    }

    private List<string> GenerateInputLines() => GenerateInputOutputLines("Input", Inputs);
    private List<string> GenerateOutputLines() => GenerateInputOutputLines("Output", Outputs);

    private List<string> GenerateInputOutputLines(string type, List<InputOutput> members)
    {
        List<string> lines = GetLineList();
        lines.AddRange(
        [
            "using Godot;",
            "",
            $"public partial class {Name}Logic",
            "{",
            $"\tpublic static class {type}",
            "\t{"
        ]);

        for (int i = 0; i < members.Count; i++)
        {
            if (string.IsNullOrEmpty(members[i].Parameters))
            {
                lines.Add($"\t\tpublic readonly record struct {members[i].Name};");
            }
            else
            {
                lines.Add($"\t\tpublic readonly record struct {members[i].Name}({members[i].Parameters});");
            }
        }
        
        lines.AddRange(["\t}", "}"]);
        return lines;
    }

    private List<string> GenerateSettingsLines()
    {
        List<string> lines = GetLineList();
        lines.AddRange(
        [
            "using Godot;",
            "",
            $"public partial class {Name}Logic",
            "{",
            $"\t/// <summary>{Name} settings.</summary>",
            "\tpublic record Settings",
            "\t{",
            "\t}",
            "}"
        ]);
        return lines;
    }

    private List<string> GenerateStateLines(string stateName, string parentName)
    {
        List<string> lines = GetLineList();
        lines.AddRange(
        [
            "using Chickensoft.LogicBlocks;",
            "using Chickensoft.Introspection;",
            "using Chickensoft.Collections;",
            "using Godot;",
            "",
            $"public partial class {Name}Logic",
            "{",
            "\tpublic partial record State",
            "\t{",
            $"\t\t[Meta, Id(\"{ToSnakeCase(Name)}_logic_state_{ToSnakeCase(stateName)}\")]",
            $"\t\tpublic partial record {stateName}: {parentName}",
            "\t\t{",
            $"\t\t\tpublic {stateName}()",
            "\t\t\t{",
            "\t\t\t}",
            "\t\t}",
            "\t}",
            "}"
        ]);
        return lines;
    }
    
    private void CreateFileIfMissing(string path, IEnumerable<string> lines)
    {
        if (_overwriteFiles == false && File.Exists(path))
        {
            Console.WriteLine($"WARNING: File '{path}' already exists. Do you want to overwrite all of the already existing files? (y/N)");
            if ((Console.ReadLine() ?? "").ToLower().Contains('y'))
            {
                _overwriteFiles = true;
            }
            else
            {
                throw new Exception("File overwriting permission denied.");
            }
        }
        
        if (string.IsNullOrEmpty(Namespace) == false)
        {
            lines = lines.Prepend($"namespace {Namespace};\n");
        }
        File.WriteAllLines(path, lines);
    }
    
    private void CreateFolderIfMissing(string fullPath)
    {
        if (!Directory.Exists(fullPath))
        {
            _ = Directory.CreateDirectory(fullPath);
        }
    }

    private List<string> GetLineList()
    {
        if (string.IsNullOrEmpty(Namespace))
        {
            return [];
        }
        else
        {
            return [
                $"namespace {Namespace};",
                ""
            ];
        }
    }
    
    private static string ToCamelCase(string str) => char.ToLower(str[0]) + str[1..];
    private static string ToSnakeCase(string fullName)
    {
        List<string> words = [];
        if (fullName.Length == 0)
        {
            throw new Exception($"Converting '{fullName}' to snake case failed. Process aborted,");
        }
            
        int startIndex = -1;
        for (int i = 0; i < fullName.Length; i++)
        {
            int endIndex;
            if (Char.IsUpper(fullName[i]))
            {
                if (startIndex != -1)
                {
                    endIndex = i - 1;
                    words.Add(fullName.Substring(startIndex, endIndex - startIndex + 1).ToLower());
                }

                startIndex = i;
            }

            if (i == fullName.Length - 1)
            {
                endIndex = i;
                words.Add(fullName.Substring(startIndex, endIndex - startIndex + 1).ToLower());
            }
        }
        return string.Join("_", words);
    }
}
