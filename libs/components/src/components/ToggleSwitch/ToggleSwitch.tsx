import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./toggle-switch.module.css";

export interface ToggleSwitchProps extends Omit<JSX.InputHTMLAttributes<HTMLInputElement>, "type" | "onChange"> {
  label?: string;
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
  invalid?: boolean;
  size?: "sm" | "md" | "lg";
  class?: string;
  onIcon?: JSX.Element;
  offIcon?: JSX.Element;
}

/**
 * Toggle switch component for boolean values (like InputSwitch in PrimeVue)
 */
const ToggleSwitch: Component<ToggleSwitchProps> = (rawProps) => {
  const props = mergeProps({ size: "md" as const, disabled: false, invalid: false }, rawProps);
  const [local, rest] = splitProps(props, [
    "label", "checked", "onChange", "disabled", "invalid", "size", "class", "onIcon", "offIcon", "id",
  ]);

  const id = local.id ?? `pv-toggle-${Math.random().toString(36).slice(2, 7)}`;

  const handleChange = (e: Event) => {
    e.preventDefault();
    if (local.disabled) return;
    const target = e.target as HTMLInputElement;
    local.onChange?.(target.checked);
  };

  return (
    <div class={[styles.pvToggleWrap, local.class ?? ""].filter(Boolean).join(" ")}>
      <input
        {...rest}
        id={id}
        type="checkbox"
        checked={local.checked ?? false}
        disabled={local.disabled}
        onChange={handleChange}
        class={styles.pvToggleInput}
        aria-invalid={local.invalid}
        aria-disabled={local.disabled}
      />
      <label
        for={id}
        class={[
          styles.pvToggleLabel,
          local.disabled ? styles.pvToggleDisabled : "",
          local.invalid ? styles.pvToggleInvalid : "",
          styles[`pvToggle${local.size.toUpperCase()}`],
        ].filter(Boolean).join(" ")}
      >
        <span class={styles.pvToggleSlider} aria-hidden="true">
          <Show when={local.checked ?? false}>
            <span class={styles.pvToggleIcon}>
              {local.onIcon ?? (
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="20 6 9 17 4 12" />
                </svg>
              )}
            </span>
          </Show>
          <Show when={!(local.checked ?? false)}>
            <span class={styles.pvToggleIcon}>
              {local.offIcon ?? (
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18" />
                  <line x1="6" y1="6" x2="18" y2="18" />
                </svg>
              )}
            </span>
          </Show>
        </span>
        <Show when={local.label}>
          <span class={styles.pvToggleLabelText}>{local.label}</span>
        </Show>
      </label>
    </div>
  );
};

export default ToggleSwitch;
