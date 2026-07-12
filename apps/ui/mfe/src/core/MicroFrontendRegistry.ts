import {
  HomeIcon,
  NotificationIcon,
  SearchIcon,
} from "components";
import type { MicroFrontendDefinition } from "./MicroFrontendTypes.ts";

export const AUTH_MFE_URL = "http://localhost:5174/index.js";

export const microfrontends: MicroFrontendDefinition[] = [
  {
    name: "messages",
    url: "http://localhost:5174/src/index.tsx",
    basePath: "/messages",
    route: "/messages",
    icon: HomeIcon,
  },
  {
    name: "search",
    url: "http://localhost:5175/src/index.tsx",
    basePath: "/search",
    route: "/search",
    icon: SearchIcon,
  },
  {
    name: "notifications",
    url: "http://localhost:5176/src/index.tsx",
    basePath: "/notifications",
    route: "/notifications",
    icon: NotificationIcon,
  },
];

// Auth MFE is a fallback - not in the registry, handled separately
export const authMfe: MicroFrontendDefinition = {
  name: "auth",
  url: AUTH_MFE_URL,
  basePath: "/auth",
  route: "/auth",
};

export function findMicroFrontendByRoute(
  route: string,
): MicroFrontendDefinition | undefined {
  return microfrontends.find((mfe) => {
    if (mfe.route === route) return true;
    const pattern = new RegExp(`^${mfe.route.replace(/:[^/]+/g, "[^/]+")}`);
    return pattern.test(route);
  });
}

export function extractRouteParams(
  mfeRoute: string,
  currentRoute: string,
): Record<string, string> {
  const paramNames = mfeRoute.match(/:[^/]+/g) || [];
  const pattern = new RegExp(`^${mfeRoute.replace(/:[^/]+/g, "([^/]+)")}$`);
  const match = currentRoute.match(pattern);

  if (!match) return {};

  const params: Record<string, string> = {};
  paramNames.forEach((name, index) => {
    params[name.slice(1)] = match[index + 1];
  });

  return params;
}
