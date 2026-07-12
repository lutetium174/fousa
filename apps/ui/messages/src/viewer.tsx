import { render } from "solid-js/web";
import App from "./App";
import type { MicroFrontendEnvironment } from "./types/MicroFrontendEnvironment.ts";
import "./styles/sizes.css";
import "./styles/badges.css";
import { ThemeProvider } from "components";

let dispose: (() => void) | undefined;

export async function mount(
  container: HTMLElement,
  env: MicroFrontendEnvironment,
) {
  dispose = render(
    () => (
      <ThemeProvider colorScheme={"light"} theme={"rose"}>
        <App env={env} />
      </ThemeProvider>
    ),
    container,
  );
}

export async function unmount(container: HTMLElement) {
  dispose?.();
  container.innerHTML = "";
}
