#!/usr/bin/env python3
import sys
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np

UK_DATA = "tmp/uk.csv"
UK_PNG = "tmp/uk.png"


def main():
    path = Path(UK_DATA)

    if not path.is_file():
        print("UK data not available")
        return 1

    data: np.ndarray = np.genfromtxt(UK_DATA, delimiter=",")

    nodes = data[:, 0]
    uninf = data[:, 1]
    asymp_not_inf = data[:, 2]
    asymp_inf = data[:, 3]
    symp = data[:, 4]
    serious = data[:, 5]
    dead = data[:, 6]
    recovered = data[:, 7]

    plt.rc('axes', labelsize=10)
    plt.rc('xtick', labelsize=10)
    plt.rc('ytick', labelsize=10)
    plt.rc('legend', fontsize=10)

    _, (ax1, ax2) = plt.subplots(2, 1, figsize=(15, 10))

    ax1.title.set_text("Infection States")
    ax1.plot(uninf, label="Uninfected")
    ax1.plot(asymp_not_inf, label="Asymptomatic Not Infectious")
    ax1.plot(asymp_inf, label="Asymptomatic Infectious")
    ax1.plot(symp, label="Symptomatic")
    ax1.plot(serious, label="Serious Infection")
    ax1.plot(dead, label="Dead")
    ax1.plot(recovered, label="Recovered")
    ax1.legend()

    ax2.title.set_text("Infected Nodes")
    ax2.plot(nodes, label="Nodes")
    ax2.legend()

    plt.tight_layout()
    plt.savefig(UK_PNG, dpi=175)

    return 0


if __name__ == "__main__":
    sys.exit(main())
