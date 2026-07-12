import { LanguageSwitcher as BaseLanguageSwitcher, type LanguageSwitcherProps } from "components";
import { useI18n } from "../i18n";

export interface ConnectedLanguageSwitcherProps extends Omit<LanguageSwitcherProps, "value" | "onChange"> {
  class?: string;
}

export const LanguageSwitcher = (props: ConnectedLanguageSwitcherProps) => {
  const { locale, setLocale } = useI18n();

  return (
    <BaseLanguageSwitcher
      {...props}
      value={locale()}
      onChange={setLocale}
    />
  );
};

export default LanguageSwitcher;
