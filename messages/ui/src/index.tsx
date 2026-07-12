import { render } from 'solid-js/web'
import App  from './App'
import type {MicroFrontendEnvironment} from "./types/MicroFrontendEnvironment.ts";
import "./styles/sizes.css";
import "./styles/badges.css";

let dispose: (() => void) | undefined

export async function mount(container: HTMLElement, env: MicroFrontendEnvironment) {
  dispose = render(() => <App env={env} />, container)
}

export async function unmount(container: HTMLElement) {
  dispose?.()
  container.innerHTML = ''
}