import {
  Button,
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  Input,
  PlusIcon,
} from "components";
import { createSignal, type Component } from "solid-js";

// Define props interface for the Chat component
export interface ChatProps {
  senderId?: string;
  apiEndpoint?: string;
  placeholder?: string;
  buttonLabel?: string;
  showButtonIcon?: boolean;
  onMessageSent?: (message: string) => void;
  onError?: (error: string) => void;
}

/**
 * Chat Component - An embeddable chat component for the messages microfrontend
 * 
 * This component provides a chat interface that can be embedded anywhere in the app.
 * It includes a dialog for composing messages and a button to open the chat.
 */
const Chat: Component<ChatProps> = (props) => {
  const [showChat, setShowChat] = createSignal<boolean>(false);
  const [message, setMessage] = createSignal<string>("");

  const sendMessage = async () => {
    if (!message().trim()) return;

    try {
      const endpoint = props.apiEndpoint || "http://localhost:5283/fuse/send";
      const senderId = props.senderId || "afa6c209-02c9-42af-965d-c98d0bb9a366";
      
      await fetch(endpoint, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Sender: senderId,
          Content: message(),
        }),
      });
      
      // Notify parent component that message was sent
      props.onMessageSent?.(message());
      
      setShowChat(false);
      setMessage("");
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : String(error);
      props.onError?.(errorMessage);
      console.error("Failed to send message:", error);
    }
  };

  return showChat() ? (
    <Dialog open={showChat()} onClose={() => setShowChat(false)}>
      <DialogHeader>
        <h3>New Message</h3>
      </DialogHeader>
      <DialogContent>
        <Input 
          placeholder={props.placeholder || "Type your message..."} 
          value={message()}
          onInput={(e) => setMessage(e.currentTarget.value)}
          onKeyPress={(e) => e.key === "Enter" && sendMessage()}
        />
      </DialogContent>
      <DialogFooter>
        <Button variant="primary" onClick={sendMessage}>
          {props.buttonLabel || "Send"}
        </Button>
      </DialogFooter>
    </Dialog>
  ) : (
    <Button 
      icon={props.showButtonIcon !== false ? PlusIcon({}) : undefined}
      onClick={() => setShowChat(true)}
    >
      {props.showButtonIcon === false ? "Chat" : undefined}
    </Button>
  );
};

export default Chat;
