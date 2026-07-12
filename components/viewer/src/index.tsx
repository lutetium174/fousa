/* @refresh reload */
import { render } from "solid-js/web";
import "./index.css";
import App from "./App";
import { ThemeProvider, ThemeSwitcher } from "components";

const root = document.getElementById("root");

render(
  () => (
    <ThemeProvider>
      <ThemeSwitcher />
      <App />
    </ThemeProvider>
  ),
  root,
);
