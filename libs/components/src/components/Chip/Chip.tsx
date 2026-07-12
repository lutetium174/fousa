import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./chip.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export type ChipSeverity = "primary" | "secondary" | "success" | "info" | "warn" | "danger";

export interface ChipProps {
  label: string;
  severity?: ChipSeverity;
  size?: "sm" | "md" | "lg";
  icon?: JSX.Element;
  image?: string;
  removable?: boolean;
  onRemove?: () => void;
  class?: string;
  disabled?: boolean;
}

/**
 * Chip component - compact element for displaying tags, categories, or items
 */
const Chip: Component<ChipProps> = (rawProps) => {
  const props = mergeProps(
    { severity: "primary" as ChipSeverity, size: "md" as const, removable: false, disabled: false },
    rawProps
  );
  const [local, rest] = splitProps(props, [
    "label", "severity", "size", "icon", "image", "removable", "onRemove", "class", "disabled",
  ]);

  const handleRemove = (e: Event) => {
    e.preventDefault();
    e.stopPropagation();
    if (local.disabled) return;
    local.onRemove?.();
  };

  return (
    <div
      {...rest}
      class={[
        styles.pvChip,
        styles[`pvChip${toPascalCase(local.severity)}`],
        styles[`pvChip${local.size.toUpperCase()}`],
        local.disabled ? styles.pvChipDisabled : "",
        local.class ?? "",
      ].filter(Boolean).join(" ")}
      role="button"
      aria-label={local.label}
    >
      <Show when={local.image}>
        <img src={local.image} alt="" class={styles.pvChipImage} />
      </Show>
      <Show when={local.icon && !local.image}>
        <span class={styles.pvChipIcon}>{local.icon}</span>
      </Show>
      <span class={styles.pvChipLabel}>{local.label}</span>
      <Show when={local.removable}>
        <button
          type="button"
          onClick={handleRemove}
          class={styles.pvChipRemove}
          aria-label="Remove"
          disabled={local.disabled}
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </Show>
    </div>
  );
};

export default Chip;
