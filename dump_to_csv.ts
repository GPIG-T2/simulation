#!/usr/bin/env deno run --allow-read --allow-write
export {};

interface Totals {
  uninfected: number;
  asymptomaticInfectedNotInfectious: number;
  asymptomaticInfectedInfectious: number;
  symptomatic: number;
  seriousInfection: number;
  dead: number;
  recoveredImmune: number;
}

interface InfectionTotals extends Totals {
  location: [string];
}

interface Line extends Totals {
  nodesHit: number;
}

function defaultLine(): Line {
  return {
    nodesHit: 0,
    uninfected: 0,
    asymptomaticInfectedNotInfectious: 0,
    asymptomaticInfectedInfectious: 0,
    symptomatic: 0,
    seriousInfection: 0,
    dead: 0,
    recoveredImmune: 0,
  };
}

const infectionKeys = new Set([
  "asymptomaticInfectedNotInfectious",
  "asymptomaticInfectedInfectious",
  "symptomatic",
  "seriousInfection",
]);

const data: Totals[][] = JSON.parse(await Deno.readTextFile("tmp/dump.json"));

const lines = data
  .map((snap) => {
    const totals = defaultLine();

    for (const node of snap) {
      let hasInfection = false;

      for (const [key, value] of Object.entries(node).filter(
        ([key]) => key !== "location"
      )) {
        totals[key as keyof Totals] += value;

        if (infectionKeys.has(key) && value > 0) {
          hasInfection = true;
        }
      }

      if (hasInfection) {
        totals.nodesHit++;
      }
    }

    return [
      totals.nodesHit,
      totals.uninfected,
      totals.asymptomaticInfectedNotInfectious,
      totals.asymptomaticInfectedInfectious,
      totals.symptomatic,
      totals.seriousInfection,
      totals.dead,
      totals.recoveredImmune,
    ].join(",");
  })
  .join("\n");

await Deno.writeTextFile("tmp/run.csv", lines);
