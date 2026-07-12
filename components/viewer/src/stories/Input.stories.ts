import { Input } from "components";
import type { TextInputProps } from "components";

export default {
  title: "Components/Input",
  component: Input,
  argTypes: {
    size: {
      control: "select",
      options: ["sm", "md", "lg"],
    },
    disabled: {
      control: "boolean",
    },
    invalid: {
      control: "boolean",
    },
    filled: {
      control: "boolean",
    },
  },
};

type Story = TextInputProps;

export const Basic: Story = {
  placeholder: "Enter text...",
  label: "Basic Input",
};

export const WithLabel: Story = {
  label: "Username",
  placeholder: "Enter your username",
};

export const WithHelperText: Story = {
  label: "Email",
  placeholder: "Enter your email",
  helperText: "We'll never share your email",
};

export const WithError: Story = {
  label: "Password",
  placeholder: "Enter password",
  errorText: "Password is required",
  invalid: true,
};

export const Disabled: Story = {
  label: "Disabled Input",
  placeholder: "Cannot edit",
  disabled: true,
};

export const Filled: Story = {
  label: "Filled Input",
  placeholder: "Filled style",
  filled: true,
};

export const Small: Story = {
  label: "Small Input",
  placeholder: "Small size",
  size: "sm",
};

export const Large: Story = {
  label: "Large Input",
  placeholder: "Large size",
  size: "lg",
};
