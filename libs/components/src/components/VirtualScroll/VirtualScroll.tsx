import {
  type Component,
  type JSX,
  createSignal,
  createEffect,
  onCleanup,
  onMount,
  mergeProps,
  splitProps,
  Show,
  Index
} from "solid-js";
import styles from "./virtual-scroll.module.css";

export interface VirtualScrollProps<T = any> {
  /** Array of items to display */
  items: T[];
  
  /** Function to render each item */
  itemTemplate: (item: T, index: number) => JSX.Element;
  
  /** Item height in pixels (required for proper virtualization) */
  itemSize?: number;
  
  /** Number of items to render outside the visible area */
  numToleratedItems?: number;
  
  /** Height of the scrollable area */
  scrollHeight?: string;
  
  /** Loading state */
  loading?: boolean;
  
  /** Loading message to display */
  loadingMessage?: string;
  
  /** Delay before loading is shown (ms) */
  loadingDelay?: number;
  
  /** Whether to enable smooth scrolling */
  smoothScroll?: boolean;
  
  /** Whether to snap to items when scrolling stops */
  snapToItems?: boolean;
  
  /** Class for the container */
  class?: string;
  
  /** ID for the container */
  id?: string;
  
  /** Callback when items are scrolled */
  onScroll?: (event: Event) => void;
  
  /** Callback when items become visible (for lazy loading) */
  onLazyLoad?: (event: { first: number; last: number }) => void;
}

/**
 * Data-driven Virtual Scroll component for efficiently rendering large lists.
 * Based on PrimeVue's Loading and Lazy Virtual Scroller design.
 */
const VirtualScroll: Component<VirtualScrollProps> = (rawProps) => {
  const props = mergeProps(
    {
      itemSize: 50,
      numToleratedItems: 5,
      scrollHeight: "400px",
      loading: false,
      loadingMessage: "Loading...",
      loadingDelay: 0,
      smoothScroll: true,
      snapToItems: false,
      onScroll: () => {},
      onLazyLoad: () => {}
    },
    rawProps
  );
  
  const [local, rest] = splitProps(props, [
    "items", "itemTemplate", "itemSize", "numToleratedItems", 
    "scrollHeight", "loading", "loadingMessage", "loadingDelay",
    "smoothScroll", "snapToItems", "class", "id", "onScroll", "onLazyLoad"
  ]);

  // State for virtualization
  const [scrollTop, setScrollTop] = createSignal(0);
  const [containerHeight, setContainerHeight] = createSignal(0);
  const [containerRef, setContainerRef] = createSignal<HTMLDivElement | null>(null);
  const [isProgrammaticScroll, setIsProgrammaticScroll] = createSignal(false);

  // Loading delay timer
  const [loadingTimer, setLoadingTimer] = createSignal<ReturnType<typeof setTimeout> | null>(null);
  const [showLoading, setShowLoading] = createSignal(false);

  // Track scroll end for snapping
  const [scrollTimer, setScrollTimer] = createSignal<ReturnType<typeof setTimeout> | null>(null);

  // Calculate visible range
  const calculateVisibleRange = () => {
    const top = scrollTop();
    const height = containerHeight();
    const itemHeight = local.itemSize;
    const tolerated = local.numToleratedItems;
    
    if (!height || itemHeight <= 0) return { first: 0, last: local.items.length - 1 };
    
    const first = Math.max(0, Math.floor(top / itemHeight) - tolerated);
    const visibleCount = Math.ceil(height / itemHeight) + (2 * tolerated);
    const last = Math.min(local.items.length - 1, first + visibleCount - 1);
    
    return { first, last };
  };

  const visibleRange = () => calculateVisibleRange();
  
  // Handle scroll events with debouncing for performance
  const handleScroll = (event: Event) => {
    const target = event.target as HTMLElement;
    const currentScrollTop = target.scrollTop;
    
    setScrollTop(currentScrollTop);
    
    // Don't process snapping if this is a programmatic scroll
    if (isProgrammaticScroll()) {
      return;
    }
    
    // Clear any existing scroll timer
    const existingTimer = scrollTimer();
    if (existingTimer) {
      clearTimeout(existingTimer);
    }
    
    // Set new timer to detect scroll end
    const newTimer = setTimeout(() => {
      // Handle snapping if enabled
      if (local.snapToItems) {
        handleSnapToItem(target);
      }
    }, 100);
    
    setScrollTimer(newTimer);
    local.onScroll?.(event);
    
    // Trigger lazy load callback
    const range = visibleRange();
    local.onLazyLoad?.({ first: range.first, last: range.last });
  };

  // Handle snapping to nearest item without any rocking
  const handleSnapToItem = (target: HTMLElement) => {
    const itemHeight = local.itemSize;
    const currentScrollTop = target.scrollTop;
    const height = containerHeight();
    const maxScroll = Math.max(0, local.items.length * itemHeight - height);
    
    // Calculate the nearest item index
    const nearestItemIndex = Math.round(currentScrollTop / itemHeight);
    const snapPosition = Math.max(0, Math.min(nearestItemIndex * itemHeight, maxScroll));
    
    // Only snap if we're not already at the correct position
    const distance = Math.abs(currentScrollTop - snapPosition);
    if (distance > 0.5) {
      setIsProgrammaticScroll(true);
      
      // Use instant scroll to prevent smooth scrolling from triggering more events
      target.scrollTop = snapPosition;
      setScrollTop(snapPosition);
      
      // Small delay to allow the DOM to update, then reset flag
      requestAnimationFrame(() => {
        setIsProgrammaticScroll(false);
      });
    }
  };

  // Handle loading delay
  createEffect(() => {
    if (local.loading) {
      const timer = setTimeout(() => {
        setShowLoading(true);
      }, local.loadingDelay);
      setLoadingTimer(timer);
    } else {
      setShowLoading(false);
      const timer = loadingTimer();
      if (timer) {
        clearTimeout(timer);
        setLoadingTimer(null);
      }
    }
  });

  // Cleanup timers
  onCleanup(() => {
    const timer = loadingTimer();
    if (timer) {
      clearTimeout(timer);
    }
    const scrollT = scrollTimer();
    if (scrollT) {
      clearTimeout(scrollT);
    }
  });

  // Update container height on mount and resize
  onMount(() => {
    if (containerRef()) {
      setContainerHeight(containerRef()!.clientHeight);
    }
  });

  createEffect(() => {
    if (containerRef()) {
      const resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          setContainerHeight(entry.contentRect.height);
        }
      });
      
      resizeObserver.observe(containerRef()!);
      
      onCleanup(() => {
        resizeObserver.disconnect();
      });
    }
  });

  // Calculate total content height for scrollbar
  const contentHeight = () => {
    return local.items.length * local.itemSize + "px";
  };

  // Calculate transform for visible items container
  const containerTransform = () => {
    const range = visibleRange();
    return `translateY(${range.first * local.itemSize}px)`;
  };

  // Calculate container height for visible items
  const visibleContainerHeight = () => {
    const range = visibleRange();
    const visibleCount = range.last - range.first + 1;
    return visibleCount * local.itemSize + "px";
  };

  // Scroll behavior style - only for user-initiated scrolling
  const scrollBehavior = () => {
    return local.smoothScroll && !isProgrammaticScroll() ? 'smooth' : 'auto';
  };

  return (
    <div
      {...rest}
      id={local.id}
      class={[styles.pvVirtualScroll, local.class ?? ""].filter(Boolean).join(" ")}
      style={{ 
        height: local.scrollHeight,
        "scroll-behavior": scrollBehavior(),
        ...(rest as any).style 
      }}
      ref={setContainerRef}
      onScroll={handleScroll}
    >
      <div
        class={styles.pvVirtualScrollContent}
        style={{ height: contentHeight() }}
      >
        <div
          class={styles.pvVirtualScrollItems}
          style={{
            height: visibleContainerHeight(),
            transform: containerTransform(),
            "will-change": "transform"
          }}
        >
          <Show when={!local.loading}>
            <Index each={local.items.slice(visibleRange().first, visibleRange().last + 1)}>
              {(item, index) => (
                <div
                  class={styles.pvVirtualScrollItem}
                  style={{
                    height: `${local.itemSize}px`,
                    position: "absolute",
                    top: `${index * local.itemSize}px`,
                    left: "0",
                    width: "100%"
                  }}
                >
                  {local.itemTemplate(item(), visibleRange().first + index)}
                </div>
              )}
            </Index>
          </Show>
        </div>
      </div>
      
      <Show when={showLoading()}>
        <div class={styles.pvVirtualScrollLoading}>
          <div class={styles.pvVirtualScrollLoadingOverlay} />
          <div class={styles.pvVirtualScrollLoadingContent}>
            {local.loadingMessage}
          </div>
        </div>
      </Show>
    </div>
  );
};

export default VirtualScroll;