import type { IconProps } from "components";
import type { EventBus } from "./EventBus";
import type { GlobalContext } from "./GlobalContext";
import type { Component } from "solid-js";

export type MicroFrontendEnv = {
  eventBus: EventBus;
  globalContext: GlobalContext;
  basePath: string;
  routeParams?: Record<string, string>;
  runtimeVersion: string;
};

export type MicroFrontendModule = {
  mount: (container: HTMLElement, env: MicroFrontendEnv) => void | Promise<void>;
  unmount: (container: HTMLElement) => void | Promise<void>;
};

export type MicroFrontendDefinition = {
  name: string;
  url: string;
  basePath: string;
  route: string;
  props?: Record<string, unknown>;
  icon?: Component<IconProps>;
};

// Types for component-based microfrontend loading
export type MicroFrontendComponentDefinition<T = unknown> = {
  name: string;
  url: string;
  basePath: string;
  componentName: string;
  props?: T;
  icon?: Component<IconProps>;
};

export type LoadedComponent<T extends Record<string, any>> = {
  component: Component<T>;
  unmount?: () => void | Promise<void>;
};

export type ComponentRegistry<T extends Record<string, any>> = {
  [mfeName: string]: {
    [componentName: string]: LoadedComponent<T>;
  };
};
