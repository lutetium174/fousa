import { GlobalProvider, RootContainer } from "./core";
import { ThemeSwitcher } from "components";
import "./App.css";

const App = () => {
  return (
    <GlobalProvider>
      <ThemeSwitcher />
      <RootContainer />
    </GlobalProvider>
  );
};

export default App;
