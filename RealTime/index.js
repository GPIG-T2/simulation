import { ClientInterface } from "./interface.js";
import { HeatMap } from "./graphing.js";

async function main() {
  const heatMap = await HeatMap.create();

  const client = new ClientInterface();
  await client.ready;

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
    console.log("updated to", data);
  };
}

main();
