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
  message: string;
  counters?: {
    likes?: number;
    reposts?: number;
    replies?: number;
  };
};

const Message = (data: MessageDetails) => {
  return (
    <>
      <section style={{ display: "flex", "flex-direction": "row" }}>
        <Avatar size="sm" />
        <p>{data.message}</p>
      </section>
      <section>
        <InputGroup orientation="horizontal">
          <Button
            rounded
            variant="text"
            aria-label="reposts"
            icon={<RepeatIcon />}
          >
            {data.counters && (data.counters.reposts ?? 0 > 0) && <Badge
              rounded
              size="sm"
              severity="primary"
              value={data.counters.reposts}
            />}
          </Button>
          <Button
            rounded
            variant="text"
            aria-label="replies"
            icon={<ChatIcon />}
          >
            {data.counters && (data.counters.replies ?? 0 > 0) && <Badge
              rounded
              size="sm"
              severity="primary"
              value={data.counters.replies}
            />}
          </Button>
          <Button
            rounded
            variant="text"
            aria-label="likes"
            icon={<FavouriteIcon />}
          >
            {data.counters && (data.counters.likes ?? 0 > 0) && <Badge
              rounded
              size="sm"
              severity="primary"
              value={data.counters.likes}
            />}
          </Button>
        </InputGroup>
      </section>
      <Divider />
    </>
  );
};

export default Message;
