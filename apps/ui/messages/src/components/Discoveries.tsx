import { createSignal, For, onMount, onCleanup } from "solid-js";
import Message, { type MessageDetails } from "./Message.tsx";
import type { MicroFrontendEnvironment } from "../types/MicroFrontendEnvironment.ts";

const languageToCulture: Record<string, string> = {
  en: "en-GB",
  de: "de-DE",
  fr: "fr-FR",
  es: "es-ES",
  it: "it-IT",
  pt: "pt-PT",
  ja: "ja-JP",
  ko: "ko-KR",
};

export const Discoveries = (props: { env: MicroFrontendEnvironment }) => {
  const [currentLanguage, setCurrentLanguage] = createSignal(
    props.env.globalContext.state.language,
  );
  const [discover, setDiscover] = createSignal<MessageDetails[]>([]);

  onMount(() => {
    const unsubscribe = props.env.eventBus.subscribe<string>(
      "i18n:language-change",
      setCurrentLanguage,
    );

    onCleanup(() => unsubscribe());
  });

  const getCulture = () => {
    const lang = currentLanguage();
    return languageToCulture[lang] || languageToCulture.en;
  };

  onMount(async () => {
    try {
      const response = await fetch("http://localhost:5283/fuses", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Sender: "afa6c209-02c9-42af-965d-c98d0bb9a366",
          Culture: getCulture(),
        }),
      });
      const data = await response.json();
      console.log("Fetched discoveries:", data);
      setDiscover(data);
    } catch (error) {
      console.error("Failed to fetch discoveries:", error);
    }
  });

  return (
    <>
      <For each={discover()}>{(item) => <Message {...item} />}</For>
    </>
  );
};
