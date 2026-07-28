import type { MicroFrontendEnvironment } from "./types/MicroFrontendEnvironment.ts";
import "./styles/sizes.css";
import "./styles/badges.css";
export declare function mount(container: HTMLElement, env: MicroFrontendEnvironment): Promise<void>;
export declare function unmount(container: HTMLElement): Promise<void>;
