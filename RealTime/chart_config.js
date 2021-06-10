const labels = [];
for (let i = 0; i <= 365; i++) labels.push(i);

export const statesConfig = {
  type: "line",
  data: {
    labels,
    datasets: [
      {
        label: "Uninfected",
        data: [],
        fill: false,
        borderColor: "rgb(46, 125, 181)",
      },
      {
        label: "Asymptomatic Not Infectious",
        data: [],
        fill: false,
        borderColor: "rgb(253, 133, 49)",
      },
      {
        label: "Asymptomatic Infectious",
        data: [],
        fill: false,
        borderColor: "rgb(0, 154, 0)",
      },
      {
        label: "Symptomatic",
        data: [],
        fill: false,
        borderColor: "rgb(210, 0, 16)",
      },
      {
        label: "Serious Infection",
        data: [],
        fill: false,
        borderColor: "rgb(153, 144, 190)",
      },
      {
        label: "Dead",
        data: [],
        fill: false,
        borderColor: "rgb(162, 121, 112)",
      },
      {
        label: "Recovered Immune",
        data: [],
        fill: false,
        borderColor: "rgb(225, 122, 193)",
      },
    ],
  },
  options: {
    elements: {
      point: {
        radius: 0,
      },
    },
    animation: {
      duration: 0,
    },
    scales: {
      y: {
        type: "linear",
        min: 0,
        max: 7_000_000_000,
      },
    },
    plugins: {
      title: {
        display: true,
        text: "World Infection States",
      },
    },
  },
};

export const infectedConfig = {
  type: "line",
  data: {
    labels,
    datasets: [
      {
        label: "Countries Infected",
        data: [],
        fill: false,
        borderColor: "rgb(46, 125, 181)",
      },
    ],
  },
  options: {
    elements: {
      point: {
        radius: 0,
      },
    },
    animation: {
      duration: 0,
    },
    scales: {
      y: {
        type: "linear",
        min: 0,
        max: 64,
      },
    },
  },
};
