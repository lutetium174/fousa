export { eventBus } from "./EventBus";
export type { EventBus } from "./EventBus";

export { globalContext } from "./GlobalContext";
export type { GlobalState, GlobalContextAPI } from "./GlobalContext";

export {
  loadMicroFrontend,
  mountMicroFrontend,
} from "./MicroFrontendLoader.ts";
export type { MicroFrontendEnv, MicroFrontendModule } from "./MicroFrontendTypes.ts";
export type { MicroFrontendDefinition } from "./MicroFrontendTypes.ts";

export { GlobalProvider, useGlobalContext } from "./GlobalContext.tsx";

export { MicroFrontendHost } from "./MicroFrontendHost.tsx";

export { RootContainer } from "./RootContainer";
export { microfrontends, findMicroFrontendByRoute, extractRouteParams, authMfe, AUTH_MFE_URL } from "./MicroFrontendRegistry.ts";
