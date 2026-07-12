import { Tabs, TabList, Tab, TabPanel } from "components";
import { ChatIcon, SearchIcon, NotificationIcon } from "components";

export default {
  title: "Components/Tabs",
  component: Tabs,
};

export const BasicTabs = () => (
  <Tabs variant="line" defaultValue="home">
    <TabList>
      <Tab value="chat" icon={<ChatIcon />}></Tab>
      <Tab value="search" icon={<SearchIcon />}></Tab>
      <Tab value="notification" icon={<NotificationIcon />}></Tab>
    </TabList>
    <TabPanel value="chat">Hello</TabPanel>
    <TabPanel value="search">World</TabPanel>
    <TabPanel value="notification">Notifications</TabPanel>
  </Tabs>
);

export const PillsVariant = () => (
  <Tabs defaultValue="tab1" variant="pills">
    <TabList>
      <Tab value="tab1">Tab 1</Tab>
      <Tab value="tab2">Tab 2</Tab>
      <Tab value="tab3">Tab 3</Tab>
    </TabList>
    <TabPanel value="tab1">Content for Tab 1</TabPanel>
    <TabPanel value="tab2">Content for Tab 2</TabPanel>
    <TabPanel value="tab3">Content for Tab 3</TabPanel>
  </Tabs>
);

export const PillsFilledVariant = () => (
  <Tabs defaultValue="tab1" variant="pills-filled">
    <TabList>
      <Tab value="tab1">Tab 1</Tab>
      <Tab value="tab2">Tab 2</Tab>
      <Tab value="tab3">Tab 3</Tab>
    </TabList>
    <TabPanel value="tab1">Content for Tab 1</TabPanel>
    <TabPanel value="tab2">Content for Tab 2</TabPanel>
    <TabPanel value="tab3">Content for Tab 3</TabPanel>
  </Tabs>
);

export const WithDisabledTab = () => (
  <Tabs defaultValue="tab1">
    <TabList>
      <Tab value="tab1">Tab 1</Tab>
      <Tab value="tab2" disabled>
        Tab 2 (Disabled)
      </Tab>
      <Tab value="tab3">Tab 3</Tab>
    </TabList>
    <TabPanel value="tab1">Content for Tab 1</TabPanel>
    <TabPanel value="tab2">Content for Tab 2</TabPanel>
    <TabPanel value="tab3">Content for Tab 3</TabPanel>
  </Tabs>
);
