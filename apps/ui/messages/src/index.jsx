import { render } from "solid-js/web";
import App from "./App";
import "./styles/sizes.css";
import "./styles/badges.css";
let dispose;
export async function mount(container, env) {
    dispose = render(() => <App env={env}/>, container);
}
export async function unmount(container) {
    dispose?.();
    container.innerHTML = "";
}
