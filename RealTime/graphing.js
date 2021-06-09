import * as d3 from "https://cdn.skypack.dev/d3?dts";
import { Chart, registerables } from "https://cdn.skypack.dev/chart.js?dts";

Chart.register(...registerables);

const geoDataSource =
  "https://raw.githubusercontent.com/holtzy/D3-graph-gallery/master/DATA/world.geojson";

export class HeatMap {
  static async create() {
    const geoData = await d3.json(geoDataSource);
    return new HeatMap(geoData);
  }

  constructor(geoData) {
    this.geoData = geoData;
    this.svg = d3.select("svg");
    this.width = +this.svg.attr("width");
    this.height = +this.svg.attr("height");

    this.projection = d3
      .geoMercator()
      .center([0, 20])
      .scale(99)
      .translate([this.width / 2, this.height / 2]);

    // Add a scale for bubble size
    this.circleSize = d3
      .scaleLog()
      .domain([1, 10_000_000]) // What's in the data
      .clamp(true)
      .range([1, 25]); // Size in pixel

    this.colorScale = d3
      .scaleThreshold()
      .domain([100, 100_000])
      .range(d3.schemeReds[7]);

    this.#initMap();
  }

  setData(data) {
    const circles = this.svg.selectAll(".infection-circle").data(data);

    circles
      .enter()
      .append("circle")
      .attr("class", "infection-circle")
      .merge(circles)
      .transition()
      .duration(100)
      .attr("cx", (d) => this.projection([d.x, d.y])[0])
      .attr("cy", (d) => this.projection([d.x, d.y])[1])
      .attr("r", (d) => this.circleSize(d.inf))
      .style("fill", (d) => this.colorScale(d.inf))
      .attr("stroke", (d) => (d.n > 2000 ? "black" : "none"))
      .attr("stroke-width", 1)
      .attr("fill-opacity", 0.4);
  }

  #initMap() {
    // Draw the map
    this.svg
      .append("g")
      .selectAll("path")
      .data(this.geoData.features)
      .enter()
      .append("path")
      .attr("fill", "#b8b8b8")
      .attr("d", d3.geoPath().projection(this.projection))
      .style("stroke", "none")
      .style("opacity", 0.3);

    // --------------- //
    // ADD LEGEND //
    // --------------- //

    // Add legend: circles
    const valuesToShow = [100, 10_000, 1_000_000];
    const xCircle = 40;
    const xLabel = 90;
    this.svg
      .selectAll("legend")
      .data(valuesToShow)
      .enter()
      .append("circle")
      .attr("cx", xCircle)
      .attr("cy", (d) => this.height - this.circleSize(d))
      .attr("r", (d) => this.circleSize(d))
      .style("fill", "none")
      .attr("stroke", "black");

    // Add legend: segments
    this.svg
      .selectAll("legend")
      .data(valuesToShow)
      .enter()
      .append("line")
      .attr("x1", (d) => xCircle + this.circleSize(d))
      .attr("x2", xLabel)
      .attr("y1", (d) => this.height - this.circleSize(d))
      .attr("y2", (d) => this.height - this.circleSize(d))
      .attr("stroke", "black")
      .style("stroke-dasharray", "2,2");

    // Add legend: labels
    this.svg
      .selectAll("legend")
      .data(valuesToShow)
      .enter()
      .append("text")
      .attr("x", xLabel)
      .attr("y", (d) => this.height - this.circleSize(d))
      .text((d) => d)
      .style("font-size", 10)
      .attr("alignment-baseline", "middle");
  }
}

export class LineChart {
  #chart;

  constructor(id, config) {
    const el = document.getElementById(id);
    this.#chart = new Chart(el, config);
  }

  addData(data) {
    for (let i = 0; i < data.length; i++) {
      this.#chart.data.datasets[i].data.push(data[i]);
    }
    this.#chart.update();
  }
}
