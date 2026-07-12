import type { EventBus } from "../core/EventBus";

interface GlobalContext {
  state: {
    language: Language;
    theme: "light" | "dark";
    user: {}
  };
};

export type Language = "en" | "fr";

export type MicroFrontendEnvironment = {
  eventBus: EventBus;
  globalContext: GlobalContext;
  basePath: string;
  routeParams?: Record<string, string>;
  runtimeVersion: string;
};
