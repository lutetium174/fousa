import type { MicroFrontendComponentDefinition } from "./MicroFrontendTypes.ts";

// Define the messages microfrontend components
// These URLs point to where the messages MFE exports its components
export const messagesMfeComponents: MicroFrontendComponentDefinition[] = [
  {
    name: "messages",
    url: "http://localhost:5174/src/components/index.ts",
    basePath: "/messages",
    componentName: "Chat",
    props: {},
  },
  {
    name: "messages",
    url: "http://localhost:5174/src/components/index.ts",
    basePath: "/messages",
    componentName: "Message",
    props: {},
  },
  {
    name: "messages",
    url: "http://localhost:5174/src/components/index.ts",
    basePath: "/messages",
    componentName: "Discoveries",
    props: {},
  },
  {
    name: "messages",
    url: "http://localhost:5174/src/components/index.ts",
    basePath: "/messages",
    componentName: "Following",
    props: {},
  },
];

// Search microfrontend components
export const searchMfeComponents: MicroFrontendComponentDefinition[] = [
  {
    name: "search",
    url: "http://localhost:5175/src/components/index.ts",
    basePath: "/search",
    componentName: "SearchBar",
    props: {},
  },
  {
    name: "search",
    url: "http://localhost:5175/src/components/index.ts",
    basePath: "/search",
    componentName: "SearchResults",
    props: {},
  },
];

// Notifications microfrontend components
export const notificationsMfeComponents: MicroFrontendComponentDefinition[] = [
  {
    name: "notifications",
    url: "http://localhost:5176/src/components/index.ts",
    basePath: "/notifications",
    componentName: "NotificationsList",
    props: {},
  },
  {
    name: "notifications",
    url: "http://localhost:5176/src/components/index.ts",
    basePath: "/notifications",
    componentName: "NotificationBadge",
    props: {},
  },
];

// Get component URL by MFE name and component name
export function getComponentUrl(
  mfeName: string,
  componentName: string
): string | undefined {
  const allComponents = [
    ...messagesMfeComponents,
    ...searchMfeComponents,
    ...notificationsMfeComponents,
  ];

  const component = allComponents.find(
    (c) => c.name === mfeName && c.componentName === componentName
  );

  return component?.url;
}

// Get component definition by MFE name and component name
export function getComponentDefinition(
  mfeName: string,
  componentName: string
): MicroFrontendComponentDefinition | undefined {
  const allComponents = [
    ...messagesMfeComponents,
    ...searchMfeComponents,
    ...notificationsMfeComponents,
  ];

  return allComponents.find(
    (c) => c.name === mfeName && c.componentName === componentName
  );
}