import {
  type Component, type JSX, createSignal, createContext,
  useContext, splitProps, mergeProps,
  type Accessor,
} from "solid-js";
import styles from "./tabs.module.css";
import {toPascalCase} from "../../utils/pascalCase";

/* ---- Context ---- */
interface TabsCtx {
  active:    Accessor<string>;
  setActive: (v: string) => void;
  variant:   "line" | "pills" | "pills-filled";
}
const TabsContext = createContext<TabsCtx>();
const useTabsCtx = () => {
  const ctx = useContext(TabsContext);
  if (!ctx) throw new Error("<Tab*> must be used inside <Tabs>");
  return ctx;
};

/* ---- Tabs (root) ---- */
export interface TabsProps {
  defaultValue?: string;
  value?:        string;
  onChange?:     (value: string) => void;
  variant?:      "line" | "pills" | "pills-filled";
  children:      JSX.Element;
  class?:        string;
}

export const Tabs: Component<TabsProps> = (rawProps) => {
  const props = mergeProps({ variant: "line" as const }, rawProps);
  const [local, _] = splitProps(props, [
    "defaultValue", "value", "onChange", "variant", "children", "class",
  ]);

  const [internalActive, setInternalActive] = createSignal(local.defaultValue ?? "");

  const active = () => local.value ?? internalActive();
  const setActive = (v: string) => {
    setInternalActive(v);
    local.onChange?.(v);
  };

  return (
    <TabsContext.Provider value={{ active, setActive, variant: local.variant }}>
      <div class={[styles.pvTabs, local.class].filter(Boolean).join(" ")}>
        {local.children}
      </div>
    </TabsContext.Provider>
  );
};

/* ---- TabList ---- */
export interface TabListProps {
  children: JSX.Element;
  class?:   string;
}

export const TabList: Component<TabListProps> = (props) => {
  const ctx = useTabsCtx();

  const handleKeyDown = (e: KeyboardEvent) => {
    const list = (e.currentTarget as HTMLElement).querySelectorAll<HTMLElement>("[role=tab]");
    const tabs = Array.from(list);
    const idx  = tabs.indexOf(document.activeElement as HTMLElement);
    if (e.key === "ArrowRight") tabs[(idx + 1) % tabs.length]?.focus();
    if (e.key === "ArrowLeft")  tabs[(idx - 1 + tabs.length) % tabs.length]?.focus();
    if (e.key === "Home")       tabs[0]?.focus();
    if (e.key === "End")        tabs[tabs.length - 1]?.focus();
  };

  return (
    <div
      role="tablist"
      class={[
        styles.pvTablist,
        styles[`pvTablist${toPascalCase(ctx.variant)}`],
        props.class ?? "",
      ].filter(Boolean).join(" ")}
      onKeyDown={handleKeyDown}
    >
      {props.children}
    </div>
  );
};

/* ---- Tab (trigger) ---- */
export interface TabProps {
  value:     string;
  disabled?: boolean;
  icon?:     JSX.Element;
  children?:  JSX.Element;
  class?:    string;
}

export const Tab: Component<TabProps> = (props) => {
  const ctx = useTabsCtx();
  const isActive = () => ctx.active() === props.value;

  return (
    <button
      role="tab"
      type="button"
      id={styles[`pvTab${props.value}`]}
      aria-controls={styles[`pvTabpanel${props.value}`]}
      aria-selected={isActive()}
      disabled={props.disabled}
      tabIndex={isActive() ? 0 : -1}
      class={[
        styles.pvTab,
        isActive() ? styles.pvTabActive : "",
        props.class ?? "",
      ].filter(Boolean).join(" ")}
      onClick={() => !props.disabled && ctx.setActive(props.value)}
    >
      {props.icon && <span class={styles.pvTabIcon} aria-hidden="true">{props.icon}</span>}
      <span class={styles.pvTabLabel}>{props.children}</span>
    </button>
  );
};

/* ---- TabPanel ---- */
export interface TabPanelProps {
  value:    string;
  children: JSX.Element;
  class?:   string;
}

export const TabPanel: Component<TabPanelProps> = (props) => {
  const ctx = useTabsCtx();
  return (
    <div
      role="tabpanel"
      id={styles[`pvTabpanel${props.value}`]}
      aria-labelledby={styles[`pvTab${props.value}`]}
      hidden={ctx.active() !== props.value}
      class={[styles.pvTabpanel, props.class ?? ""].filter(Boolean).join(" ")}
      tabIndex={0}
    >
      {props.children}
    </div>
  );
};