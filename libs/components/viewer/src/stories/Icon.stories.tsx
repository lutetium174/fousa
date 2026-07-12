import {
  HomeIcon,
  ChatIcon,
  IconProps,
  RepeatIcon,
  SpinnerIcon,
  LoaderIcon
} from "components";

type Story = IconProps & { children?: string };

export default {
  title: "Components/Icons",
  component: HomeIcon,
};

export const Static = () => (
  <>
    <ChatIcon />
    <RepeatIcon />
  </>
);
export const Animated = () => (
  <>
    <SpinnerIcon spin />
    <LoaderIcon spin spinDuration={1.2} spinDirection="reverse" />
  </>  
);
