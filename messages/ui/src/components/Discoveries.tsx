import { createSignal, For, onMount } from "solid-js";
import Message, { type MessageDetails } from "./Message.tsx";

export const Discoveries = () => {
  const [discover, setDiscover] = createSignal<MessageDetails[]>([]);

  onMount(
    async () =>
      await fetch("http://localhost:5283/fuses", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Sender: "afa6c209-02c9-42af-965d-c98d0bb9a366",
          Culture: "de-DE",
        }),
      })
        .then(async (response) => {
          const data = await response.json();
          console.log("Fetched discoveries:", data);
          setDiscover(data);
        })
        .catch((error) => console.error("Failed to fetch discoveries:", error)),
  );

  return (
    <>
      <For each={discover()}>{(item) => <Message {...item} />}</For>
    </>
  );
};
