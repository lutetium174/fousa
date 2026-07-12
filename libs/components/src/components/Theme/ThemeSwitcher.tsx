import { type Component, For, createMemo } from "solid-js";
import { useTheme } from "./ThemeProvider";
import { type BuiltinThemeName, builtinThemes } from "./themes";
import { Button } from "../Button/Button";
import {RefreshIcon} from "../Icons/Icons"
import styles from "./theme-switcher.module.css";

const SCHEME_LABELS = {
  light: "Light",
  dark: "Dark",
  system: "System",
} as const;

export interface ThemeSwitcherProps {
  themes?: BuiltinThemeName[];
  showSchemeToggle?: boolean;
  class?: string;
}

const ThemeSwitcher: Component<ThemeSwitcherProps> = (props) => {
  const { themeName, colorScheme, setTheme, setColorScheme } = useTheme();

  const themeList = createMemo(
    () => (props.themes ?? Object.keys(builtinThemes)) as BuiltinThemeName[],
  );

  const schemes = ["light", "dark", "system"] as const;
  const showScheme = () => props.showSchemeToggle !== false;

  return (
    <div
      class={[styles.pvThemeSwitcher, props.class].filter(Boolean).join(" ")}
    >
      <div class={styles.pvThemeSwitcherSection}>
        <span class={styles.pvThemeSwitcherLabel}>Theme</span>
        <div class={styles.pvThemeSwitcherSwatches}>
          <For each={themeList()}>
            {(name) => {
              const def = builtinThemes[name];
              const primary = def.light.tokens["--primary"];
              return (
                <button
                  type="button"
                  class={[
                    styles.pvThemeSwatch,
                    themeName() === name ? styles.pvThemeSwatchActive : "",
                  ]
                    .filter(Boolean)
                    .join(" ")}
                  style={{ "--swatch-color": primary }}
                  title={name.charAt(0).toUpperCase() + name.slice(1)}
                  aria-pressed={themeName() === name}
                  onClick={() => setTheme(name)}
                >
                  <span class={styles.pvThemeSwatchDot} />
                  <span class={styles.pvThemeSwatchName}>{name}</span>
                </button>
              );
            }}
          </For>
        </div>
      </div>

      {showScheme() && (
        <div class={styles.pvThemeSwitcherSection}>
          <span class={styles.pvThemeSwitcherLabel}>Mode</span>
          <div class={styles.pvThemeSwitcherModes}>
            <For each={schemes}>
              {(s) => (
                <Button
                  class={[
                    colorScheme() === s ? styles.pvModeBtnActive : "",
                  ]
                    .filter(Boolean)
                    .join(" ")}
                  aria-pressed={colorScheme() === s}
                  onClick={() => setColorScheme(s)}
                >
                  {SCHEME_LABELS[s]}
                </Button>
              )}
            </For>
          </div>
        </div>
      )}
    </div>
  );
};

export default ThemeSwitcher;
