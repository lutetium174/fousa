import {
  createContext,
  createSignal,
  useContext,
  type Accessor,
  type ParentComponent,
} from "solid-js";

export type RouteState = {
  path: string;
};

type RouteContextValue = {
  state: Accessor<RouteState>;
  setState: (fn: (prev: RouteState) => RouteState) => void;
};

const RouteContext = createContext<RouteContextValue>();

export const useRouter = () => {
  const context = useContext(RouteContext);
  if (!context) {
    throw new Error("useRoute must be used within RouteProvider");
  }

  return {route: context.state, setRoute: context.setState };
};

export const RouteProvider: ParentComponent = (props) => {
  const [state, setState] = createSignal<RouteState>({ path: "/" });

  return (
    <RouteContext.Provider value={{ state, setState }}>
      {props.children}
    </RouteContext.Provider>
  );
};
