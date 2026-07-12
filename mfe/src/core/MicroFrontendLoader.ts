import type {MicroFrontendModule, MicroFrontendEnv} from "./MicroFrontendTypes.ts";
import {eventBus} from "./EventBus";
import {globalContext} from "./GlobalContext";

const RUNTIME_VERSION = "1.0.0";

export async function loadMicroFrontend(url: string): Promise<MicroFrontendModule> {
  try {
    const module = await import(url);

    if (typeof module.mount !== "function") {
      throw new Error(`Module at ${url} does not export 'mount' function`);
    }

    if (typeof module.unmount !== "function") {
      throw new Error(`Module at ${url} does not export 'unmount' function`);
    }

    return module as MicroFrontendModule;
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : String(error);
    const fullError = new Error(
      `Failed to load module from ${url}: ${errorMessage}`
    );
    console.error(fullError);
    throw fullError;
  }
}

export async function mountMicroFrontend(
  url: string,
  container: HTMLElement,
  basePath: string,
  routeParams?: Record<string, string>
): Promise<() => void> {
  const module = await loadMicroFrontend(url);

  const env: MicroFrontendEnv = {
    eventBus,
    globalContext,
    basePath,
    routeParams,
    runtimeVersion: RUNTIME_VERSION,
  };

  await module.mount(container, env);

  return async () => {
    try {
      await module.unmount(container);
    } catch (error) {
      console.error(`Error unmounting module from ${url}:`, error);
    }
  };
}
