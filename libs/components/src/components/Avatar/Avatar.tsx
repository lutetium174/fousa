import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./avatar.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export type AvatarSize = "xs" | "sm" | "md" | "lg" | "xl";
export type AvatarShape = "circle" | "square";

export interface AvatarProps {
  label?: string;
  icon?: JSX.Element;
  image?: string;
  size?: AvatarSize;
  shape?: AvatarShape;
  class?: string;
  alt?: string;
}

/**
 * Avatar component for displaying user profile pictures or icons
 */
const Avatar: Component<AvatarProps> = (rawProps) => {
  const props = mergeProps(
    { size: "md" as AvatarSize, shape: "circle" as AvatarShape },
    rawProps
  );
  const [local, rest] = splitProps(props, ["label", "icon", "image", "size", "shape", "class", "alt"]);

  const showImage = () => local.image && !local.icon;
  const showIcon = () => local.icon && !local.image;
  const showLabel = () => local.label && !local.image && !local.icon;

  return (
    <div
      {...rest}
      class={[
        styles.pvAvatar,
        styles[`pvAvatar${toPascalCase(local.size)}`],
        styles[`pvAvatar${toPascalCase(local.shape)}`],
        local.class ?? "",
      ].filter(Boolean).join(" ")}
      aria-label={local.alt ?? local.label}
      role="img"
    >
      <Show when={showImage()}>
        <img
          src={local.image}
          alt={local.alt ?? local.label}
          class={styles.pvAvatarImage}
          onError={(e) => {
            (e.target as HTMLImageElement).style.display = "none";
          }}
        />
      </Show>
      <Show when={showIcon()}>
        <span class={styles.pvAvatarIcon}>{local.icon}</span>
      </Show>
      <Show when={showLabel()}>
        <span class={styles.pvAvatarLabel}>
          {(local.label ?? "").split(" ").map((n) => n[0]).join("")}
        </span>
      </Show>
    </div>
  );
};

/**
 * AvatarGroup component for displaying multiple avatars stacked together
 */
export interface AvatarGroupProps {
  children?: JSX.Element;
  class?: string;
  max?: number;
}

export const AvatarGroup: Component<AvatarGroupProps> = (rawProps) => {
  const props = mergeProps({ max: 5 }, rawProps);
  const [local, rest] = splitProps(props, ["children", "class", "max"]);

  return (
    <div
      {...rest}
      class={[styles.pvAvatarGroup, local.class ?? ""].filter(Boolean).join(" ")}
      role="group"
    >
      {local.children}
    </div>
  );
};

export default Avatar;
