# Messages Microfrontend - Embeddable Components

This microfrontend provides embeddable components that can be loaded and rendered by the main application using the DynamicContainer system.

## Available Components

### Chat
A chat interface component that provides message composition and submission.

**Props:**
```typescript
interface ChatProps {
  senderId?: string;           // Sender identifier (default: "afa6c209-02c9-42af-965d-c98d0bb9a366")
  apiEndpoint?: string;       // API endpoint for message submission (default: "http://localhost:5283/fuse/send")
  placeholder?: string;        // Input placeholder text (default: "Type your message...")
  buttonLabel?: string;        // Send button label (default: "Send")
  showButtonIcon?: boolean;    // Whether to show icon on button (default: true)
  onMessageSent?: (message: string) => void;  // Callback when message is sent
  onError?: (error: string) => void;          // Callback when error occurs
}
```

**Usage in Main App:**
```tsx
import { DynamicContainer } from "./core";

<DynamicContainer
  mfeUrl="http://localhost:5174/src/components/index.ts"
  componentName="Chat"
  basePath="/messages"
  props={{
    senderId: "custom-sender-id",
    placeholder: "Type your message here...",
    onMessageSent: (message) => console.log("Message sent:", message),
    onError: (error) => console.error("Error:", error)
  }}
/>
```

### Message
A component to display individual messages with counters.

**Props:**
```typescript
interface MessageProps {
  message: string;              // The message content
  counters?: {
    likes?: number;            // Number of likes
    reposts?: number;          // Number of reposts
    replies?: number;          // Number of replies
  };
}
```

**Usage in Main App:**
```tsx
import { DynamicContainer } from "./core";

<DynamicContainer
  mfeUrl="http://localhost:5174/src/components/index.ts"
  componentName="Message"
  basePath="/messages"
  props={{
    message: "Hello, this is a test message!",
    counters: { likes: 5, reposts: 2, replies: 3 }
  }}
/>
```

### Discoveries
A component for displaying message discoveries.

### Following
A component for displaying following user lists.

## Development Setup

1. **Install dependencies:**
```bash
cd messages
pnpm install
```

2. **Run development server:**
```bash
pnpm run dev
```

The microfrontend will be available at `http://localhost:5174` with CORS enabled for cross-origin loading from the main application.

## Component Export Structure

All embeddable components are exported from `src/components/index.ts`:

```typescript
// src/components/index.ts
export { default as Chat, type ChatProps } from "./Chat";
export { default as Message, type MessageDetails } from "./Message";
export { default as Discoveries } from "./Discoveries";
export { default as Following } from "./Following";
```

## Adding New Embeddable Components

To make a new component embeddable:

1. **Create the component** in the `src/components` directory
2. **Export it** from `src/components/index.ts`
3. **Register it** in the main app's `MicroFrontendComponentRegistry.ts`

**Example:**
```typescript
// src/components/NewComponent.tsx
export interface NewComponentProps {
  // Define your props
}

const NewComponent: Component<NewComponentProps> = (props) => {
  // Component implementation
  return <div>New Component</div>;
};

export default NewComponent;
```

```typescript
// src/components/index.ts
export { default as NewComponent, type NewComponentProps } from "./NewComponent";
```

```typescript
// In main app's MicroFrontendComponentRegistry.ts
{
  name: "messages",
  url: "http://localhost:5174/src/components/index.ts",
  basePath: "/messages",
  componentName: "NewComponent",
  props: {},
}
```

## Dependencies

This microfrontend uses the shared `components` library for UI primitives. Ensure that:

1. The `components` package is available in the workspace
2. The Vite config properly resolves the `components` import
3. Both the main app and this MFE use compatible versions of `components`

## Styling

Components should include their own styling. Use one of these approaches:

- **CSS Modules**: `import styles from "./Component.module.css"`
- **CSS-in-JS**: Inline styles or styled components
- **Global CSS**: Import CSS files in component files

## Context Access

If your component needs access to global state or context:

```typescript
import { useGlobalContext } from "../core/GlobalContext.tsx";

const ContextAwareComponent: Component = () => {
  const { state } = useGlobalContext();
  // Access global state
  return <div>User: {state().user?.name}</div>;
};
```

Note: The component must be wrapped in a `GlobalProvider` in the main app for context to be available.

## Production Build

To build for production:
```bash
pnpm run build
```

The build output will be optimized and ready for deployment.

## CORS Configuration

For development, CORS is automatically configured to allow loading from:
- `http://localhost:5173` (main MFE)
- `http://127.0.0.1:5173` (main MFE)

For production, ensure your hosting service has proper CORS headers configured.
