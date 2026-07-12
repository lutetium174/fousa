import { Card, CardHeader, CardContent, CardFooter } from "components";

export default {
  title: "Components/Card",
  component: Card,
  argTypes: {
    variant: {
      control: "select",
      options: ["flat", "outlined", "elevated"],
    },
  },
};

export const Basic = () => (
  <Card variant="flat">
    <CardHeader title="Card Title" subtitle="Subtitle" />
    <CardContent>This is the card content area.</CardContent>
    <CardFooter>Footer content here</CardFooter>
  </Card>
);

export const Outlined = () => (
  <Card variant="outlined">
    <CardHeader title="Outlined Card" />
    <CardContent>Content for outlined card</CardContent>
  </Card>
);

export const Elevated = () => (
  <Card variant="elevated">
    <CardHeader title="Elevated Card" />
    <CardContent>Content for elevated card with shadow</CardContent>
  </Card>
);

export const WithActions = () => (
  <Card variant="flat">
    <CardHeader title="Card with Actions" actions={<button>Action</button>} />
    <CardContent>Card content here</CardContent>
  </Card>
);

export const OnlyContent = () => (
  <Card variant="flat">
    <CardContent>Just content, no header or footer</CardContent>
  </Card>
);
