import type { IconProps } from "components";
import type { EventBus } from "./EventBus";
import type { GlobalContextAPI } from "./GlobalContext";
import type { Component } from "solid-js";

export type MicroFrontendEnv = {
  eventBus: EventBus;
  globalContext: GlobalContextAPI;
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
  icon?: Component<IconProps>
};
