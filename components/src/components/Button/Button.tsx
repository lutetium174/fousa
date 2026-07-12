import {type Component, splitProps, mergeProps, type JSX} from "solid-js";
import styles from "./button.module.css";
import {toPascalCase} from "../../utils/pascalCase";

export type ButtonVariant = "primary" | "secondary" | "outlined" | "text" | "danger";
export type ButtonSize    = "sm" | "md" | "lg";

export interface ButtonProps extends JSX.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?:  ButtonVariant;
  size?:     ButtonSize;
  loading?:  boolean;
  icon?:     JSX.Element;
  iconPos?:  "left" | "right";
  rounded?:  boolean;
  label?:    string;
}

export const Button: Component<ButtonProps> = (rawProps) => {
  const props = mergeProps(
    { variant: "primary" as ButtonVariant, size: "md" as ButtonSize, iconPos: "left" },
    rawProps
  );
  const [local, rest] = splitProps(props, [
    "variant", "size", "loading", "icon", "iconPos",
    "rounded", "label", "children", "class",
  ]);

  const classes = [
    styles.pvBtn,
    styles[`pvBtn${toPascalCase(local.variant)}`],
    styles[`pvBtn${toPascalCase(local.size)}`],
    local.rounded  ? styles.pvBtnRounded  : "",
    local.loading  ? styles.pvBtnLoading  : "",
    !local.label && !local.children ? styles.pvBtnIconOnly : "",
    local.class ?? "",
  ].filter(Boolean).join(" ")

  return (
    <button
      class={classes}
      {...rest}
      disabled={rest.disabled || local.loading}
      aria-busy={local.loading}
    >
      {local.loading && <span class={styles.pvBtnSpinner} aria-hidden="true" />}
      {!local.loading && local.icon && local.iconPos === "left" && (
        <span class={styles.pvBtnIcon}>{local.icon}</span>
      )}
      {(local.label || local.children) && (
        <span class={styles.pvBtnLabel}>{local.label ?? local.children}</span>
      )}
      {!local.loading && local.icon && local.iconPos === "right" && (
        <span class={styles.pvBtnIcon}>{local.icon}</span>
      )}
    </button>
  );
};

export default Button;