namespace Sandbox;

public class Programm
{
    public static void Main()
    {
        new Processor().Run();
    }
}

public class Processor
{
    public StateMachine StateMachine { get; set; }

    public Processor()
    {
        StateMachine = new StateMachine();
    }

    public void Run()
    {
        for(char input = default; input != 'E'; )
        {
            StateMachine.Process(input = Console.ReadKey().KeyChar);
        }
    }
}

public class StateMachine
{
    private readonly Model _model;
    private IState _currentState;

    public StateMachine()
    {
        _model = new Model();
        _currentState = new InitialState(_model);
        Print();
    }

    public void Process(char input)
    {
        if (_currentState.AvailableKeys.Count > 0 && !_currentState.AvailableKeys.Contains(input))
        {
            Console.WriteLine("Wrong input, try again please.");
            return;
        }

        _currentState = _currentState.Next(input);
        Print();
    }

    public void Print()
    {
        Console.Clear();
        _currentState.InitText();
    }
}

public interface IState
{
    public void InitText();
    public IState Next(char key);
    public List<char> AvailableKeys { get; }
}

public abstract class BaseState(Model model) : IState
{
    protected readonly Model _model = model;

    public abstract List<char> AvailableKeys { get; }
    public abstract void InitText();
    public abstract IState Next(char key);
}

public class InitialState(Model model) : BaseState(model)
{
    public override List<char> AvailableKeys => ['S', 'G'];

    public override void InitText() => Console.WriteLine("Print either 'S' to start or 'G' to get statistics.");

    public override IState Next(char key) => key == 'S' ? new ProccesingState(_model) : new StatisticsState(_model);
}

public class StatisticsState(Model model) : BaseState(model)
{
    public override List<char> AvailableKeys => [];

    public override void InitText()
    {
        Console.WriteLine("Your results:");
        foreach (var e in _model.Statistics.OrderByDescending(statRow => statRow.Key.A + statRow.Key.B))
        {
            Console.WriteLine($"{e.Key.A}-{e.Key.B}: {e.Value}");
        }
        Console.WriteLine("Press any key to return home.");
    }

    public override IState Next(char key)
    {
        return new InitialState(_model);
    }
}

public class ProccesingState(Model model) : BaseState(model)
{
    public override List<char> AvailableKeys => _model.AvailableKeys;

    public override void InitText()
    {
        "ABC".ToList().ForEach(key => Console.WriteLine($"{key}: {_model.Attempts[key]}|10, {_model.Stone[key]}"));
        Console.WriteLine($"Press key(one of available: {string.Join(" ", _model.AvailableKeys)}) to try increase relevant feature of stone.");
    }

    public override IState Next(char key)
    {
        if (!_model.IsComplete)
        {
            _model.Try(key);
            return new ProccesingState(_model);
        } 
        else
        {
            _model.UpdateStatistics();
            _model.Reset();
            return new InitialState(_model);
        }
    }
}

public class Model
{
    public List<char> AvailableKeys => [.. Attempts.Where(pair => pair.Value < 10).Select(pair => pair.Key)];
    public bool IsComplete => AvailableKeys.Count == 0;
    public Dictionary<(int A, int B), int> Statistics { get; private set; } = [];
    public Dictionary<char, int> Stone { get; private set; } = [];
    public Dictionary<char, int> Attempts { get; private set; } = [];

    private readonly Random _generator = new();
    private decimal _chanceValue = 0.75M;
    private decimal Chance { get => _chanceValue; set => _chanceValue = value > 0.75M ? 0.75M : value < 0.25M ? 0.25M : value; }

    public Model() => Initialize();

    public void Reset()
    {
        UpdateStatistics();
        Initialize();
    }

    public void Initialize()
    {
        Stone = new() { ['A'] = 0, ['B'] = 0, ['C'] = 0 };
        Attempts = new() { ['A'] = 0, ['B'] = 0, ['C'] = 0 };
    }

    public void UpdateStatistics()
    {
        if (Statistics.ContainsKey((Stone['A'], Stone['B'])))
        {
            Statistics[(Stone['A'], Stone['B'])]++;
        }
        else
        {
            Statistics.Add((Stone['A'], Stone['B']), 0);
        }
    }

    public void Try(char property)
    {
        Attempts[property]++;
        if ((decimal)_generator.NextDouble() < Chance)
        {
            Chance += 0.1M;
        }
        else
        {
            Stone[property]++;
            Chance -= 0.1M;
        }
    }
}