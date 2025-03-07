using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easy2Sim.Interfaces;
using Newtonsoft.Json;

namespace Easy2Sim.Solvers
{
    public class DateTimeConfigurations : IFrameworkBase
    {
        /// <summary>
        /// Unique Guid that can be used to uniquely identify class instances
        /// </summary>
        [JsonIgnore]
        public Guid Guid { get; set; }

        /// <summary>
        /// Start date of the simulation
        /// </summary>
        [JsonProperty]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the simulation
        /// </summary>
        [JsonProperty]
        public DateTime EndDate { get; set; }

        [JsonIgnore]
        public long NecessarySimulationSteps
        {
            get
            {
                long milliseconds = (long)(EndDate - StartDate).TotalMilliseconds;
                long simulationSteps = milliseconds / TimeFactor;
                return simulationSteps;
            }
        }

        /// <summary>
        /// Date increase in milliseconds, per simulation step
        /// </summary>
        [JsonProperty]
        public int TimeFactor { get; set; }


        internal DateTime GetCurrentSimulationDate(long simulationTime)
        {
            DateTime result = StartDate.AddMilliseconds(simulationTime * TimeFactor);


            return result;
        }

        public DateTimeConfigurations()
        {
            Guid = Guid.NewGuid();
            TimeFactor = 1000 * 60; //
            StartDate = new DateTime(2024, 1, 1);
        }
    }
}
