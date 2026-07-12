import { Navbar } from "components";
import type { NavbarItem } from "components";
import { HomeIcon, UsersIcon, SettingsIcon } from "components";

export default {
  title: "Components/Navbar",
  component: Navbar,
  argTypes: {
    variant: {
      control: "select",
      options: ["flat", "outlined", "elevated"],
    },
  },
};

const itemsWithSubnav: NavbarItem[] = [
  {
    key: "home",
    label: "Home",
    icon: <HomeIcon />,
  },
  {
    key: "users",
    label: "Users",
    icon: <UsersIcon />,
    subItems: [
      { key: "users:all", label: "All Users" },
      { key: "users:active", label: "Active Users" },
      { key: "users:inactive", label: "Inactive Users" },
    ],
  },
  {
    key: "settings",
    label: "Settings",
    icon: <SettingsIcon />,
    subItems: [
      { key: "settings:general", label: "General" },
      { key: "settings:profile", label: "Profile" },
      { key: "settings:notifications", label: "Notifications" },
    ],
  },
];

const simpleItems: NavbarItem[] = [
  { key: "home", label: "Home", icon: <HomeIcon /> },
  { key: "users", label: "Users", icon: <UsersIcon /> },
  { key: "settings", label: "Settings", icon: <SettingsIcon /> },
];

export const Basic = () => <Navbar items={simpleItems} />;

export const WithSubnav = () => <Navbar items={itemsWithSubnav} />;

export const WithSelectedItem = () => (
  <Navbar items={itemsWithSubnav} selectedKey="users:active" />
);

export const WithSelectedParent = () => (
  <Navbar items={itemsWithSubnav} selectedKey="users" />
);
