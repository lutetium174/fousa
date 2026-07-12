import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./checkbox.module.css";

export interface CheckboxProps extends Omit<JSX.InputHTMLAttributes<HTMLInputElement>, "type" | "onChange"> {
  label?: string;
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
  invalid?: boolean;
  size?: "sm" | "md" | "lg";
  class?: string;
  icon?: JSX.Element;
}

export const Checkbox: Component<CheckboxProps> = (rawProps) => {
  const props = mergeProps({ size: "md" as const, disabled: false, invalid: false }, rawProps);
  const [local, rest] = splitProps(props, [
    "label", "checked", "onChange", "disabled", "invalid", "size", "class", "icon", "id",
  ]);

  const id = local.id ?? `pv-checkbox-${Math.random().toString(36).slice(2, 7)}`;

  const handleChange = (e: Event) => {
    e.preventDefault();
    if (local.disabled) return;
    const target = e.target as HTMLInputElement;
    local.onChange?.(target.checked);
  };

  return (
    <div class={[styles.pvCheckboxWrap, local.class ?? ""].filter(Boolean).join(" ")}>
      <input
        {...rest}
        id={id}
        type="checkbox"
        checked={local.checked ?? false}
        disabled={local.disabled}
        onChange={handleChange}
        class={styles.pvCheckboxInput}
        aria-invalid={local.invalid}
        aria-disabled={local.disabled}
      />
      <label
        for={id}
        class={[
          styles.pvCheckboxLabel,
          local.disabled ? styles.pvCheckboxDisabled : "",
          local.invalid ? styles.pvCheckboxInvalid : "",
          styles[`pvCheckbox${local.size.toUpperCase()}`],
        ].filter(Boolean).join(" ")}
      >
        <Show when={local.checked ?? false}>
          <span class={styles.pvCheckboxCheckIcon} aria-hidden="true">
            {local.icon ?? (
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <polyline points="20 6 9 17 4 12" />
              </svg>
            )}
          </span>
        </Show>
        {local.label && <span class={styles.pvCheckboxLabelText}>{local.label}</span>}
      </label>
    </div>
  );
};

export default Checkbox;
