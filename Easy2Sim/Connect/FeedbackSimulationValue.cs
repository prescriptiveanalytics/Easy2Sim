using Easy2Sim.Environment;
using Easy2Sim.Interfaces;
using Newtonsoft.Json;

namespace Easy2Sim.Connect;

public class FeedbackSimulationValue<T, T1> : IFrameworkBase, ISimulationValue
{
    [JsonProperty]
    public Guid Guid { get; }

    [JsonIgnore]
    public Guid EnvironmentGuid { get; set; }

    [JsonIgnore]
    public SimulationBase? Parent
    {
        get
        {
            SimulationEnvironment environment = ComponentRegister.GetEnvironment(EnvironmentGuid);
            SimulationBase simBase = environment.GetComponentByName(ParentName);
            if (simBase == null)
            {
                simBase.LogError($"Simulation value ({ParentName}-{PropertyName} parent not found");
            }
            return simBase;
        }
    }

    [JsonProperty]
    public string PropertyName { get; }
    [JsonProperty]
    public string ParentName { get; set; }

    [JsonProperty]
    public List<SimulationValueAttributes> Attributes { get; set; }

    [JsonProperty]
    public bool ValueChanged { get; set; }
    [JsonProperty]
    public bool FeedbackValueChanged { get; set; }
    [JsonProperty]
    private T? _value;
    [JsonProperty]
    private T1? _feedbackValue;

    [JsonConstructor]
    public FeedbackSimulationValue()
    {
        PropertyName = string.Empty;
        ParentName = string.Empty;
        Attributes = new List<SimulationValueAttributes>();
        FeedbackValueChanged = false;
    }

    public FeedbackSimulationValue(T value, T1 feedbackValue, string propertyName, SimulationBase simBase, List<SimulationValueAttributes> attributes)
    {
        Attributes = attributes;
        PropertyName = propertyName;
        ParentName = simBase.Easy2SimName;
        _value = value;
        _feedbackValue = feedbackValue;
        Guid = Guid.NewGuid();
        EnvironmentGuid = simBase.SimulationEnvironmentGuid;
    }
    public FeedbackSimulationValue(T value, T1 feedbackValue, string propertyName, SimulationBase simBase, SimulationValueAttributes attribute)
    {
        Attributes = new List<SimulationValueAttributes>() { attribute };
        PropertyName = propertyName;
        ParentName = simBase.Easy2SimName;
        _value = value;
        _feedbackValue = feedbackValue;
        Guid = Guid.NewGuid();
        EnvironmentGuid = simBase.SimulationEnvironmentGuid;
    }

    // Event to be raised when the attribute changes
    public T? GetValue()
    {
        return _value;
    }

    public T Value
    {
        get => _value;
        set
        {
            T? oldValue = _value;
            _value = value;
            ValueChanged = true;
            OnPropertyChanged(value, oldValue, SimulationEventType.DiscreteCalculation);
        }
    }
    public T1 FeedbackValue
    {
        get => _feedbackValue;
        set
        {
            T1? oldValue = _feedbackValue;
            _feedbackValue = value;
            FeedbackValueChanged = true;

            OnFeedbackValueChanged(value, oldValue, SimulationEventType.PostCalculation);
        }
    }

    public event EventHandler<PropertyValueChangedEventArgs<T>>? PropertyChanged;
    public event EventHandler<PropertyValueChangedEventArgs<T1>>? FeedbackPropertyChanged;

    public void SetParameter(object parameter)
    {
        if (parameter is T value)
            _value = value;
    }
    public void SetValue(T? newValue, SimulationEventType type = SimulationEventType.DiscreteCalculation)
    {
        T? oldValue = _value;
        _value = newValue;
        ValueChanged = true;
        OnPropertyChanged(newValue, oldValue, type);
    }
    public void SetFeedbackValue(T1? newValue, SimulationEventType type = SimulationEventType.PostCalculation)
    {
        T1? oldValue = _feedbackValue;
        _feedbackValue = newValue;
        FeedbackValueChanged = true;
        OnFeedbackValueChanged(newValue, oldValue, type);
    }
    /// <summary>
    /// Set a simulation value without creating a event.
    /// This should only be used in special situations.
    /// </summary>
    /// <param name="newValue"></param>
    public void SetValueNoEvent(T? newValue)
    {
        T? oldValue = _value;
        _value = newValue;
        ValueChanged = true;
    }

    protected void OnPropertyChanged(T? newValue, T? oldValue, SimulationEventType type)
    {
        PropertyChanged?.Invoke(this, new PropertyValueChangedEventArgs<T>(newValue, oldValue, Parent.Solver, type));
    }
    protected void OnFeedbackValueChanged(T1? newValue, T1? oldValue, SimulationEventType type)
    {
        FeedbackPropertyChanged?.Invoke(this, new PropertyValueChangedEventArgs<T1>(newValue, oldValue, Parent.Solver, type));
    }

}
