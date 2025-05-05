# Using Easy2Sim

!!! note annotate
    Make sure that you know which simulation type should be implemented before you continue. (1)

1. Simulation types can be e.g. discrete event simulation or dynamic simulation

To run a simulation this framework uses two key components:

- **Simulation environment**
	- Components
	- Links
	- Logging (Serilog)
- Simulation specific **Solver**
	- How are the connections resolved?
	- How often are components called 

When creating a new component, it registers in the simulation environment.

## Running a simulation

To start a simulation the steps are as follows:

1. Implement simulation library
2. Instantiate simulation environment and solver
3. Add components to environment
4. Add connections to the environment
5. Run the simulation 


## Implement a sine simulation component and visualize in xaml


[Sine code example](./Files/Sine.zip)
	
Visualization of random values and the Sine (Sine uses code from above):

![type:video](./Videos/cropped.mp4)