import {createSignal, For} from "solid-js";

export const Following = () => {
  const [following, setFollowing] = createSignal<string[]>([]);

  setFollowing(["Following 1", "Following 2", "Following 3"]);

  return <>
    <For each={following()}>
      {(item) => <div>{item}</div>}
    </For>
  </>
}