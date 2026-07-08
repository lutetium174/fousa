import { GlobalProvider, RootContainer } from "./core";
import { ThemeSwitcher } from "components";
import { LanguageSwitcher } from "./components/LanguageSwitcher";
import "./App.css";

const App = () => {
  return (
    <GlobalProvider>
      <div class="app-controls">
        <ThemeSwitcher />
        <LanguageSwitcher />
      </div>
      <RootContainer />
    </GlobalProvider>
  );
};

export default App;
