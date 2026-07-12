import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./textarea.module.css";

export interface TextareaProps extends Omit<JSX.TextareaHTMLAttributes<HTMLTextAreaElement>, "onChange" | "size"> {
  label?: string;
  value?: string;
  onChange?: (value: string) => void;
  disabled?: boolean;
  invalid?: boolean;
  size?: "sm" | "md" | "lg";
  helperText?: string;
  errorText?: string;
  filled?: boolean;
  autoResize?: boolean;
  rows?: number;
  class?: string;
  id?: string;
}

/**
 * Textarea component for multi-line text input
 */
const Textarea: Component<TextareaProps> = (rawProps) => {
  const props = mergeProps(
    { disabled: false, invalid: false, size: "md" as const, filled: false, autoResize: false, rows: 3 },
    rawProps
  );
  const [local, rest] = splitProps(props, [
    "label", "value", "onChange", "disabled", "invalid", "size", "helperText", "errorText", "filled", "autoResize", "rows", "class", "id",
  ]);

  const id = local.id ?? `pv-textarea-${Math.random().toString(36).slice(2, 7)}`;
  const isInvalid = () => local.invalid || !!local.errorText;

  const handleChange = (e: Event) => {
    if (local.disabled) return;
    const target = e.target as HTMLTextAreaElement;
    local.onChange?.(target.value);
    if (local.autoResize) {
      target.style.height = "auto";
      target.style.height = `${target.scrollHeight}px`;
    }
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
          styles.pvTextareaWrap,
          styles[`pvTextareaWrap${local.size.toUpperCase()}`],
          local.filled ? styles.pvTextareaWrapFilled : "",
          isInvalid() ? styles.pvTextareaWrapInvalid : "",
          local.disabled ? styles.pvTextareaWrapDisabled : "",
        ].filter(Boolean).join(" ")}
      >
        <textarea
          {...rest}
          id={id}
          class={styles.pvTextarea}
          disabled={local.disabled}
          onInput={handleChange}
          value={local.value}
          rows={local.rows}
          aria-invalid={isInvalid()}
          aria-disabled={local.disabled}
          aria-describedby={
            local.errorText ? `${id}-error`
              : local.helperText ? `${id}-helper`
                : undefined
          }
        />
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

export default Textarea;
