import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./divider.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export type DividerType = "solid" | "dashed" | "dotted";
export type DividerOrientation = "horizontal" | "vertical";
export type DividerAlignment = "start" | "center" | "end";
export type DividerLayout = "default" | "compact";

export interface DividerProps extends JSX.HTMLAttributes<HTMLDivElement> {
  /** Orientation of the divider: horizontal or vertical */
  orientation?: DividerOrientation;
  
  /** Type of divider line: solid, dashed, or dotted */
  type?: DividerType;
  
  /** Alignment of content: start, center, or end */
  align?: DividerAlignment;
  
  /** Layout density: default or compact */
  layout?: DividerLayout;
  
  /** Content to display in the divider (text, icon, etc.) */
  children?: JSX.Element;
}

/**
 * Divider component - separates content with a horizontal or vertical line.
 * Inspired by PrimeVue's Divider component.
 */
export const Divider: Component<DividerProps> = (rawProps) => {
  const props = mergeProps(
    {
      orientation: "horizontal" as DividerOrientation,
      type: "solid" as DividerType,
      align: "center" as DividerAlignment,
      layout: "default" as DividerLayout,
    },
    rawProps
  );

  const [local, rest] = splitProps(props, [
    "orientation",
    "type",
    "align",
    "layout",
    "children",
    "class",
  ]);

  const rootClasses = [
    styles.pvDivider,
    styles[`pvDivider${toPascalCase(local.orientation)}`],
    styles[`pvDivider${toPascalCase(local.type)}`],
    styles[`pvDivider${toPascalCase(local.align)}`],
    styles[`pvDividerLayout${toPascalCase(local.layout)}`],
    local.class ?? "",
  ].filter(Boolean).join(" ");

  return (
    <div class={rootClasses} {...rest}>
      <Show when={local.orientation === "horizontal" || !local.children}>
        <div class={styles.pvDividerLine} />
      </Show>
      
      <Show when={local.children}>
        <div class={styles.pvDividerContent}>
          {local.children}
        </div>
      </Show>
      
      <Show when={local.orientation === "horizontal" || !local.children}>
        <div class={styles.pvDividerLine} />
      </Show>
    </div>
  );
};

export default Divider;
