import { Badge } from "components";
import type { BadgeProps } from "components";

export default {
  title: "Components/Badge",
  component: Badge,
  argTypes: {
    severity: {
      control: "select",
      options: ["primary", "secondary", "success", "info", "warn", "danger"],
    },
    size: {
      control: "select",
      options: ["sm", "md", "lg"],
    },
    dot: {
      control: "boolean",
    },
    rounded: {
      control: "boolean",
    },
  },
};

type Story = BadgeProps & { children?: string };

export const Primary: Story = {
  value: "Primary",
  severity: "primary",
};

export const Secondary: Story = {
  value: "Secondary",
  severity: "secondary",
};

export const Success: Story = {
  value: "Success",
  severity: "success",
};

export const Info: Story = {
  value: "Info",
  severity: "info",
};

export const Warning: Story = {
  value: "Warning",
  severity: "warn",
};

export const Danger: Story = {
  value: "Danger",
  severity: "danger",
};

export const Dot: Story = {
  value: 5,
  severity: "danger",
  dot: true,
};

export const Small: Story = {
  value: "Small",
  size: "sm",
  severity: "primary",
};

export const Medium: Story = {
  value: "Med",
  size: "md",
  severity: "primary",
};

export const Large: Story = {
  value: "Large",
  size: "lg",
  severity: "primary",
};

export const Rounded: Story = {
  value: "Rounded",
  severity: "primary",
  rounded: true,
};
