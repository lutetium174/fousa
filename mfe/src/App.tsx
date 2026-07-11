import { 
  GlobalProvider, 
  RootContainer, 
  DynamicContainer,
  createDynamicContainer,
} from "./core";
import { ThemeSwitcher } from "components";
import { LanguageSwitcher } from "./components/LanguageSwitcher";
import "./App.css";

// Create a specialized container for the messages microfrontend
const Messages = createDynamicContainer(
  "http://localhost:5174/src/components/index.ts",
  "/messages"
);

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
          mfeUrl="http://localhost:5174/src/components/index.ts"
          componentName="Chat"
          basePath="/messages"
          className="chat-container"
          props={{
            senderId: "afa6c209-02c9-42af-965d-c98d0bb9a366",
            placeholder: "Type your message here...",
            buttonLabel: "Send Message",
            showButtonIcon: true,
            onMessageSent: (message) => {
              console.log("Message sent from embedded Chat:", message);
            },
            onError: (error) => {
              console.error("Chat error:", error);
            }
          }}
          onError={(error) => console.error("Failed to load Chat component:", error)}
        />

        {/* Example of using the specialized container for Message component */}
        <Messages.Component
          componentName="Message"
          componentProps={{
            message: "This is an embedded message from the messages MFE!",
            counters: { likes: 5, reposts: 2, replies: 3 }
          }}
          className="message-container"
        />
      </div>
    </GlobalProvider>
  );
};

export default App;
