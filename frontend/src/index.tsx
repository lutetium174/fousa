/* @refresh reload */
import { render } from 'solid-js/web'
import type { MicrofrontendEnv } from '@root-container/core'
import App  from './App'

const root = document.getElementById('root')

render(() => <App />, root!)

let dispose: (() => void) | undefined

export async function mount(container: HTMLElement, env: MicrofrontendEnv) {
    dispose = render(() => <App env={env} />, container)
}

export async function unmount(container: HTMLElement) {
    if (dispose) dispose()
}