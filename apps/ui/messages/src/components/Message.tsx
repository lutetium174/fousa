import {
  Button,
  Badge,
  RepeatIcon,
  ChatIcon,
  FavouriteIcon,
  Divider,
  InputGroup,
  Avatar,
} from "components";
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
const Message: Component<MessageDetails> = (props) => {
  return (
    <>
      <section style={{ display: "flex", "flex-direction": "row", gap: "12px", alignItems: "center" }}>
        <Avatar size="sm" />
        <p style={{ margin: 0 }}>{props.message}</p>
      </section>
      <section>
        <InputGroup orientation="horizontal">
          <Button
            rounded
            variant="text"
            aria-label="reposts"
            icon={<RepeatIcon />}
          >
            {props.counters && (props.counters.reposts ?? 0 > 0) && <Badge
              rounded
              size="sm"
              severity="primary"
              value={props.counters.reposts}
            />}
          </Button>
          <Button
            rounded
            variant="text"
            aria-label="replies"
            icon={<ChatIcon />}
          >
            {props.counters && (props.counters.replies ?? 0 > 0) && <Badge
              rounded
              size="sm"
              severity="primary"
              value={props.counters.replies}
            />}
          </Button>
          <Button
            rounded
            variant="text"
            aria-label="likes"
            icon={<FavouriteIcon />}
          >
            {props.counters && (props.counters.likes ?? 0 > 0) && <Badge
              rounded
              size="sm"
              severity="primary"
              value={props.counters.likes}
            />}
          </Button>
        </InputGroup>
      </section>
      <Divider />
    </>
  );
};

export default Message;
