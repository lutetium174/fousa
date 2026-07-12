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

export type MessageDetails = {
  content: string;
  counters: {
    likes?: number;
    reposts?: number;
    replies?: number;
  };
};

const Message = (message: MessageDetails) => {
  return (
    <>
      <section style={{ display: "flex", "flex-direction": "row" }}>
        <Avatar size="sm" />
        <p>{message.content}</p>
      </section>
      <section>
        <InputGroup orientation="horizontal">
          <Button
            rounded
            variant="text"
            aria-label="reposts"
            icon={<RepeatIcon />}
          >
            <Badge
              rounded
              size="sm"
              severity="primary"
              value={message.counters.reposts}
            />
          </Button>
          <Button
            rounded
            variant="text"
            aria-label="replies"
            icon={<ChatIcon />}
          >
            <Badge
              rounded
              size="sm"
              severity="primary"
              value={message.counters.replies}
            />
          </Button>
          <Button
            rounded
            variant="text"
            aria-label="likes"
            icon={<FavouriteIcon />}
          >
            <Badge
              rounded
              size="sm"
              severity="primary"
              value={message.counters.likes}
            />
          </Button>
        </InputGroup>
      </section>
      <Divider />
    </>
  );
};

export default Message;
