import { type Component, type JSX, mergeProps, splitProps } from "solid-js";
import styles from "./float-label.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export type FloatLabelVariant = "over" | "in" | "on";

export interface FloatLabelProps extends JSX.HTMLAttributes<HTMLDivElement> {
  variant?: FloatLabelVariant;
  class?: string;
}

export const FloatLabel: Component<FloatLabelProps> = (rawProps) => {
  const props = mergeProps({ variant: "over" as FloatLabelVariant }, rawProps);
  const [local, rest] = splitProps(props, ["variant", "class", "children"]);

  const wrapperClasses = [
    styles.pvFloatLabel,
    local.variant !== "over" ? styles[`pvFloatLabel${toPascalCase(local.variant)}`] : "",
    local.class ?? "",
  ].filter(Boolean).join(" ");

  return (
    <div class={wrapperClasses} {...rest}>
      {local.children}
    </div>
  );
};

export default FloatLabel;
