import { render } from "solid-js/web";
import App from "./App";
import "./styles/sizes.css";
import "./styles/badges.css";
import { ThemeProvider } from "components";
let dispose;
export async function mount(container, env) {
    dispose = render(() => (<ThemeProvider colorScheme={"light"} theme={"rose"}>
        <App env={env}/>
      </ThemeProvider>), container);
}
export async function unmount(container) {
    dispose?.();
    container.innerHTML = "";
}
