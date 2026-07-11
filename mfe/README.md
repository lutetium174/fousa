## Usage

```bash
$ npm install # or pnpm install or yarn install
```

### Learn more on the [Solid Website](https://solidjs.com) and come chat with us on our [Discord](https://discord.com/invite/solidjs)

## Available Scripts

In the project directory, you can run:

### `npm run dev`

Runs the app in the development mode.<br>
Open [http://localhost:5173](http://localhost:5173) to view it in the browser.

### `npm run build`

Builds the app for production to the `dist` folder.<br>
It correctly bundles Solid in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.<br>
Your app is ready to be deployed!

## Deployment

Learn more about deploying your application with the [documentations](https://vite.dev/guide/static-deploy.html)

---

## Dynamic Microfrontend Containers

This project includes a **DynamicContainer** system that allows you to embed specific components from microfrontends anywhere in your application while maintaining the microfrontend's context and isolation.

### Overview

The DynamicContainer system provides:

- **`DynamicContainer`**: Load and render specific components from any microfrontend
- **`createDynamicContainer`**: Factory function for creating specialized containers for specific MFEs
- **`BatchDynamicContainer`**: Load multiple components from the same MFE in parallel
- **Context Isolation**: Each component maintains its own microfrontend context
- **Caching**: Components are cached to avoid duplicate loading
- **Error Handling**: Built-in error states and recovery

### Quick Start

```tsx
import { DynamicContainer, createDynamicContainer } from "./core";

// Method 1: Direct usage
<DynamicContainer
  mfeUrl="http://localhost:5174/src/components/index.ts"
  componentName="MessageCreator"
  basePath="/messages"
  props={{ theme: "dark" }}
  onError={(error) => console.error(error)}
/>

// Method 2: Factory pattern for reusable containers
const Messages = createDynamicContainer(
  "http://localhost:5174/src/components/index.ts",
  "/messages"
);

<Messages.Component
  componentName="MessageList"
  componentProps={{ limit: 10 }}
/>
```

---

## Creating Embeddable Components for Microfrontends

To make components from your microfrontend embeddable using DynamicContainer, follow these guidelines:

### 1. Component Export Structure

Your microfrontend must export components as **named exports** from an entry file. The DynamicContainer system uses dynamic imports to load these components.

**Example: Messages Microfrontend Structure**

```
messages-mfe/
├── src/
│   ├── components/
│   │   ├── MessageCreator.tsx      # The component to embed
│   │   ├── MessageList.tsx         # Another embeddable component
│   │   └── index.ts                # Export point
│   ├── App.tsx
│   └── index.ts
└── vite.config.ts
```

**`messages-mfe/src/components/index.ts`**
```typescript
// Re-export all embeddable components
export { default as MessageCreator } from "./MessageCreator";
export { default as MessageList } from "./MessageList";
export { default as MessageDetail } from "./MessageDetail";
```

**`messages-mfe/src/components/MessageCreator.tsx`**
```tsx
import { type Component } from "solid-js";
import type { MessageCreatorProps } from "./types";

// Define your component props interface
export interface MessageCreatorProps {
  onCreate?: (message: string) => void;
  theme?: "light" | "dark";
  placeholder?: string;
}

// Component must be a default export or named export
const MessageCreator: Component<MessageCreatorProps> = (props) => {
  const [content, setContent] = createSignal("");

  const handleSubmit = (e: Event) => {
    e.preventDefault();
    props.onCreate?.(content());
    setContent("");
  };

  return (
    <form onSubmit={handleSubmit} class="message-creator">
      <textarea
        value={content()}
        onInput={(e) => setContent(e.currentTarget.value)}
        placeholder={props.placeholder || "Type your message..."}
        class={props.theme || "light"}
      />
      <button type="submit">Send</button>
    </form>
  );
};

export default MessageCreator;
```

### 2. Vite Configuration for Component Loading

Ensure your microfrontend's Vite config exposes the components directory:

**`messages-mfe/vite.config.ts`**
```typescript
import { defineConfig } from "vite";
import solidPlugin from "vite-plugin-solid";

export default defineConfig({
  plugins: [solidPlugin()],
  server: {
    port: 5174, // Messages MFE runs on port 5174
    cors: true, // Enable CORS for cross-origin loading
  },
  build: {
    // Ensure components can be loaded individually
    rollupOptions: {
      input: {
        main: "src/index.ts",
        components: "src/components/index.ts", // Expose components entry
      },
      output: {
        entryFileNames: "[name].js",
      },
    },
  },
});
```

### 3. Required Component Characteristics

For components to work with DynamicContainer:

1. **Must be SolidJS components** using `createSignal`, `createEffect`, etc.
2. **Must be exported** (default or named exports both work)
3. **Should accept props** via a well-defined interface
4. **Should be self-contained** with their own styling and state
5. **Should not depend on microfrontend-specific context** (or should handle missing context gracefully)

### 4. Component Props and State Management

Components should manage their own state internally and expose a clean props interface:

```tsx
// Good: Self-contained component
export interface ChatWidgetProps {
  initialMessage?: string;
  onSend?: (message: string) => void;
  maxLength?: number;
}

const ChatWidget: Component<ChatWidgetProps> = (props) => {
  // Internal state
  const [messages, setMessages] = createSignal<string[]>([
    props.initialMessage || ""
  ]);
  const [input, setInput] = createSignal("");

  // Internal logic
  const handleSend = () => {
    if (input().length > 0 && (props.maxLength === undefined || input().length <= props.maxLength)) {
      setMessages([...messages(), input()]);
      props.onSend?.(input());
      setInput("");
    }
  };

  return (
    <div class="chat-widget">
      <div class="messages">
        {messages().map((msg) => <div>{msg}</div>)}
      </div>
      <input 
        value={input()} 
        onInput={(e) => setInput(e.currentTarget.value)}
      />
      <button onClick={handleSend}>Send</button>
    </div>
  );
};

export default ChatWidget;
```

### 5. Handling Microfrontend Context

If your component needs access to the microfrontend's context (event bus, global state, etc.), use the `useGlobalContext` hook:

```tsx
import { useGlobalContext } from "../core/GlobalContext.tsx";

const ContextAwareComponent: Component = () => {
  const { state, setState } = useGlobalContext();
  
  // Component can access global state
  const user = state().user;
  
  return (
    <div>
      <p>Current user: {user?.name}</p>
    </div>
  );
};

export default ContextAwareComponent;
```

### 6. Styling Considerations

Components should include their own styling:

**Option A: CSS Modules**
```tsx
import styles from "./MessageCreator.module.css";

const MessageCreator: Component = () => {
  return (
    <div class={styles.container}>
      <form class={styles.form}>
        {/* ... */}
      </form>
    </div>
  );
};
```

**Option B: CSS-in-JS**
```tsx
const MessageCreator: Component = () => {
  return (
    <div style={{
      "border": "1px solid var(--border)",
      "padding": "16px",
      "border-radius": "8px"
    }}>
      {/* ... */}
    </div>
  );
};
```

**Option C: Global CSS (import in component file)**
```tsx
import "./MessageCreator.css";

// Component implementation...
```

### 7. Registering Components in the Main App

Once your microfrontend exports embeddable components, register them in the main app:

**`src/core/MicroFrontendComponentRegistry.ts`**
```typescript
import type { MicroFrontendComponentDefinition } from "./MicroFrontendTypes.ts";

export const messagesMfeComponents: MicroFrontendComponentDefinition[] = [
  {
    name: "messages",
    url: "http://localhost:5174/src/components/index.ts",
    basePath: "/messages",
    componentName: "MessageCreator",
    props: {},
  },
  {
    name: "messages",
    url: "http://localhost:5174/src/components/index.ts",
    basePath: "/messages",
    componentName: "MessageList",
    props: {},
  },
];
```

### 8. Using Embedded Components in the Main App

Now you can use the components anywhere in your main application:

```tsx
import { DynamicContainer, createDynamicContainer } from "./core";

// Create reusable container for messages MFE
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
      
      {/* Embed MessageCreator component */}
      <div class="sidebar">
        <Messages.Component
          componentName="MessageCreator"
          componentProps={{
            onCreate: (message) => console.log("New message:", message),
            theme: "dark",
            placeholder: "Create a new message..."
          }}
        />
      </div>
      
      {/* Embed MessageList component */}
      <div class="main-content">
        <Messages.Component
          componentName="MessageList"
          componentProps={{ limit: 50 }}
        />
      </div>
    </GlobalProvider>
  );
};
```

### 9. Batch Loading Components

For loading multiple components from the same MFE:

```tsx
import { BatchDynamicContainer } from "./core";

const MessagesDashboard = () => {
  return (
    <BatchDynamicContainer
      mfeUrl="http://localhost:5174/src/components/index.ts"
      basePath="/messages"
      components={[
        {
          componentName: "MessageCreator",
          props: { showHeader: true },
          className: "dashboard-creator"
        },
        {
          componentName: "MessageList",
          props: { limit: 10 },
          className: "dashboard-list"
        },
      ]}
    />
  );
};
```

### 10. Error Handling and Loading States

DynamicContainer provides built-in error handling and loading states:

```tsx
<DynamicContainer
  mfeUrl="http://localhost:5174/src/components/index.ts"
  componentName="MessageCreator"
  basePath="/messages"
  onError={(error) => {
    console.error("Failed to load component:", error);
    // Show fallback UI
  }}
  onLoad={(component) => {
    console.log("Component loaded:", component);
  }}
  className="error-handled-container"
/>
```

The container automatically displays:
- Loading spinner while component is being loaded
- Error message if loading fails
- Component content when successfully loaded

### 11. CORS Configuration

Ensure your microfrontend development server has CORS enabled:

**`vite.config.ts`**
```typescript
export default defineConfig({
  server: {
    cors: {
      origin: ["http://localhost:5173", "http://127.0.0.1:5173"],
      methods: ["GET", "POST", "PUT", "DELETE"],
      allowedHeaders: ["*"],
    },
  },
});
```

### 12. Production Deployment

For production, ensure:
1. Microfrontend components are built and accessible via HTTP
2. URLs in the main app point to the production locations
3. CORS headers are properly configured on your hosting

**Example production URLs:**
```typescript
// Development
mfeUrl="http://localhost:5174/src/components/index.ts"

// Production  
mfeUrl="https://mfe.yourdomain.com/messages/components.js"
```

---

## Troubleshooting

### Component not loading
- ✅ Check that the component is exported from the specified URL
- ✅ Verify the microfrontend server is running on the correct port
- ✅ Check CORS headers are properly configured
- ✅ Ensure the component name matches the export name

### TypeScript errors
- ✅ Verify component props interface is properly defined
- ✅ Check that all required props are provided
- ✅ Ensure the component is a valid SolidJS component

### Styling issues
- ✅ Components should include their own styles
- ✅ Check if styles are being loaded (CSS import or CSS-in-JS)
- ✅ Verify CSS variables are available in the host app

### Context not available
- ✅ If using global context, ensure the component is wrapped in GlobalProvider
- ✅ Check that the context hook is imported from the correct path
- ✅ Verify the microfrontend exports the context if needed
