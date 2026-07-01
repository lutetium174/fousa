class GlobalContextAPI {
}

type MicrofrontendEnv = {
    eventBus: EventBus;              // Global event bus
    globalContext: GlobalContextAPI; // Global state API
    basePath: string;                // MFE base path
    routeParams?: Record<string, string>; // Route params (if pattern matched)
    runtimeVersion: string;          // Container version (e.g., "1.0.0")
};