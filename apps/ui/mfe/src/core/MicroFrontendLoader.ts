import type {
  MicroFrontendModule,
  MicroFrontendEnv,
  LoadedComponent,
} from "./MicroFrontendTypes.ts";
import { eventBus } from "./EventBus";
import { globalContext } from "./GlobalContext";
import type { Component } from "solid-js";

const RUNTIME_VERSION = "1.0.0";

// Cache for loaded modules
const moduleCache = new Map<string, MicroFrontendModule>();

// Cache for loaded components
const componentCache = new Map<string, Map<string, LoadedComponent<any>>>();

export async function loadMicroFrontend(url: string): Promise<MicroFrontendModule> {
  // Check cache first
  if (moduleCache.has(url)) {
    return moduleCache.get(url)!;
  }

  try {
    const module = await import(url);

    if (typeof module.mount !== "function") {
      throw new Error(`Module at ${url} does not export 'mount' function`);
    }

    if (typeof module.unmount !== "function") {
      throw new Error(`Module at ${url} does not export 'unmount' function`);
    }

    const loadedModule = module as MicroFrontendModule;
    moduleCache.set(url, loadedModule);
    return loadedModule;
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

// Load a specific component from a microfrontend module
export async function loadComponentFromModule<T extends Record<string, any>>(
  url: string,
  componentName: string
): Promise<LoadedComponent<T>> {
  // Check component cache first
  if (componentCache.has(url)) {
    const urlComponents = componentCache.get(url)!;
    if (urlComponents.has(componentName)) {
      return urlComponents.get(componentName)!;
    }
  }

  try {
    const module = await import(/* @vite-ignore */ url);

    if (!module[componentName]) {
      throw new Error(
        `Module at ${url} does not export component '${componentName}'`
      );
    }

    const component = module[componentName] as Component<T>;
    
    // Create component entry if it doesn't exist
    if (!componentCache.has(url)) {
      componentCache.set(url, new Map());
    }
    
    const loadedComponent = {
      component,
    } as LoadedComponent<T>;
    
    componentCache.get(url)!.set(componentName, loadedComponent);
    
    return loadedComponent;
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : String(error);
    const fullError = new Error(
      `Failed to load component '${componentName}' from ${url}: ${errorMessage}`
    );
    console.error(fullError);
    throw fullError;
  }
}

// Clear caches (useful for hot reloading)
export function clearCaches(): void {
  moduleCache.clear();
  componentCache.clear();
}
