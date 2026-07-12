import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./input-group.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export type InputGroupOrientation = "horizontal" | "vertical";
export type InputGroupLayout = "default" | "compact";

export interface InputGroupProps extends JSX.HTMLAttributes<HTMLDivElement> {
  /** Orientation of the input group: horizontal or vertical */
  orientation?: InputGroupOrientation;
  
  /** Layout density: default or compact (compact removes gaps between elements) */
  layout?: InputGroupLayout;
  
  /** Label text displayed above the input group */
  label?: string;
  
  /** Content to display in the divider (text, icon, etc.) */
  children?: JSX.Element;
}

/**
 * InputGroup component - groups form elements together with consistent spacing.
 * Inspired by PrimeVue's InputGroup component.
 */
export const InputGroup: Component<InputGroupProps> = (rawProps) => {
  const props = mergeProps(
    {
      orientation: "horizontal" as InputGroupOrientation,
      layout: "default" as InputGroupLayout,
    },
    rawProps
  );

  const [local, rest] = splitProps(props, [
    "orientation",
    "layout",
    "label",
    "children",
    "class",
  ]);

  const rootClasses = [
    styles.pvInputGroup,
    styles[`pvInputGroup${toPascalCase(local.orientation)}`],
    styles[`pvInputGroup${toPascalCase(local.layout)}`],
    local.class ?? "",
  ].filter(Boolean).join(" ");

  return (
    <div class={rootClasses} {...rest}>
      <Show when={local.label}>
        <label class={styles.pvInputGroupLabel}>
          {local.label}
        </label>
      </Show>
      {local.children}
    </div>
  );
};

/**
 * InputGroupAddon - prefix or suffix addon for InputGroup
 */
export interface InputGroupAddonProps extends JSX.HTMLAttributes<HTMLDivElement> {
  children?: JSX.Element;
}

export const InputGroupAddon: Component<InputGroupAddonProps> = (rawProps) => {
  const props = rawProps;
  const [local, rest] = splitProps(props, ["children", "class"]);

  return (
    <div class={[styles.pvInputGroupAddon, local.class ?? ""].filter(Boolean).join(" ")} {...rest}>
      {local.children}
    </div>
  );
};

export default InputGroup;
