import { type Component, createSignal, createEffect } from "solid-js";
import styles from "./language-switcher.module.css";
import Select from "../Select/Select";

type Language = "en" | "fr" | "es" | "de" | "it" | "pt" | "ja" | "ko";

const LANGUAGE_LABELS: Record<Language, string> = {
  en: "English",
  fr: "Français",
  es: "Español",
  de: "Deutsch",
  it: "Italiano",
  pt: "Português",
  ja: "日本語",
  ko: "한국어",
} as const;

export interface LanguageSwitcherProps {
  languages?: Language[];
  class?: string;
  value?: Language;
  onChange?: (language: Language) => void;
}

const LanguageSwitcher: Component<LanguageSwitcherProps> = (props) => {
  const [currentLanguage, setCurrentLanguage] = createSignal<Language>(
    props.value || "en",
  );

  const languageList = () =>
    props.languages ??
    (["en", "fr", "de", "es", "it", "pt", "ja", "ko"] as Language[]);

  createEffect(() => {
    if (props.value && props.value !== currentLanguage()) {
      setCurrentLanguage(props.value);
    }
  });

  const handleLanguageChange = (lang: Language) => {
    setCurrentLanguage(lang);
    props.onChange?.(lang);
  };

  return (
    <div
      class={[styles.pvLanguageSwitcher, props.class].filter(Boolean).join(" ")}
    >
      <div class={styles.pvLanguageSwitcherSection}>
        <span class={styles.pvLanguageSwitcherLabel}>Language</span>
        <div class={styles.pvLanguageSwitcherButtons}>
          <Select options={languageList().map((lang) => ({
            value: lang,
            label: LANGUAGE_LABELS[lang],
          }))} 
          onChange={handleLanguageChange}/>
        </div>
      </div>
    </div>
  );
};

export default LanguageSwitcher;
