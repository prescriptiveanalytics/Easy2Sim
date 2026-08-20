using System.Text;
using Easy2Sim.Environment;
using Easy2Sim.Interfaces;
using Newtonsoft.Json;

namespace Easy2Sim.Connect;

public class SimulationValue<T> : IFrameworkBase, ISimulationValue, ICloneable<SimulationValue<T>>
{
    [JsonProperty]
    public Guid Guid { get; }

    [JsonIgnore]
    public SimulationEnvironment? SimulationEnvironment { get; set; }

    [JsonIgnore]
    public SimulationBase? Parent
    {
        get
        {
            SimulationBase? simBase = SimulationEnvironment?.GetComponentByName(ParentName);
            if (simBase == null)
            {
                SimulationEnvironment?.LogEnvironmentError($"Simulation value ({ParentName}-{PropertyName} parent not found");
            }
            return simBase;
        }
    }

    [JsonProperty]
    public string PropertyName { get; set; }
    [JsonProperty]
    public string ParentName { get; set; }

    [JsonProperty]
    public List<SimulationValueAttributes> Attributes { get; set; }

    //DO NOT RENAME
    //is used with dynamic objects in the framework
    [JsonProperty]
    public bool ValueChanged { get; set; }
    [JsonProperty]
    private T? _value;

    [JsonConstructor]
    public SimulationValue()
    {
        Guid = Guid.NewGuid();
        PropertyName = string.Empty;
        ParentName = string.Empty;
        Attributes = new List<SimulationValueAttributes>();
        SimulationEnvironment = null;
    }
    public SimulationValue(T value, string propertyName, SimulationBase simBase, List<SimulationValueAttributes> attributes)
    {
        Attributes = attributes;
        PropertyName = propertyName;
        ParentName = simBase.Easy2SimName;
        _value = value;
        Guid = Guid.NewGuid();
        SimulationEnvironment = simBase.SimulationEnvironment;
    }
    public SimulationValue(T value, string propertyName, SimulationBase simBase, SimulationValueAttributes attribute)
    {
        Attributes = new List<SimulationValueAttributes>() { attribute };
        PropertyName = propertyName;
        ParentName = simBase.Easy2SimName;
        _value = value;
        Guid = Guid.NewGuid();
        SimulationEnvironment = simBase.SimulationEnvironment;
    }

    // Event to be raised when the attribute changes
    public T? GetValue()
    {
        return _value;
    }


    //DO NOT RENAME
    //is used with dynamic objects in the framework

    /// <summary>
    /// In case the property is used, a discrete calculation event is created
    /// </summary>
    [JsonProperty]
    public T? Value
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

    public event EventHandler<PropertyValueChangedEventArgs<T>>? PropertyChanged;

    public void SetParameter(object parameter)
    {
        if (parameter is T value)
            _value = value;
    }

    /// <summary>
    /// Set a value in the simulation and specify what type of event should be created
    /// </summary>
    /// <param name="newValue"> New value of the connection</param>
    /// <param name="type">What type of event is set</param>
    public void SetValue(T? newValue, SimulationEventType type = SimulationEventType.DiscreteCalculation)
    {
        T? oldValue = _value;

        _value = newValue;
        ValueChanged = true;
        OnPropertyChanged(newValue, oldValue, type);

    }

    public void OnPropertyChanged(T? newValue, T? oldValue, SimulationEventType type)
    {
        if (Parent?.Solver == null)
        {
            return;
        }
        PropertyChanged?.Invoke(this, new PropertyValueChangedEventArgs<T>(newValue, oldValue, Parent.Solver, type));
    }


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{ParentName}\\{PropertyName}: ");
        if (_value != null)
            sb.Append(_value.ToString());
        return sb.ToString();
    }
    [JsonIgnore]
    public Type GenericType => typeof(T);


    public SimulationValue<T> Clone()
    {
        SimulationValue<T> result = new SimulationValue<T>();
        result.PropertyName = PropertyName;
        result.ParentName = ParentName;
        result.Attributes.AddRange(Attributes);
        result.Value = Value;
        result.ValueChanged = ValueChanged;
        return result;
    }


}
