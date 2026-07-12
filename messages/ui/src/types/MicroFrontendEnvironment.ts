class EventBus {
}

class GlobalContextAPI {
}

export type MicroFrontendEnvironment = {
  eventBus: EventBus;
  globalContext: GlobalContextAPI;
  basePath: string;
  routeParams?: Record<string, string>;
  runtimeVersion: string;
};