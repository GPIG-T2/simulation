import { ClientInterface } from "./interface.js";
import { HeatMap, LineChart } from "./graphing.js";
import { statesConfig, infectedConfig } from "./chart_config.js";

const maxReconnections = 5;
const totalsKeys = [
  "uninfected", // 0
  "asymptomaticInfectedNotInfectious", // 1
  "asymptomaticInfectedInfectious", // 2
  "symptomatic", // 3
  "seriousInfection", // 4
  "dead", // 5
  "recoveredImmune", // 6
];

async function main() {
  const heatMap = await HeatMap.create();
  const stateChart = new LineChart("states", statesConfig);
  const infChart = new LineChart("infected", infectedConfig);

  let reconnections = 0;
  const client = new ClientInterface();
  await client.ready;

  const statusText = document.getElementById("status");
  statusText.innerText = "Connected";

  const deathText = document.getElementById("death");
  const setDeathText = (t) =>
    (deathText.innerText = `Total Deaths: ${t.toLocaleString()}`);
  setDeathText(0);

  client.onclose = () => {
    statusText.innerText = "Disconnected";

    if (reconnections < maxReconnections) {
      setTimeout(() => client.reconnect(), 1000);
      reconnections++;
    }
  };

  const settings = await client.getSettings();
  console.log(settings);

  client.onsnapshot = (snapshot) => {
    const data = snapshot.map((t, i) => ({
      inf:
        t.asymptomaticInfectedNotInfectious +
        t.asymptomaticInfectedInfectious +
        t.symptomatic +
        t.seriousInfection,
      pop:
        t.uninfected +
        t.asymptomaticInfectedNotInfectious +
        t.asymptomaticInfectedInfectious +
        t.symptomatic +
        t.seriousInfection +
        t.recoveredImmune,
      x: settings.locations[i]._position.x,
      y: settings.locations[i]._position.y,
    }));

    heatMap.setData(data);

    const totals = snapshot.reduce((p, c) => {
      for (const key of totalsKeys) {
        p[key] += c[key];
      }
      return p;
    }, emptyTotals());

    stateChart.addData(totalsKeys.map((k) => totals[k]));
    setDeathText(totals.dead);

    const infected = snapshot.reduce(
      (p, c) =>
        p +
        (c.asymptomaticInfectedNotInfectious +
          c.asymptomaticInfectedInfectious +
          c.symptomatic +
          c.seriousInfection >
        0
          ? 1
          : 0),
      0
    );

    infChart.addData([infected]);
  };
}

/** @returns {import("./interface.js").Totals} */
function emptyTotals() {
  const obj = {};
  for (const key of totalsKeys) {
    obj[key] = 0;
  }
  return obj;
}

main();
