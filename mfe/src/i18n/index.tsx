import {
  createContext,
  useContext,
  createSignal,
  createResource,
  createMemo,
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

type languages = "en" | "fr";
type I18nContextType = {
  i18n: Translator<BaseRecordDict, string>;
  locale?: Accessor<languages>;
  setLocale?: Setter<languages>;
};

const I18nContext = createContext<I18nContextType>({
  i18n: {} as Translator<BaseRecordDict, string>,
});

const dictionaries = {
  en: () => import("./dictionaries/en"),
  fr: () => import("./dictionaries/fr"),
};

export function I18nProvider(props: ParentProps<Record<string, any>>) {
  const [locale, setLocale] = createSignal<languages>("en");

  const [dictionary] = createResource(locale, async (language) => {
    const mod = await dictionaries[language]();
    return flatten(mod.default);
  });

  const i18n = createMemo(() => translator(() => dictionary()!))();

  return (
    <I18nContext.Provider value={{ i18n, locale, setLocale }}>
      {props.children}
    </I18nContext.Provider>
  );
}

export const useI18n = () => {
  const context = useContext(I18nContext);
  if (!context) {
    throw new Error("useI18n must be used within an I18nProvider");
  }
  return context;
};
