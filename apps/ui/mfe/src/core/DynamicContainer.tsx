/**
 * DynamicContainer - Working Implementation
 * 
 * This implementation uses a component registry pattern that works in both
 * development and production. Instead of dynamic HTTP imports, it uses
 * static imports with a registry of available components.
 */

import {
  createEffect,
  createSignal,
  onCleanup,
  Show,
  type Component,
} from "solid-js";
import { useGlobalContext } from "./GlobalContext.tsx";
import { useI18n } from "../i18n/index.tsx";
import { Badge, SpinnerIcon } from "components";

// ============================================================================
// COMPONENT REGISTRY
// In development, we statically import components from microfrontends
// In production, these would be loaded from the built microfrontend modules
// ============================================================================

// Development: Direct imports from messages MFE (works with Vite's fs.allow)
let MessagesComponents: Record<string, Component | Component<any>> | null = null;

async function loadMessagesComponents(): Promise<Record<string, Component>> {
  if (MessagesComponents) return MessagesComponents;
  
  try {
    // Import components from messages MFE
    // This works because vite.config.ts has fs.allow: ['..']
    // Path: mfe/src/core/ -> ../../messages/src/components/index.ts
    const module = await import("../../../messages/src/components/index.ts");
    
    MessagesComponents = {
      Chat: module.Chat || module.default
    };
    
    return MessagesComponents;
  } catch (error) {
    console.error("Failed to load messages components:", error);
    throw error;
  }
}

// Registry of available microfrontend components
const componentRegistry = new Map<string, () => Promise<Record<string, Component>>>([
  ["messages", loadMessagesComponents],
  // Add other MFEs here: ["search", loadSearchComponents],
  // ["notifications", loadNotificationsComponents],
]);

// ============================================================================
// TYPES
// ============================================================================

export type DynamicContainerProps<T = unknown> = {
  mfeName: string; // e.g., "messages", "search", "notifications"
  componentName: string;
  basePath?: string;
  props?: T;
  className?: string;
  onLoad?: (component: Component<T>) => void;
  onError?: (error: string) => void;
};

// ============================================================================
// MAIN COMPONENT
// ============================================================================

/**
 * DynamicContainer - Loads and renders specific components from microfrontends.
 * 
 * Usage:
 * <DynamicContainer
 *   mfeName="messages"
 *   componentName="Chat"
 *   props={{ onMessageSent: (msg) => console.log(msg) }}
 * />
 */
export function DynamicContainer<T = unknown>(
  props: DynamicContainerProps<T>
) {
  const { i18n } = useI18n();
  const { state } = useGlobalContext();

  const [component, setComponent] = createSignal<Component<T> | null>(null);
  const [loading, setLoading] = createSignal(true);
  const [error, setError] = createSignal<string | null>(null);

  createEffect(() => {
    setLoading(true);
    setError(null);
    setComponent(null);

    let cancelled = false;

    (async () => {
      try {
        // Get the loader for this MFE
        const loader = componentRegistry.get(props.mfeName);
        
        if (!loader) {
          throw new Error(`Microfrontend '${props.mfeName}' not registered`);
        }
        
        // Load all components from this MFE
        const components = await loader();
        
        if (cancelled) return;
        
        // Get the specific component
        const component = components[props.componentName];
        
        if (!component) {
          throw new Error(`Component '${props.componentName}' not found in MFE '${props.mfeName}'`);
        }
        
        if (typeof component !== 'function') {
          throw new Error(`'${props.componentName}' is not a valid component`);
        }
        
        setComponent(() => component as Component<T>);
        props.onLoad?.(component as Component<T>);
        
      } catch (err) {
        if (cancelled) return;

        const msg = err instanceof Error ? err.message : String(err);
        setError(msg);
        props.onError?.(msg);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    onCleanup(() => {
      cancelled = true;
    });
  });

  import.meta.hot?.dispose(() => {
    setComponent(null);
    // Clear cache on hot reload
    MessagesComponents = null;
  });

  return (
    <div
      class={`dynamic-container ${props.className || ""}`}
      data-mfe={props.mfeName}
      data-component={props.componentName}
    >
      <Show when={loading() && !component()}>
        <div class="dynamic-container-loading">
          <SpinnerIcon spin />
          <span>{i18n("loading") || "Loading..."}</span>
        </div>
      </Show>

      <Show when={error() && !loading()}>
        <div class="dynamic-container-error">
          <Badge severity="danger" value={`${i18n("errorLoadingComponent") || "Error loading component"}: ${error()}`} />
        </div>
      </Show>

      <Show when={component() && !loading() && !error()}>
        <div class="dynamic-container-content">
          {component()!(props.props as T)}
        </div>
      </Show>
    </div>
  );
}

// ============================================================================
// FACTORY FUNCTION
// ============================================================================

export function createDynamicContainer(mfeName: string, basePath?: string) {
  return {
    Component: (props: { 
      componentName: string; 
      componentProps?: unknown;
      className?: string;
      onLoad?: (component: Component) => void;
      onError?: (error: string) => void;
    }) => (
      <DynamicContainer
        mfeName={mfeName}
        componentName={props.componentName}
        basePath={basePath}
        props={props.componentProps}
        className={props.className}
        onLoad={props.onLoad}
        onError={props.onError}
      />
    ),
  };
}

// ============================================================================
// BATCH CONTAINER
// ============================================================================

export type BatchDynamicContainerProps = {
  mfeName: string;
  basePath?: string;
  components: Array<{
    componentName: string;
    props?: unknown;
    className?: string;
  }>;
  className?: string;
};

export function BatchDynamicContainer(
  props: BatchDynamicContainerProps
) {
  const [loadedComponents, setLoadedComponents] = createSignal<
    Map<string, Component<unknown>>
  >(new Map());
  const [loadingStates, setLoadingStates] = createSignal<
    Map<string, boolean>
  >(new Map());
  const [errors, setErrors] = createSignal<Map<string, string>>(new Map());

  createEffect(() => {
    const newLoadingStates = new Map<string, boolean>();
    const newErrors = new Map<string, string>();
    const newComponents = new Map<string, Component<unknown>>();

    props.components.forEach((comp) => {
      newLoadingStates.set(comp.componentName, true);
      newErrors.set(comp.componentName, "");
      newComponents.set(comp.componentName, null!);
    });

    setLoadingStates(newLoadingStates);
    setErrors(newErrors);
    setLoadedComponents(newComponents);

    let cancelled = false;

    (async () => {
      try {
        const loader = componentRegistry.get(props.mfeName);
        
        if (!loader) {
          throw new Error(`Microfrontend '${props.mfeName}' not registered`);
        }
        
        const components = await loader();
        
        if (cancelled) return;
        
        // Load all requested components
        props.components.forEach((comp) => {
          const component = components[comp.componentName];
          
          if (!component) {
            newErrors.set(comp.componentName, `Component not found`);
            newLoadingStates.set(comp.componentName, false);
          } else if (typeof component !== 'function') {
            newErrors.set(comp.componentName, `Invalid component`);
            newLoadingStates.set(comp.componentName, false);
          } else {
            newComponents.set(comp.componentName, component as Component<unknown>);
            newLoadingStates.set(comp.componentName, false);
          }
        });
        
        setLoadedComponents(new Map(newComponents));
        setLoadingStates(new Map(newLoadingStates));
        setErrors(new Map(newErrors));
        
      } catch (err) {
        if (cancelled) return;
        
        const msg = err instanceof Error ? err.message : String(err);
        props.components.forEach((comp) => {
          newErrors.set(comp.componentName, msg);
          newLoadingStates.set(comp.componentName, false);
        });
        setErrors(new Map(newErrors));
        setLoadingStates(new Map(newLoadingStates));
      }
    })();

    onCleanup(() => {
      cancelled = true;
    });
  });

  return (
    <div class={`batch-dynamic-container ${props.className || ""}`}>
      {props.components.map((comp) => (
        <div class="batch-container-item" data-component={comp.componentName}>
          <Show when={loadingStates().get(comp.componentName)}>
            <div class="dynamic-container-loading">
              <SpinnerIcon spin />
              <span>{`Loading ${comp.componentName}...`}</span>
            </div>
          </Show>

          <Show when={errors().get(comp.componentName)}>
            <div class="dynamic-container-error">
              <Badge severity="danger" value={`Error: ${errors().get(comp.componentName)}`} />
            </div>
          </Show>

          <Show when={loadedComponents().get(comp.componentName) && 
                       !loadingStates().get(comp.componentName) && 
                       !errors().get(comp.componentName)}>
            <div class="dynamic-container-content">
              {loadedComponents().get(comp.componentName)!(comp.props)}
            </div>
          </Show>
        </div>
      ))}
    </div>
  );
}

// ============================================================================
// EXPORTS
// ============================================================================

export type {
  DynamicContainerProps,
  BatchDynamicContainerProps,
};

export {
  componentRegistry,
};