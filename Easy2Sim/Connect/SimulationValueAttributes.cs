namespace Easy2Sim.Connect;

public enum SimulationValueAttributes
{
    Input, //If a component receives information
    Output, //A component sets information to a follow up component
    Parameter, //Should be a parameter for the simulation, e.g. should be set from excel
    Visualization, //Updated multiple times during a simulation run
    VisualizationOnChange, //Updated when changed in the simulation
    VisualizationInitialize //Value for the visualization, pushed once at the start of a simulation during the initialization
}
