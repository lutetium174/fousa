import { type Component, type JSX, splitProps, mergeProps, Show } from "solid-js";
import styles from "./progress-bar.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export interface ProgressBarProps {
  value?: number;
  max?: number;
  label?: string;
  showValue?: boolean;
  size?: "sm" | "md" | "lg";
  variant?: "flat" | "raised";
  severity?: "primary" | "secondary" | "success" | "info" | "warn" | "danger";
  class?: string;
  id?: string;
}

/**
 * Progress bar component showing completion percentage
 */
const ProgressBar: Component<ProgressBarProps> = (rawProps) => {
  const props = mergeProps(
    { value: 0, max: 100, showValue: false, size: "md" as const, variant: "flat" as const, severity: "primary" as const },
    rawProps
  );
  const [local, rest] = splitProps(props, [
    "value", "max", "label", "showValue", "size", "variant", "severity", "class", "id",
  ]);

  const percentage = () => {
    const v = Math.min(local.value, local.max);
    return Math.round((v / local.max) * 100);
  };

  const displayValue = () => (local.showValue ? `${percentage()}%` : undefined);

  return (
    <div
      {...rest}
      class={[styles.pvProgressBar, local.class ?? ""].filter(Boolean).join(" ")}
      role="progressbar"
      aria-valuenow={local.value}
      aria-valuemin={0}
      aria-valuemax={local.max}
      aria-label={local.label ?? `Progress: ${percentage()}%`}
    >
      <Show when={local.label}>
        <span class={styles.pvProgressBarLabel}>{local.label}</span>
      </Show>
      <div
        class={[
          styles.pvProgressBarTrack,
          styles[`pvProgressBarTrack${toPascalCase(local.variant)}`],
          styles[`pvProgressBar${toPascalCase(local.size)}`],
        ].filter(Boolean).join(" ")}
      >
        <div
          class={[
            styles.pvProgressBarValue,
            styles[`pvProgressBarValue${toPascalCase(local.severity)}`],
          ].filter(Boolean).join(" ")}
          style={{ width: `${percentage()}%` }}
        />
        <Show when={displayValue()}>
          <span class={styles.pvProgressBarText}>{displayValue()}</span>
        </Show>
      </div>
    </div>
  );
};

export default ProgressBar;
