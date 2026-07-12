import { type Component, JSX, splitProps, mergeProps } from "solid-js";
import styles from "./badge.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export type BadgeSeverity =
  | "primary"
  | "secondary"
  | "success"
  | "info"
  | "warn"
  | "danger";
export type BadgeSize = "sm" | "md" | "lg";

export interface BadgeProps {
  value?: string | number;
  severity?: BadgeSeverity;
  size?: BadgeSize;
  dot?: boolean;
  rounded?: boolean;
  class?: string;
}

export const Badge: Component<BadgeProps & { children?: JSX.Element }> = (
  rawProps,
) => {
  const props = mergeProps(
    { severity: "primary" as BadgeSeverity, size: "md" as BadgeSize },
    rawProps,
  );
  const [local, _] = splitProps(props, [
    "value",
    "severity",
    "size",
    "dot",
    "rounded",
    "children",
    "class",
  ]);

  const classes = [
    styles.pvBadge,
    styles[`pvBadge${toPascalCase(local.severity)}`],
    styles[`pvBadge${toPascalCase(local.size)}`],
    local.dot ? styles.pvBadgeDot : "",
    local.rounded ? styles.pvBadgeRounded : "",
    local.class ?? "",
  ]
    .filter(Boolean)
    .join(" ");

  const pill = (
    <span
      class={classes}
      aria-label={local.dot ? String(local.value ?? "") : undefined}
    >
      {!local.dot && (local.value ?? "")}
    </span>
  );

  if (local.children) {
    return (
      <span class={styles.pvBadgeHost}>
        {local.children}
        {pill}
      </span>
    );
  }

  return pill;
};

export default Badge;
