import {
  createContext,
  useContext,
  createSignal,
  createResource,
  createEffect,
  type Accessor,
  type Setter,
} from "solid-js";
import {
  flatten,
  translator,
  type BaseRecordDict,
  type Translator,
} from "@solid-primitives/i18n";
import type { ParentProps } from "solid-js/types/render/component.js";

type Language = "en" | "fr" | "es" | "de" | "it" | "pt" | "ja" | "ko";
type I18nContextType = {
  i18n: Translator<BaseRecordDict, string>;
  locale: Accessor<Language>;
  setLocale: Setter<Language>;
};

const I18nContext = createContext<I18nContextType | null>(null);

const dictionaries = {
  en: () => import("./dictionaries/en"),
  fr: () => import("./dictionaries/fr"),
  es: () => import("./dictionaries/es"),
  de: () => import("./dictionaries/de"),
  it: () => import("./dictionaries/it"),
  pt: () => import("./dictionaries/pt"),
  ja: () => import("./dictionaries/ja"),
  ko: () => import("./dictionaries/ko"),
};

export interface I18nProviderProps extends ParentProps {
  value?: Language;
  onChange?: (language: Language) => void;
}

export function I18nProvider(props: I18nProviderProps) {
  const [locale, setLocale] = createSignal<Language>(props.value || "en");

  const [dictionary] = createResource(locale, async (language) => {
    const mod = await dictionaries[language]();
    return flatten(mod.default);
  });

  const i18n = translator(() => dictionary()!);

  createEffect(() => {
    props.onChange?.(locale());
  });

  return (
    <I18nContext.Provider value={{ i18n, locale, setLocale }}>
      {props.children}
    </I18nContext.Provider>
  );
}

export const useI18n = () => {
  const context = useContext(I18nContext);
  if (context === null) {
    throw new Error("useI18n must be used within an I18nProvider");
  }
  return context;
};
