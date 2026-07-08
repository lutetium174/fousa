import { createContext, useContext, createSignal, type ParentComponent, type Accessor, type Setter } from "solid-js";

type Language = "en" | "fr" | "es" | "de" | "it" | "pt" | "ja" | "ko";

type LanguageContextType = {
  language: Accessor<Language>;
  setLanguage: Setter<Language>;
};

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export interface LanguageProviderProps {
  value?: Language;
  onChange?: (language: Language) => void;
}

export const LanguageProvider: ParentComponent<LanguageProviderProps> = (props) => {
  const [language, setLanguage] = createSignal<Language>(props.value || "en");

  const handleChange: Setter<Language> = (value: Language | ((prev: Language) => Language)): void => {
    if (typeof value === "function") {
      setLanguage((prev) => {
        const result = value(prev);
        props.onChange?.(result);
        return result;
      });
    } else {
      setLanguage(value);
      props.onChange?.(value);
    }
  };

  return (
    <LanguageContext.Provider value={{ language, setLanguage: handleChange }}>
      {props.children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error("useLanguage must be used within a LanguageProvider");
  }
  return context;
};

export default LanguageProvider;