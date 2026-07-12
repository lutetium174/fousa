import { createContext, createSignal, Suspense, useContext } from "solid-js";
import type { ParentComponent, Accessor } from "solid-js";
import type { GlobalState } from "./GlobalContext";
import { globalContext } from "./GlobalContext";
import { I18nProvider } from "../i18n";
import { SpinnerIcon } from "components";

type GlobalContextValue = {
  state: Accessor<GlobalState>;
  setState: (fn: (prev: GlobalState) => GlobalState) => void;
};

const GlobalContext = createContext<GlobalContextValue>();

export const useGlobalContext = () => {
  const context = useContext(GlobalContext);
  if (!context) {
    throw new Error("useGlobalContext must be used within GlobalProvider");
  }
  return context;
};

export const GlobalProvider: ParentComponent = (props) => {
  const [state, setState] = createSignal<GlobalState>(globalContext.getState());

  globalContext.subscribe((newState) => {
    setState(newState);
  });

  const value: GlobalContextValue = {
    state,
    setState: (fn) => globalContext.update(fn),
  };

  return (
    <GlobalContext.Provider value={value}>
      <Suspense fallback={<SpinnerIcon />}>
        <I18nProvider>{props.children}</I18nProvider>
      </Suspense>
    </GlobalContext.Provider>
  );
};
