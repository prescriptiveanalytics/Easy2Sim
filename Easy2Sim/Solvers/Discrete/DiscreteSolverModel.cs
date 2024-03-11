using Easy2Sim.Environment;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers.Discrete
{
    /// <summary>
    /// Holds all data for a discrete simulation.
    /// </summary>
    public class DiscreteSolverModel : IFrameworkBase
    {
        /// <summary>
        /// Can the a component be added multiple times at the same simulation time
        /// </summary>
        [JsonProperty]
        public bool AllowLoops { get; set; }


        /// <summary>
        /// List of all current events
        /// </summary>
        [JsonProperty]
        public List<DiscreteEvent> EventList { get; set; }


        public void AddEvent(long timeStamp, SimulationBase simBase)
        {
            DiscreteEvent result = new DiscreteEvent();
            result.TimeStamp = timeStamp;
            result.ComponentName = simBase.Name;
            result.TimeStampIndex = simBase.Index +
                                    HistoricEvents.Where(x => x.TimeStamp == timeStamp)
                                        .Count(x => x.ComponentName == simBase.Name) *
                                    simBase.SimulationEnvironment.Model.SimulationIndex;
            if (AllowLoops)
            {
                EventList.Add(result);
            }
            else
            {
                //No loops allowed, check if the event exists already
                if (!HistoricEvents.Any(x => (x.TimeStamp == timeStamp) && (x.ComponentName == simBase.Name)))
                {
                    EventList.Add(result);
                }
            }
        }


        /// <summary>
        /// Keep track of historic events
        /// </summary>
        [JsonProperty]
        public List<DiscreteEvent> HistoricEvents { get; set; }



        [JsonProperty]
        public Guid Guid { get; set; }

        /// <summary>
        /// Constructor that is used for serialization.
        /// Should not be used, as a environment guid is necessary.
        /// </summary>
        public DiscreteSolverModel()
        {
            HistoricEvents = new List<DiscreteEvent>();
            EventList = new List<DiscreteEvent>();
        }


        /// <summary>
        /// Serialize to json uses the default constructor.
        /// </summary>
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
