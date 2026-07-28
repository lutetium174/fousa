import {
  GlobalProvider,
  RootContainer,
  DynamicContainer,
} from "./core";
import { ThemeSwitcher } from "components";
import { LanguageSwitcher } from "./components/LanguageSwitcher";
import "./App.css";

// Create a specialized container for the messages microfrontend
//const Messages = createDynamicContainer("messages", "/messages");

const App = () => {
  return (
    <GlobalProvider>
      <div class="app-controls">
        <ThemeSwitcher />
        <LanguageSwitcher />
      </div>
      <RootContainer />

      {/* Example of using DynamicContainer directly to load Chat from messages MFE */}
      <div class="dynamic-containers-section">
        <h2>Messages Microfrontend Components</h2>

        <DynamicContainer
          mfeName="messages"
          componentName="Chat"
          basePath="/messages"
          className="chat-container"
          props={{
            onMessageSent: (message: string) =>
              console.log("Message sent from embedded Chat:", message),
            onError: (error: any) => console.error("Chat error:", error),
          }}
          onError={(error) =>
            console.error("Failed to load Chat component:", error)
          }
        />
      </div>
    </GlobalProvider>
  );
};

export default App;
