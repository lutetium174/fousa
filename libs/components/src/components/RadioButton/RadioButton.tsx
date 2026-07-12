import {
  type Component,
  type JSX,
  splitProps,
  mergeProps,
  createContext,
  useContext,
  createSignal,
  type Accessor,
  type Setter,
  Show,
} from "solid-js";
import styles from "./radio-button.module.css";

// ─── Context ─────────────────────────────────────────────────

interface RadioContextValue {
  name: Accessor<string>;
  value: Accessor<string>;
  setValue: (v: string) => void;
  disabled?: boolean;
  invalid?: boolean;
  size?: "sm" | "md" | "lg";
}

const RadioContext = createContext<RadioContextValue | null>(null);

export function useRadioContext(): RadioContextValue {
  const ctx = useContext(RadioContext);
  if (!ctx) throw new Error("<RadioButton> must be used inside <RadioGroup>");
  return ctx;
}

// ─── RadioGroup ─────────────────────────────────────────────────

export interface RadioGroupProps {
  name?: string;
  value?: string;
  onChange?: (value: string) => void;
  disabled?: boolean;
  invalid?: boolean;
  size?: "sm" | "md" | "lg";
  children?: JSX.Element;
  class?: string;
  label?: string;
  helperText?: string;
  errorText?: string;
}

/**
 * Radio group container that manages state for multiple radio buttons
 */
const RadioGroup: Component<RadioGroupProps> = (rawProps) => {
  const props = mergeProps(
    { name: `pv-radio-group-${Math.random().toString(36).slice(2, 7)}`, disabled: false, invalid: false, size: "md" as const },
    rawProps
  );
  const [local, rest] = splitProps(props, [
    "name", "value", "onChange", "disabled", "invalid", "size", "children", "class", "label", "helperText", "errorText",
  ]);

  const [internalValue, setInternalValue] = createSignal(local.value ?? "");

  const value = () => local.value ?? internalValue();
  const setValue = (v: string) => {
    setInternalValue(v);
    local.onChange?.(v);
  };

  const ctx: RadioContextValue = {
    name: () => local.name,
    value,
    setValue,
    disabled: local.disabled,
    invalid: local.invalid,
    size: local.size,
  };

  return (
    <div
      class={[styles.pvRadioGroup, local.class ?? ""].filter(Boolean).join(" ")}
      {...rest}
      role="radiogroup"
      aria-invalid={local.invalid}
      aria-disabled={local.disabled}
    >
      <Show when={local.label}>
        <legend class={styles.pvRadioGroupLabel}>{local.label}</legend>
      </Show>
      <RadioContext.Provider value={ctx}>
        {local.children}
      </RadioContext.Provider>
      <Show when={local.errorText}>
        <p class={`${styles.pvRadioGroupMessage} ${styles.pvRadioGroupMessageError}`} role="alert">
          {local.errorText}
        </p>
      </Show>
      <Show when={local.helperText && !local.errorText}>
        <p class={styles.pvRadioGroupMessage}>{local.helperText}</p>
      </Show>
    </div>
  );
};

// ─── RadioButton ────────────────────────────────────────────────

export interface RadioButtonProps {
  value: string;
  label?: string;
  disabled?: boolean;
  invalid?: boolean;
  size?: "sm" | "md" | "lg";
  class?: string;
  id?: string;
}

/**
 * Individual radio button option within a RadioGroup
 */
const RadioButton: Component<RadioButtonProps> = (rawProps) => {
  const props = mergeProps({}, rawProps);
  const [local, rest] = splitProps(props, ["value", "label", "disabled", "invalid", "size", "class", "id"]);

  const ctx = useRadioContext();
  const checked = () => ctx.value() === local.value;
  const actualDisabled = () => local.disabled ?? ctx.disabled;
  const actualInvalid = () => local.invalid ?? ctx.invalid;
  const actualSize = () => local.size ?? ctx.size ?? "md";

  const id = local.id ?? `${ctx.name()}-${local.value}`;

  const handleChange = (e: Event) => {
    e.preventDefault();
    if (actualDisabled()) return;
    ctx.setValue(local.value);
  };

  return (
    <div class={[styles.pvRadioButtonWrap, local.class ?? ""].filter(Boolean).join(" ")} {...rest}>
      <input
        id={id}
        type="radio"
        name={ctx.name()}
        value={local.value}
        checked={checked()}
        disabled={actualDisabled()}
        onChange={handleChange}
        class={styles.pvRadioInput}
        aria-invalid={actualInvalid()}
        aria-disabled={actualDisabled()}
      />
      <label
        for={id}
        class={[
          styles.pvRadioLabel,
          actualDisabled() ? styles.pvRadioDisabled : "",
          actualInvalid() ? styles.pvRadioInvalid : "",
          styles[`pvRadio${actualSize().toUpperCase()}`],
        ].filter(Boolean).join(" ")}
      >
        <span class={styles.pvRadioDot} aria-hidden="true" />
        <Show when={local.label}>
          <span class={styles.pvRadioLabelText}>{local.label}</span>
        </Show>
      </label>
    </div>
  );
};

export { RadioGroup, RadioButton as Radio };
export default RadioButton;
