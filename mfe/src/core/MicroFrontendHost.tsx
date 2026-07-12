import { createEffect, createSignal, onCleanup } from "solid-js";
import type { MicroFrontendDefinition } from "./MicroFrontendTypes.ts";
import { mountMicroFrontend } from "./MicroFrontendLoader.ts";
import { Badge, SpinnerIcon } from "components";
import { useRouter } from "../contexts/RouteContext.tsx";
import { useI18n } from "../i18n/index.tsx";

type MicroFrontendHostProps = {
  mfe: MicroFrontendDefinition;
};

export const MicroFrontendHost = (props: MicroFrontendHostProps) => {
  let containerRef: HTMLDivElement | undefined;

  const {i18n} = useI18n();

  const [error, setError] = createSignal<string | null>(null);
  const [loading, setLoading] = createSignal(true);
  const [load, setLoad] = createSignal<string>("");
  const { route } = useRouter();

  createEffect(() => {
    if (containerRef) containerRef.innerHTML = "";
    setError(null);
    setLoading(true);

    let cancelled = false;
    let unmountFn: (() => void) | null = null;

    (async () => {
      console.log(`loading: ${route().path}`);
      try {
        setLoad((_) => `${props.mfe.url}?_cb=${Date.now()}`);
        const unmount = await mountMicroFrontend(
          load(),
          containerRef!,
          props.mfe.basePath,
        );

        if (cancelled) return unmount();

        unmountFn = unmount;
      } catch (err) {
        if (!cancelled) {
          const msg = err instanceof Error ? err.message : String(err);
          setLoading(false);
          setError(msg);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    onCleanup(() => {
      cancelled = true;
      try {
        unmountFn?.();
      } catch (e) {
        console.warn("unmount cleanup failed", e);
      }
      if (containerRef) containerRef.innerHTML = "";
    });
  });

  import.meta.hot?.dispose(() => {
    if (containerRef) containerRef.innerHTML = "";
  });

  return (
    <div class="mfe-host">
      {loading() && <SpinnerIcon spin />}
      {error() && <Badge severity="danger" value={`${i18n("errorRefresh")}`} />}
      <div ref={containerRef} class="mfe-container" id="mfe-container" />
    </div>
  );
};
