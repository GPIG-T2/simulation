#!/usr/bin/env python3
import json
import math
import sys
from pathlib import Path
from typing import Union

import matplotlib.pyplot as plt
import numpy as np

TMP_DIR = Path("tmp").resolve()


def load_node_names(path: Path):
    with open(path, "r") as fp:
        return list(map(lambda n: n["name"], json.load(fp)["nodes"]))


class Plotter:
    def __init__(self, world: str, src_dir: Union[str, None] = None):
        self.world = Path("WorldFiles/{0}.json".format(world))

        self.base = TMP_DIR.joinpath(world if src_dir is None else src_dir)
        self.csv = self.base.joinpath("agg.csv")
        self.png = self.base.joinpath("agg.png")

        self.node_names = load_node_names(self.world)
        self.png_nodes = self.base.joinpath("nodes.png")

    def csv_node(self, i: int) -> Path:
        return self.base.joinpath("node_{0}.csv".format(i))

    def plot(self):
        self.plot_agg()
        self.plot_nodes()

    def plot_agg(self):
        data: np.ndarray = np.genfromtxt(self.csv, delimiter=",")

        nodes = data[:, 0]
        inf_data = data[:, 1:]

        _, (ax1, ax2) = plt.subplots(2, 1, figsize=(15, 10))

        self.plot_node_data(inf_data, ax1)

        ax2.title.set_text("Infected Nodes")
        ax2.plot(nodes, label="Nodes")
        ax2.legend()

        plt.tight_layout()
        plt.savefig(self.png, dpi=175)
        print("> {0}".format(self.png))

    def plot_nodes(self):
        sqr = math.sqrt(len(self.node_names))
        width, height = math.floor(sqr), math.ceil(sqr + 0.5)

        _, axs = plt.subplots(height, width, figsize=(15 * width, 5 * height))
        axs = axs.flatten()

        for i, name in enumerate(self.node_names):
            data: np.ndarray = np.genfromtxt(self.csv_node(i), delimiter=",")
            self.plot_node_data(data, axs[i], " for {}".format(name))

        plt.tight_layout()
        plt.savefig(self.png_nodes, dpi=175)
        print("> {0}".format(self.png_nodes))

    def plot_node_data(self, data: np.ndarray, ax: plt.Axes, title_end: Union[str, None] = None):
        uninf = data[:, 0]
        asymp_not_inf = data[:, 1]
        asymp_inf = data[:, 2]
        symp = data[:, 3]
        serious = data[:, 4]
        dead = data[:, 5]
        recovered = data[:, 6]

        ax.title.set_text("Infection States" +
                          (title_end if title_end != None else ""))
        ax.plot(uninf, label="Uninfected")
        ax.plot(asymp_not_inf, label="Asymptomatic Not Infectious")
        ax.plot(asymp_inf, label="Asymptomatic Infectious")
        ax.plot(symp, label="Symptomatic")
        ax.plot(serious, label="Serious Infection")
        ax.plot(dead, label="Dead")
        ax.plot(recovered, label="Recovered")
        ax.legend()


def main():
    plt.rc('axes', labelsize=10)
    plt.rc('xtick', labelsize=10)
    plt.rc('ytick', labelsize=10)
    plt.rc('legend', fontsize=10)

    plotters = [
        Plotter("UK"),
        Plotter("Europe"),
        Plotter("Earth"),
        Plotter("UK", "UK-movement-restrictions"),
        Plotter("UK", "UK-stay-at-home")
    ]

    for plotter in plotters:
        plotter.plot()
        print("Graphs for {0} generated".format(plotter.base))

    print("Generated all graphs")

    return 0


if __name__ == "__main__":
    sys.exit(main())
