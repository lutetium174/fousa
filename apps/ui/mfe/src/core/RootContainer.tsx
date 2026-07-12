import { createEffect, createSignal, For, Show } from "solid-js";
import type { MicroFrontendDefinition } from "./MicroFrontendTypes.ts";
import {
  authMfe,
  findMicroFrontendByRoute,
  microfrontends,
} from "./MicroFrontendRegistry.ts";
import { MicroFrontendHost } from "./MicroFrontendHost.tsx";
import { globalContext } from "./GlobalContext";
import { useRouter } from "../contexts/RouteContext.tsx";
import { Tab, TabList, Tabs } from "components";

type RootContainerProps = {};

export function RootContainer(_: RootContainerProps) {
  const { route, setRoute } = useRouter();

  const [currentMfe, setCurrentMfe] =
    createSignal<MicroFrontendDefinition | null>(null);
  const [isAuthenticated, setIsAuthenticated] = createSignal(
    globalContext.getState().user !== null,
  );

  createEffect(() =>
    globalContext.subscribe((state) => {
      setIsAuthenticated(state.user !== null);
    }),
  );

  createEffect(() => {
    if (true || isAuthenticated()) {
      const mfe = findMicroFrontendByRoute(route().path || "/");
      setCurrentMfe(mfe || null);
      return;
    }

    setCurrentMfe(authMfe);
  });

  return (
    <div class="root-container">
      <Tabs
        variant="line"
        defaultValue="/messages"
        onChange={(value) => setRoute(() => ({ path: value }))}
      >
        <Show when={currentMfe()}>
          {(mfe) => <MicroFrontendHost mfe={mfe()} />}
        </Show>

        <Show when={!currentMfe() && isAuthenticated()}>
          <div class="no-mfe">
            <p>No module configured for route: {location.pathname}</p>
          </div>
        </Show>
        <TabList>
          <For each={microfrontends}>
            {(item: MicroFrontendDefinition) =>
              item.icon ? (
                <Tab value={item.route} icon={item.icon({})} />
              ) : (
                <Tab value={item.route} />
              )
            }
          </For>
        </TabList>
      </Tabs>
    </div>
  );
}
