namespace Easy2Sim.Interfaces;

//Clone a object ignoring Simulation Environment and Solver
public interface ICloneable<T>
{
    T Clone();
}