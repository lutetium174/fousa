import { type Component } from "solid-js";
export interface ChatProps {
    onMessageSent?: (message: string) => void;
    onError?: (error: string) => void;
}
declare const Chat: Component<ChatProps>;
export default Chat;
