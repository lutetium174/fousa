import { Divider } from "components";
import type { DividerProps } from "components";

export default {
  title: "Components/Divider",
  component: Divider,
  argTypes: {
    orientation: {
      control: "select",
      options: ["horizontal", "vertical"],
    },
    type: {
      control: "select",
      options: ["solid", "dashed", "dotted"],
    },
    align: {
      control: "select",
      options: ["start", "center", "end"],
    },
    layout: {
      control: "select",
      options: ["default", "compact"],
    },
  },
};

type Story = DividerProps;

export const Default: Story = {};

export const Horizontal: Story = {
  orientation: "horizontal",
  children: "OR",
};

export const Vertical: Story = {
  orientation: "vertical",
};

export const Dashed: Story = {
  type: "dashed",
  children: "Dashed",
};

export const Dotted: Story = {
  type: "dotted",
  children: "Dotted",
};

export const WithText: Story = {
  children: "Divide",
};

export const AlignStart: Story = {
  children: "Start",
  align: "start",
};

export const AlignEnd: Story = {
  children: "End",
  align: "end",
};

export const Compact: Story = {
  children: "Compact",
  layout: "compact",
};
