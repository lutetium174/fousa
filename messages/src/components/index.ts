// Export all embeddable components from the messages microfrontend
// These components can be loaded and rendered by the main MFE using DynamicContainer

export { default as Chat, type ChatProps } from "./Chat";
export { default as Message, type MessageDetails } from "./Message";
export { default as Discoveries } from "./Discoveries";
export { default as Following } from "./Following";

// Re-export all named exports for convenience
export type { MessageDetails as MessageProps } from "./Message";