import {
  type Component,
  type JSX,
  splitProps,
  mergeProps,
  createSignal,
  Show,
  onCleanup,
  createEffect,
  For,
} from "solid-js";
import styles from "./select.module.css";

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface SelectProps extends Omit<JSX.SelectHTMLAttributes<HTMLSelectElement>, "onChange" | "size"> {
  label?: string;
  options?: SelectOption[];
  value?: string;
  onChange?: (value: string) => void;
  disabled?: boolean;
  invalid?: boolean;
  size?: "sm" | "md" | "lg";
  placeholder?: string;
  helperText?: string;
  errorText?: string;
  filled?: boolean;
  class?: string;
  id?: string;
}

/**
 * Select dropdown component with optional label
 */
const Select: Component<SelectProps> = (rawProps) => {
  const props = mergeProps(
    { options: [] as SelectOption[], disabled: false, invalid: false, size: "md" as const, placeholder: "Select an option" },
    rawProps
  );
  const [local, rest] = splitProps(props, [
    "label", "options", "value", "onChange", "disabled", "invalid", "size", "placeholder", "helperText", "errorText", "filled", "class", "id",
  ]);

  const id = local.id ?? `pv-select-${Math.random().toString(36).slice(2, 7)}`;
  const isInvalid = () => local.invalid || !!local.errorText;

  const handleChange = (e: Event) => {
    if (local.disabled) return;
    const target = e.target as HTMLSelectElement;
    local.onChange?.(target.value);
  };

  return (
    <div class={[styles.pvField, local.class ?? ""].filter(Boolean).join(" ")}>
      <Show when={local.label}>
        <label class={styles.pvFieldLabel} for={id}>
          {local.label}
        </label>
      </Show>

      <div
        class={[
          styles.pvSelectWrap,
          styles[`pvSelectWrap${local.size.toUpperCase()}`],
          local.filled ? styles.pvSelectWrapFilled : "",
          isInvalid() ? styles.pvSelectWrapInvalid : "",
          local.disabled ? styles.pvSelectWrapDisabled : "",
        ].filter(Boolean).join(" ")}
      >
        <select
          {...rest}
          id={id}
          class={styles.pvSelect}
          disabled={local.disabled}
          onChange={handleChange}
          value={local.value}
          aria-invalid={isInvalid()}
          aria-disabled={local.disabled}
        >
          <Show when={!local.value && local.placeholder}>
            <option value="" disabled hidden>
              {local.placeholder}
            </option>
          </Show>
          <For each={local.options}>
            {(option) => (
              <option
                value={option.value}
                disabled={option.disabled}
              >
                {option.label}
              </option>
            )}
          </For>
        </select>
      </div>

      <Show when={local.errorText}>
        <p class={`${styles.pvFieldMessage} ${styles.pvFieldMessageError}`} id={`${id}-error`} role="alert">
          {local.errorText}
        </p>
      </Show>
      <Show when={local.helperText && !local.errorText}>
        <p class={styles.pvFieldMessage} id={`${id}-helper`}>
          {local.helperText}
        </p>
      </Show>
    </div>
  );
};

export default Select;
