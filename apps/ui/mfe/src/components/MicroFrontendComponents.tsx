/**
 * MicroFrontendComponents.tsx
 * 
 * This file demonstrates various ways to use DynamicContainer components
 * to load and display components from different microfrontends.
 */

import { 
  DynamicContainer, 
  BatchDynamicContainer,
  createDynamicContainer 
} from "../core";

// Pre-configured containers for specific microfrontends

// Messages MFE container factory
const MessagesContainer = createDynamicContainer(
  "http://localhost:5174/src/components/index.ts",
  "/messages"
);

// Search MFE container factory  
const SearchContainer = createDynamicContainer(
  "http://localhost:5175/src/components/index.ts",
  "/search"
);

// Notifications MFE container factory
const NotificationsContainer = createDynamicContainer(
  "http://localhost:5176/src/components/index.ts", 
  "/notifications"
);

/**
 * Example 1: Simple usage - Load a single component from messages MFE
 */
export function MessageCreatorWidget() {
  return (
    <DynamicContainer
      mfeName="http://localhost:5174/src/components/index.ts"
      componentName="MessageCreator"
      basePath="/messages"
      className="mfe-widget"
    />
  );
}

/**
 * Example 2: Using the factory pattern for messages MFE
 */
export function MessageListWidget(props: { limit: number }) {
  return (
    <MessagesContainer.Component
      componentName="MessageList"
      componentProps={{ limit: props.limit }}
      className="mfe-widget message-list"
    />
  );
}

/**
 * Example 3: Batch loading multiple components from the same MFE
 */
export function MessagesDashboard() {
  return (
    <BatchDynamicContainer
      mfeName="http://localhost:5174/src/components/index.ts"
      basePath="/messages"
      className="messages-dashboard"
      components={[
        {
          componentName: "MessageCreator",
          props: { showHeader: true },
          className: "dashboard-creator"
        },
        {
          componentName: "MessageList", 
          props: { limit: 5 },
          className: "dashboard-list"
        },
        {
          componentName: "MessageDetail",
          props: { messageId: "latest" },
          className: "dashboard-detail"
        }
      ]}
    />
  );
}

/**
 * Example 4: Cross-MFE dashboard with components from different microfrontends
 */
export function CrossMfeDashboard() {
  return (
    <div class="cross-mfe-dashboard">
      <h2>Cross-Microfrontend Dashboard</h2>
      
      <div class="dashboard-section">
        <h3>Messages</h3>
        <MessagesContainer.Component
          componentName="MessageCreator"
          className="cross-mfe-component"
        />
      </div>

      <div class="dashboard-section">
        <h3>Search</h3>
        <SearchContainer.Component
          componentName="SearchBar"
          className="cross-mfe-component"
        />
      </div>

      <div class="dashboard-section">
        <h3>Notifications</h3>
        <NotificationsContainer.Component
          componentName="NotificationBadge"
          className="cross-mfe-component"
        />
      </div>
    </div>
  );
}

/**
 * Example 5: Conditional loading with error handling
 */
export function SafeMessageCreator(props: { 
  onError: (error: string) => void;
  onLoad: () => void;
}) {
  return (
    <DynamicContainer
      mfeName="http://localhost:5174/src/components/index.ts"
      componentName="MessageCreator"
      basePath="/messages"
      onError={props.onError}
      onLoad={() => props.onLoad()}
      className="safe-container"
    />
  );
}