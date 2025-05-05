## What is a simulation component?
A simulation component describes a logical part of a system that should be modeled.
An example would be a model of a  flow shop. This model can have simulation components for:

  - Machines 
  - Vehicles
  - Operators

And each simulation component has attributes that describe its current state.
A machine can for example have:

  - Unique Id: Unique identifier of the machine
  - Processing Time: Time it takes to do one task at the machine
  - Setup time: Time it takes to swap the tool of the machine
  - Energy consumption: How much energy is consumed while idle/producing?
  - Buffer Size: Size of the buffer for raw material that is used at this machine

## What is a connection in the simulation?
A connection describes a information flow between components in the simulation.
E.g. a machine could inform a vehicle that it needs more material for further production or 
a machine finishes a production and informs the next machine.

Another example is a component InputParser that parses sensor data.
The parsed results can be provided to other components via a connection.