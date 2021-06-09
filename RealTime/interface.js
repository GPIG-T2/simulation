/**
 * @typedef {object} Totals
 * @prop {number} uninfected
 * @prop {number} asymptomaticInfectedNotInfectious
 * @prop {number} asymptomaticInfectedInfectious
 * @prop {number} symptomatic
 * @prop {number} seriousInfection
 * @prop {number} dead
 * @prop {number} recoveredImmune
 */

/** */
export class ClientInterface {
  #ws;
  #id = 0;
  #msgs = new Map();
  #ready;

  /** @type {((snapshot: Totals[]) => void) | undefined} */
  onsnapshot;
  /** @type {(() => void) | undefined} */
  onclose;

  constructor() {
    this.#ws = this.#connect();
  }

  get ready() {
    return this.#ready;
  }

  getSettings() {
    return new Promise((resolve, reject) => {
      const id = this.#id++;
      const request = {
        id,
        endpoint: "/settings",
        method: 0,
      };

      this.#ws.send(JSON.stringify(request));
      this.#msgs.set(id, [resolve, reject]);
    });
  }

  reconnect() {
    this.#ws.close();
    this.#ws = this.#connect();

    return this.#ready;
  }

  #connect() {
    const ws = new WebSocket("ws://127.0.0.1");
    ws.addEventListener("message", (ev) => this.#handleMessage(ev));
    ws.addEventListener("close", () => this.onclose?.());

    this.#ready = new Promise((resolve) => {
      const handler = () => {
        resolve();
        ws.removeEventListener("open", handler);
      };

      ws.addEventListener("open", handler);
    });

    return ws;
  }

  /**
   * @param {MessageEvent} ev
   */
  #handleMessage(ev) {
    /**
     * @type {{id?: number; status: number; message: string;}}
     */
    const response = JSON.parse(ev.data);

    if (response.id == null) {
      switch (response.status) {
        case -1:
          {
            const snapshot = JSON.parse(response.message);
            this.onsnapshot?.(snapshot);
          }
          break;
      }
    } else {
      const [resolve, reject] = this.#msgs.get(response.id);

      if (response.status !== 200) {
        reject(response.message);
      } else {
        resolve(JSON.parse(response.message));
      }
    }
  }
}
