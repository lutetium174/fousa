import {createSignal, For} from "solid-js";
import Message, {type MessageDetails} from "./Message.tsx";

export const Discoveries = () => {
  const [discover, setDiscover] = createSignal<MessageDetails[]>([]);

  setDiscover([
    {
      content: "Discover 1",
      counters: {}
    },
    {
      content: "Discover 2",
      counters: {likes: 1}
    },
    {
      content: "Discover 3",
      counters: {reposts: 2, replies: 3}
    }]);

  return <>
    <For each={discover()}>
      {(item) => <Message {...item} />}
    </For>
  </>
}