#!/usr/bin/env python3
import json
import math
import sys
from pathlib import Path
from typing import Union

import matplotlib.pyplot as plt
import numpy as np

TMP_DIR = Path("tmp").resolve()
UK_DATA = TMP_DIR.joinpath("uk.csv")
UK_PNG = TMP_DIR.joinpath("uk.png")
UK_NODE_DATA_FMT = "uk_node_{0}.csv"
UK_NODE_PNG = TMP_DIR.joinpath("uk_nodes.png")


def load_json(path: Path):
    with open(path, "r") as fp:
        return json.load(fp)


UK_WORLD_JSON = Path("WorldFiles/UK.json")
WORLD = load_json(UK_WORLD_JSON)
UK_NODE_COUNT = len(WORLD["nodes"])
UK_NODES = []

for i in range(UK_NODE_COUNT):
    UK_NODES.append(TMP_DIR.joinpath(UK_NODE_DATA_FMT.format(i)))


def plot_node_data(data: np.ndarray, ax: plt.Axes, title_end: Union[str, None] = None):
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


def plot_agg():
    data: np.ndarray = np.genfromtxt(UK_DATA, delimiter=",")

    nodes = data[:, 0]
    inf_data = data[:, 1:]

    _, (ax1, ax2) = plt.subplots(2, 1, figsize=(15, 10))

    plot_node_data(inf_data, ax1)

    ax2.title.set_text("Infected Nodes")
    ax2.plot(nodes, label="Nodes")
    ax2.legend()

    plt.tight_layout()
    plt.savefig(UK_PNG, dpi=175)


def plot_nodes():
    width, height = UK_NODE_COUNT // 4, math.ceil(float(UK_NODE_COUNT) / 3)

    _, axs = plt.subplots(height, width, figsize=(15 * width, 5 * height))
    axs = axs.flatten()

    for i in range(UK_NODE_COUNT):
        data: np.ndarray = np.genfromtxt(UK_NODES[i], delimiter=",")
        plot_node_data(data, axs[i], " For Node {}".format(i))

    plt.tight_layout()
    plt.savefig(UK_NODE_PNG, dpi=175)


def main():
    plt.rc('axes', labelsize=10)
    plt.rc('xtick', labelsize=10)
    plt.rc('ytick', labelsize=10)
    plt.rc('legend', fontsize=10)

    plot_agg()
    plot_nodes()

    return 0


if __name__ == "__main__":
    sys.exit(main())
