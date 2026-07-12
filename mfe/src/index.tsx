/* @refresh reload */
import { render } from "solid-js/web";
import "./index.css";
import "./styles/tokens.css";
import App from "./App.tsx";
import { ThemeProvider } from "components";
import { RouteProvider } from "./contexts/RouteContext.tsx";

const root = document.getElementById("root");

render(
  () => (
    <ThemeProvider>
      <RouteProvider>
        <App />
      </RouteProvider>
    </ThemeProvider>
  ),
  root!,
);
