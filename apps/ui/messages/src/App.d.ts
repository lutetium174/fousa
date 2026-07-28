import type { MicroFrontendEnvironment } from "./types/MicroFrontendEnvironment.ts";
import "./App.css";
declare function App(props: {
    env: MicroFrontendEnvironment;
}): import("solid-js").JSX.Element;
export default App;
