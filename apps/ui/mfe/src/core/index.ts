export { eventBus } from "./EventBus";
export type { EventBus } from "./EventBus";

export { globalContext } from "./GlobalContext";
export type { GlobalState, GlobalContext as GlobalContextAPI, Language } from "./GlobalContext";

export {
  loadMicroFrontend,
  mountMicroFrontend,
  loadComponentFromModule,
  clearCaches,
} from "./MicroFrontendLoader.ts";
export type { MicroFrontendEnv, MicroFrontendModule } from "./MicroFrontendTypes.ts";
export type { 
  MicroFrontendDefinition,
  MicroFrontendComponentDefinition,
  LoadedComponent,
  ComponentRegistry,
} from "./MicroFrontendTypes.ts";

export { GlobalProvider, useGlobalContext } from "./GlobalContext.tsx";

export { MicroFrontendHost } from "./MicroFrontendHost.tsx";

export { RootContainer } from "./RootContainer";
export { microfrontends, findMicroFrontendByRoute, extractRouteParams, authMfe, AUTH_MFE_URL } from "./MicroFrontendRegistry.ts";

export {
  messagesMfeComponents,
  searchMfeComponents,
  notificationsMfeComponents,
  getComponentUrl,
  getComponentDefinition,
} from "./MicroFrontendComponentRegistry.ts";

export {
  DynamicContainer,
  createDynamicContainer,
  BatchDynamicContainer,
  type DynamicContainerProps,
  type BatchDynamicContainerProps,
} from "./DynamicContainer.tsx";
