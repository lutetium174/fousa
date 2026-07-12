import {
  type Component,
  type JSX,
  splitProps,
  mergeProps,
  Show,
} from "solid-js";
import styles from "./input.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export type TextInputSize = "sm" | "md" | "lg";

export interface TextInputProps extends Omit<
  JSX.InputHTMLAttributes<HTMLInputElement>,
  "size"
> {
  label?: string;
  helperText?: string;
  errorText?: string;
  size?: TextInputSize;
  iconLeft?: JSX.Element;
  iconRight?: JSX.Element;
  invalid?: boolean;
  filled?: boolean;
}

export const Input: Component<TextInputProps> = (rawProps) => {
  const props = mergeProps({ size: "md" as TextInputSize }, rawProps);
  const [local, rest] = splitProps(props, [
    "label",
    "helperText",
    "errorText",
    "size",
    "iconLeft",
    "iconRight",
    "invalid",
    "filled",
    "class",
    "id",
  ]);

  const id = local.id ?? `pv-input-${Math.random().toString(36).slice(2, 7)}`;
  const isInvalid = () => local.invalid || !!local.errorText;

  const classes = [
    styles.pvInputWrap,
    styles[`pvInputWrap${local.size.toUpperCase()}`],
    local.iconLeft ? styles.pvInputWrapIconLeft : "",
    local.iconRight ? styles.pvInputWrapIconRight : "",
    local.filled ? styles.pvInputWrapFilled : "",
    isInvalid() ? styles.pvInputWrapInvalid : "",
  ]
    .filter(Boolean)
    .join(" ");

    console.log(`pvInputWrap${local.size.toUpperCase()}`)
  return (
    <div class={[styles.pvField, local.class].filter(Boolean).join(" ")}>
      <Show when={local.label}>
        <label class={styles.pvFieldLabel} for={id}>
          {local.label}
        </label>
      </Show>

      <div class={classes}>
        <Show when={local.iconLeft}>
          <span
            class={`${styles.pvInputIcon} ${styles.pvInputIconLeft}`}
            aria-hidden="true"
          >
            {local.iconLeft}
          </span>
        </Show>

        <input
          {...rest}
          id={id}
          class={styles.pvInput}
          aria-invalid={isInvalid()}
          aria-describedby={
            local.errorText
              ? `${id}-error`
              : local.helperText
                ? `${id}-helper`
                : undefined
          }
        />

        <Show when={local.iconRight}>
          <span
            class={`${styles.pvInputIcon} ${styles.pvInputIconRight}`}
            aria-hidden="true"
          >
            {local.iconRight}
          </span>
        </Show>
      </div>

      <Show when={local.errorText}>
        <p
          class={`${styles.pvFieldMessage} ${styles.pvFieldMessageError}`}
          id={`${id}-error`}
          role="alert"
        >
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

export default Input;
