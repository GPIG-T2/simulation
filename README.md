# Contagion and Counter-Contagion Simulation

This repository contains the code that was build for our 2020/21 GPIG project. It provides a full pandemic simulation platform with a model World Health Organisation designed to counter it.

The project is built on .NET 5 with supporting scripts in Python 3.

## Virus

The contagion model is provided under [`Virus/`](Virus/). This model is based on a node-graph, with each node representing a location on the map. It also contains a WebSocket server in order to enable the WHO model to connect and interact with the real-time data.

You will need to provide a world file to run the virus. 3 are provided in this repository under [`WorldFiles/`](WorldFiles/). The selected world file path can be given an a command line argument or typed in at run-time.

To run, you can use the following command:

```
dotnet run --project Virus -- [world file path]
```

Unit tests are in the `Virus.Test` project, and the REST-WebSocket translation layer is under `Virus.Rest`.

When the model finishes a run, the data for that run is output to `tmp/run/`. This data can be graphed using:

```
python3 generate_run_graphs.py [UK,Europe,Earth]
```

The command line argument must be one of exactly `UK`, `Europe` or `Earth`. This is used to load node names for the figures, and should match the input world file of the run.

## WHO

The counter-contagion models are provided under [`WHO/`](WHO/). 2 models are provided, one based on trigger-events and the other on a simple threshold-lockdown system. These models are also capable of connecting to the server instance via either WebSockets or REST, depending on what is available.

To run, you can use the following command:

```
dotnet run --project WHO
```

To run with the simple model:

```
dotnet run --project WHO -- --simple
```

Unit test are provided within this project.

## Unit Tests

Various unit test are provided for both the Virus and WHO projects. To run these, you can use the following command:

```
dotnet test
```

These tests will also output data for various isolated virus runs under `tmp/`. This data can automatically be graphed with `generate_graphs.py`, which outputs `.png` figures in the relevant directories.

## Run Example: Earth

These are the steps needed to run the simulation end-to-end with the Earth data file. This assumes that you have a terminal open in the root directory of this repository.

1. Build the project

```
dotnet build
```

2. Start up the virus model

```
dotnet run --project Virus -- WorldFiles/Earth.json
```

3. In another terminal (with the same working directory), start up the WHO model

```
dotnet run --project WHO
```

4. Once the simulation has finished, generate the graphs

```
python3 generate_run_graphs.py Earth
```
