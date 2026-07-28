import { createSignal } from "solid-js";
import type { MicroFrontendEnvironment } from "./types/MicroFrontendEnvironment.ts";
import "./App.css";
import { Dynamic } from "solid-js/web";
import {
  BookmarkIcon,
  ChatIcon,
  Tab,
  TabList,
  Tabs
} from "components";
import { Discoveries } from "./components/Discoveries.jsx";
import { Following } from "./components/Following.jsx";

type actions = "discover" | "following";

function App(props: { env: MicroFrontendEnvironment }) {
  const [value, setValue] = createSignal<actions>("discover");

  const actionHandlers = {
    discover: () => <Discoveries env={props.env} />,
    following: () => <Following />,
  };

  return (
    <>
      <section id="actions">
        <Tabs
          variant="line"
          onChange={(item: string) => {
            setValue(() => item as actions);
          }}
        >
          <TabList>
            <Tab value="discover" icon={<ChatIcon />}>
              Discover
            </Tab>
            <Tab value="following" icon={<BookmarkIcon />}>
              Following
            </Tab>
          </TabList>
        </Tabs>
      </section>

      <div class="ticks"></div>
      <section id="details">
        <Dynamic component={actionHandlers[value()]} />
      </section>
    </>
  );
}

export default App;
