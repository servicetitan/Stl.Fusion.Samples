import React from "react";
import ReactDOM from "react-dom";
import "./tailwind.min.css";
import App from "./App";
import { configure } from "./lib/Fusion";

configure({ uri: "ws://localhost:5005/fusion/ws", options: { wait: 600 } });

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById("root")
);
