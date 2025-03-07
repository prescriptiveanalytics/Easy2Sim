using Easy2Sim.Environment;

namespace Easy2Sim.Interfaces
{
    public interface IConnection : IFrameworkBase
    {
        public Guid EnvironmentGuid { get; set; }
        public SimulationBase? SourceObject { get; }
        public SimulationBase? TargetObject { get; }
        public bool IsComponentConnection { get; }
        public void Reapply();
    }
}
