#!/usr/bin/env deno run --allow-net
export {};

const ws = new WebSocket("ws://127.0.0.1");
ws.addEventListener("message", (ev) => console.log(ev));
