import { Checkbox } from "components";
import type { CheckboxProps } from "components";

export default {
  title: "Components/Checkbox",
  component: Checkbox,
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
  },
};

type Story = CheckboxProps;

export const Basic: Story = {
  label: "Basic Checkbox",
  checked: false,
};

export const Checked: Story = {
  label: "Checked Checkbox",
  checked: true,
};

export const Disabled: Story = {
  label: "Disabled Checkbox",
  checked: false,
  disabled: true,
};

export const DisabledChecked: Story = {
  label: "Disabled Checked",
  checked: true,
  disabled: true,
};

export const Invalid: Story = {
  label: "Invalid Checkbox",
  checked: false,
  invalid: true,
};

export const Small: Story = {
  label: "Small Checkbox",
  size: "sm",
};

export const Large: Story = {
  label: "Large Checkbox",
  size: "lg",
};
