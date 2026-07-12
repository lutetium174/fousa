import { type Component, type JSX, splitProps, mergeProps } from "solid-js";
import styles from "./card.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export interface CardProps {
  children?: JSX.Element;
  class?: string;
  variant?: "flat" | "outlined" | "elevated";
}

/**
 * Card container component - wraps header, content, and footer
 */
const Card: Component<CardProps> = (rawProps) => {
  const props = mergeProps({ variant: "flat" as const }, rawProps);
  const [local, rest] = splitProps(props, ["children", "class", "variant"]);

  return (
    <div
      class={[
        styles.pvCard,
        styles[`pvCard${toPascalCase(local.variant)}`],
        local.class ?? "",
      ].filter(Boolean).join(" ")}
      {...rest}
    >
      {local.children}
    </div>
  );
};

export { Card, CardHeader, CardContent, CardFooter };

export interface CardHeaderProps {
  children?: JSX.Element;
  class?: string;
  title?: string;
  subtitle?: string;
  icon?: JSX.Element;
  actions?: JSX.Element;
}

/**
 * Card header section with optional title, subtitle, icon, and actions
 */
const CardHeader: Component<CardHeaderProps> = (rawProps) => {
  const props = mergeProps({}, rawProps);
  const [local, rest] = splitProps(props, ["children", "class", "title", "subtitle", "icon", "actions"]);

  return (
    <div class={[styles.pvCardHeader, local.class ?? ""].filter(Boolean).join(" ")} {...rest}>
      {local.icon && <div class={styles.pvCardHeaderIcon}>{local.icon}</div>}
      <div class={styles.pvCardHeaderContent}>
        {local.title && <h3 class={styles.pvCardHeaderTitle}>{local.title}</h3>}
        {local.subtitle && <p class={styles.pvCardHeaderSubtitle}>{local.subtitle}</p>}
        {local.children}
      </div>
      {local.actions && <div class={styles.pvCardHeaderActions}>{local.actions}</div>}
    </div>
  );
};

export interface CardContentProps {
  children?: JSX.Element;
  class?: string;
}

/**
 * Card main content area
 */
const CardContent: Component<CardContentProps> = (rawProps) => {
  const props = mergeProps({}, rawProps);
  const [local, rest] = splitProps(props, ["children", "class"]);

  return (
    <div class={[styles.pvCardContent, local.class ?? ""].filter(Boolean).join(" ")} {...rest}>
      {local.children}
    </div>
  );
};

export interface CardFooterProps {
  children?: JSX.Element;
  class?: string;
}

/**
 * Card footer section
 */
const CardFooter: Component<CardFooterProps> = (rawProps) => {
  const props = mergeProps({}, rawProps);
  const [local, rest] = splitProps(props, ["children", "class"]);

  return (
    <div class={[styles.pvCardFooter, local.class ?? ""].filter(Boolean).join(" ")} {...rest}>
      {local.children}
    </div>
  );
};

export default Card;
