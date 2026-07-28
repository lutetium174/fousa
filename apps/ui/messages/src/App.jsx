import { createSignal } from "solid-js";
import "./App.css";
import { Dynamic } from "solid-js/web";
import { BookmarkIcon, ChatIcon, Tab, TabList, Tabs } from "components";
import { Discoveries } from "./components/Discoveries.jsx";
import { Following } from "./components/Following.jsx";
function App(props) {
    const [value, setValue] = createSignal("discover");
    const actionHandlers = {
        discover: () => <Discoveries env={props.env}/>,
        following: () => <Following />,
    };
    return (<>
      <section id="actions">
        <Tabs variant="line" onChange={(item) => {
            setValue(() => item);
        }}>
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
        <Dynamic component={actionHandlers[value()]}/>
      </section>
    </>);
}
export default App;
