import { Button } from "components";
import type { ButtonProps } from "components";

export default {
  title: "Components/Button",
  component: Button,
  argTypes: {
    variant: {
      control: "select",
      options: ["primary", "secondary", "outlined", "text", "danger"],
    },
    size: {
      control: "select",
      options: ["sm", "md", "lg"],
    },
    disabled: {
      control: "boolean",
    },
    loading: {
      control: "boolean",
    },
    rounded: {
      control: "boolean",
    },
  },
};

type Story = ButtonProps & { children?: string };

export const Primary: Story = {
  children: "Primary Button",
  variant: "primary",
};

export const Secondary: Story = {
  children: "Secondary Button",
  variant: "secondary",
};

export const Outlined: Story = {
  children: "Outlined Button",
  variant: "outlined",
};

export const Text: Story = {
  children: "Text Button",
  variant: "text",
};

export const Danger: Story = {
  children: "Danger Button",
  variant: "danger",
};

export const Disabled: Story = {
  children: "Disabled Button",
  disabled: true,
};

export const Loading: Story = {
  children: "Loading Button",
  loading: true,
};

export const WithIcon: Story = {
  children: "With Icon",
};

export const Small: Story = {
  children: "Small Button",
  size: "sm",
};

export const Large: Story = {
  children: "Large Button",
  size: "lg",
};

export const Rounded: Story = {
  children: "Rounded",
  rounded: true,
};
