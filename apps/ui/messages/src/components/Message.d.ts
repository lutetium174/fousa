import { type Component } from "solid-js";
export type MessageDetails = {
    message: string;
    counters?: {
        likes?: number;
        reposts?: number;
        replies?: number;
    };
};
/**
 * Message Component - Displays a single message with interaction counters
 *
 * This component can be embedded using DynamicContainer from the main MFE.
 */
declare const Message: Component<MessageDetails>;
export default Message;
