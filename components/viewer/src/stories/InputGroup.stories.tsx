import { InputGroup, InputGroupAddon } from "components";
import { Input } from "components";
import { Button } from "components";
import { SearchIcon, CalendarIcon } from "components";

export default {
  title: "Components/InputGroup",
  component: InputGroup,
  argTypes: {
    orientation: {
      control: "select",
      options: ["horizontal", "vertical"],
    },
    layout: {
      control: "select",
      options: ["default", "compact"],
    },
  },
};

export const Default = () => (
  <InputGroup>
    <Input placeholder="Enter text" />
    <Button>Submit</Button>
  </InputGroup>
);

export const WithLabel = () => (
  <InputGroup label="Search">
    <Input placeholder="Enter search term" />
    <Button icon={<SearchIcon />} />
  </InputGroup>
);

export const Horizontal = () => (
  <InputGroup orientation="horizontal">
    <Input placeholder="Left" />
    <Input placeholder="Right" />
  </InputGroup>
);

export const Vertical = () => (
  <InputGroup orientation="vertical">
    <Input placeholder="Top" />
    <Input placeholder="Bottom" />
  </InputGroup>
);

export const WithAddons = () => (
  <InputGroup>
    <InputGroupAddon>$</InputGroupAddon>
    <Input placeholder="Amount" />
    <InputGroupAddon>.00</InputGroupAddon>
  </InputGroup>
);

export const WithIconAddons = () => (
  <InputGroup>
    <InputGroupAddon>
      <CalendarIcon />
    </InputGroupAddon>
    <Input placeholder="Select date" />
    <InputGroupAddon>
      <SearchIcon />
    </InputGroupAddon>
  </InputGroup>
);

export const CompactLayout = () => (
  <InputGroup layout="compact">
    <InputGroupAddon>$</InputGroupAddon>
    <Input placeholder="Amount" />
    <InputGroupAddon>.00</InputGroupAddon>
  </InputGroup>
);

export const CompactWithButton = () => (
  <InputGroup layout="compact">
    <Input placeholder="Search..." />
    <Button icon={<SearchIcon />} />
  </InputGroup>
);

export const MultipleInputs = () => (
  <InputGroup label="Price Range">
    <Input placeholder="Min" />
    <InputGroupAddon>to</InputGroupAddon>
    <Input placeholder="Max" />
  </InputGroup>
);
