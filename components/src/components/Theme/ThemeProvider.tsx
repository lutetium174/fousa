import {
  createContext,
  useContext,
  createSignal,
  createEffect,
  onCleanup,
  onMount,
  mergeProps,
  type Component,
  type JSX,
  type Accessor,
  type Setter,
} from "solid-js";

import {
  type ThemeDefinition, type BuiltinThemeName, builtinThemes,
} from "./themes";

// ─── Types ─────────────────────────────────────────────────

export type ColorScheme = "light" | "dark" | "system";

export interface ThemeContextValue {
  /** Name of the active theme */
  themeName:      Accessor<string>;
  /** Resolved color scheme ("light" | "dark") */
  resolvedScheme: Accessor<"light" | "dark">;
  /** The raw color scheme setting, which may be "system" */
  colorScheme:    Accessor<ColorScheme>;
  /** Switch to a different theme by name or definition */
  setTheme:       (t: BuiltinThemeName | ThemeDefinition) => void;
  /** Override the color scheme */
  setColorScheme: Setter<ColorScheme>;
  /** Register a custom theme so it can be referenced by name */
  registerTheme:  (def: ThemeDefinition) => void;
}

export interface ThemeProviderProps {
  /** Built-in theme name or a custom ThemeDefinition object. Default: "aura" */
  theme?:        BuiltinThemeName | ThemeDefinition;
  /** "light" | "dark" | "system". Default: "system" */
  colorScheme?:  ColorScheme;
  /** When true, CSS vars are applied on <html> instead of the wrapper div.
   *  Useful for the top-level provider. Default: false */
  global?:       boolean;
  children:      JSX.Element;
  class?:        string;
  style?:        JSX.CSSProperties;
}

// ─── Context ───────────────────────────────────────────────

const ThemeContext = createContext<ThemeContextValue>();

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error("useTheme must be used inside <ThemeProvider>");
  return ctx;
}

// ─── Helpers ───────────────────────────────────────────────

function applyTokens(el: HTMLElement, tokens: Record<string, string>) {
  for (const [k, v] of Object.entries(tokens)) {
    el.style.setProperty(k, v);
  }
}

function clearTokens(el: HTMLElement, tokens: Record<string, string>) {
  for (const k of Object.keys(tokens)) {
    el.style.removeProperty(k);
  }
}

function resolveTheme(
  name: BuiltinThemeName | ThemeDefinition,
  registry: Map<string, ThemeDefinition>
): ThemeDefinition {
  if (typeof name === "object") return name;
  return registry.get(name) ?? builtinThemes[name] ?? builtinThemes.aura;
}

// ─── Provider ──────────────────────────────────────────────

const ThemeProvider: Component<ThemeProviderProps> = (rawProps) => {
  const props = mergeProps(
    { theme: "aura" as BuiltinThemeName, colorScheme: "system" as ColorScheme, global: false },
    rawProps,
  );

  // Registry of named themes (built-ins + any user-registered ones)
  const registry = new Map<string, ThemeDefinition>(Object.entries(builtinThemes));

  // Signals
  const initialTheme = typeof props.theme === "object" ? props.theme : resolveTheme(props.theme, registry);
  const [activeTheme,   setActiveTheme]   = createSignal<ThemeDefinition>(initialTheme);
  const [colorScheme,   setColorScheme]   = createSignal<ColorScheme>(props.colorScheme);
  const [systemDark,    setSystemDark]    = createSignal(false);
  const [themeName,     setThemeName]     = createSignal(initialTheme.name);

  // Track system dark preference
  onMount(() => {
    const mq = window.matchMedia("(prefers-color-scheme: dark)");
    setSystemDark(mq.matches);
    const handler = (e: MediaQueryListEvent) => setSystemDark(e.matches);
    mq.addEventListener("change", handler);
    onCleanup(() => mq.removeEventListener("change", handler));
  });

  const resolvedScheme = (): "light" | "dark" => {
    const cs = colorScheme();
    if (cs === "system") return systemDark() ? "dark" : "light";
    return cs;
  };

  // DOM ref for the wrapper element (or <html> when global)
  let el: HTMLElement | undefined;

  // Apply tokens whenever theme or scheme changes
  createEffect(() => {
    const target = props.global ? document.documentElement : el;
    if (!target) return;

    const theme  = activeTheme();
    const scheme = resolvedScheme();
    const tokens = scheme === "dark" ? theme.dark.tokens : theme.light.tokens;

    // Clear previously set tokens from all schemes to avoid stale vars
    clearTokens(target, theme.light.tokens);
    clearTokens(target, theme.dark.tokens);

    applyTokens(target, tokens);

    // Set a data attribute for easy CSS targeting
    target.dataset.pvTheme  = theme.name;
    target.dataset.pvScheme = scheme;
  });

  // Context value
  const ctx: ThemeContextValue = {
    themeName:      () => themeName(),
    resolvedScheme: () => resolvedScheme(),
    colorScheme:    () => colorScheme(),
    setColorScheme: setColorScheme as Setter<ColorScheme>,
    registerTheme: (def: ThemeDefinition) => {
      registry.set(def.name, def);
    },
    setTheme: (t: BuiltinThemeName | ThemeDefinition) => {
      const resolved = resolveTheme(t, registry);
      setActiveTheme(resolved);
      setThemeName(resolved.name);
    },
  };

  // For global mode we don't need a wrapper div — just render children
  if (props.global) {
    // Still need to run the effect so we wire up `el` to documentElement
    el = typeof document !== "undefined" ? document.documentElement : undefined;
    return (
      <ThemeContext.Provider value={ctx}>
        {props.children}
      </ThemeContext.Provider>
    );
  }

  return (
    <ThemeContext.Provider value={ctx}>
      <div
        ref={r => el = r}
        class={["pv-theme-root", props.class].filter(Boolean).join(" ")}
        style={{
          "color":      "var(--text-color)",
          "background": "var(--bg)",
          "font-family":"var(--font-sans)",
          ...props.style,
        }}
      >
        {props.children}
      </div>
    </ThemeContext.Provider>
  );
};

export default ThemeProvider;