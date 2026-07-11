import {
  createEffect,
  createSignal,
  onCleanup,
  Show,
  type Component,
  type JSX,
} from "solid-js";
import type { MicroFrontendComponentDefinition } from "./MicroFrontendTypes.ts";
import { loadComponentFromModule } from "./MicroFrontendLoader.ts";
import { useGlobalContext } from "./GlobalContext.tsx";
import { useI18n } from "../i18n/index.tsx";
import { Badge, SpinnerIcon } from "components";

export type DynamicContainerProps<T = unknown> = {
  mfeUrl: string;
  componentName: string;
  basePath?: string;
  props?: T;
  className?: string;
  onLoad?: (component: Component<T>) => void;
  onError?: (error: string) => void;
};

/**
 * DynamicContainer - A container component that loads and renders specific components
 * from microfrontends while maintaining the microfrontend's context and isolation.
 * 
 * This allows you to embed components from different microfrontends anywhere in your app
 * without loading the entire microfrontend.
 */
export function DynamicContainer<T = unknown>(
  props: DynamicContainerProps<T>
) {
  const { i18n } = useI18n();
  const { state } = useGlobalContext();

  const [component, setComponent] = createSignal<Component<T> | null>(null);
  const [loading, setLoading] = createSignal(true);
  const [error, setError] = createSignal<string | null>(null);
  const [loadKey, setLoadKey] = createSignal<string>("");

  createEffect(() => {
    // Create a unique load key based on props to trigger reload when they change
    const newLoadKey = `${props.mfeUrl}?component=${props.componentName}&lang=${state().language}&_t=${Date.now()}`;
    setLoadKey(newLoadKey);
  });

  createEffect(() => {
    loadKey(); // Trigger reload when loadKey changes
    
    setLoading(true);
    setError(null);
    setComponent(null);

    let cancelled = false;

    (async () => {
      try {
        const loaded = await loadComponentFromModule<T>(
          props.mfeUrl,
          props.componentName
        );

        if (cancelled) return;

        setComponent(() => loaded.component);
        props.onLoad?.(loaded.component);
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
  });

  return (
    <div
      class={`dynamic-container ${props.className || ""}`}
      data-mfe-url={props.mfeUrl}
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
        {/* @once - SolidJS will only render this once, maintaining component state */}
        <div class="dynamic-container-content">
          {component()!(props.props as T)}
        </div>
      </Show>
    </div>
  );
}

// Higher-order component for creating specialized dynamic containers
export function createDynamicContainer<T = unknown>(
  mfeUrl: string,
  basePath?: string
) {
  return {
    Component: (props: { 
      componentName: string; 
      componentProps?: T;
      className?: string;
      onLoad?: (component: Component<T>) => void;
      onError?: (error: string) => void;
    }) => (
      <DynamicContainer<T>
        mfeUrl={mfeUrl}
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

// Batch container for loading multiple components from the same MFE
export type BatchDynamicContainerProps = {
  mfeUrl: string;
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

  // Load all components in parallel
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

    Promise.all(
      props.components.map(async (comp) => {
        try {
          const loaded = await loadComponentFromModule(
            props.mfeUrl,
            comp.componentName
          );
          
          if (cancelled) return;
          
          newComponents.set(comp.componentName, loaded.component);
          newLoadingStates.set(comp.componentName, false);
          setLoadedComponents(new Map(newComponents));
          setLoadingStates(new Map(newLoadingStates));
        } catch (err) {
          if (cancelled) return;
          
          const msg = err instanceof Error ? err.message : String(err);
          newErrors.set(comp.componentName, msg);
          newLoadingStates.set(comp.componentName, false);
          setErrors(new Map(newErrors));
          setLoadingStates(new Map(newLoadingStates));
        }
      })
    ).finally(() => {
      if (!cancelled) {
        // Mark any remaining as not loading
        newLoadingStates.forEach((_, key) => newLoadingStates.set(key, false));
        setLoadingStates(new Map(newLoadingStates));
      }
    });

    onCleanup(() => {
      cancelled = true;
    });
  });

  return (
    <div class={`batch-dynamic-container ${props.className || ""}`}>
      {props.components.map((comp) => (
        <div
          class="batch-container-item"
          data-component={comp.componentName}
        >
          <Show when={loadingStates().get(comp.componentName)}>
            <div class="dynamic-container-loading">
              <SpinnerIcon spin />
              <span>{`Loading ${comp.componentName}...`}</span>
            </div>
          </Show>

          <Show when={errors().get(comp.componentName)}>
            <div class="dynamic-container-error">
              <Badge 
                severity="danger" 
                value={`Error: ${errors().get(comp.componentName)}`} 
              />
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