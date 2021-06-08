#!/usr/bin/env python3
import sys
from typing import List

from generate_graphs import Plotter


def main(args: List[str]) -> int:
    if len(args) < 2:
        print("Have to give UK, Europe or Earth as an argument")
        return 1

    world = args[1]

    if world not in ["UK", "Europe", "Earth"]:
        print("Have to give UK, Europe or Earth as an argument")
        return 1

    plotter = Plotter(world, "run")
    plotter.plot()

    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
