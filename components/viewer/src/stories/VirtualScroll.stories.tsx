import { VirtualScroll, createSignal, createEffect, onCleanup } from "components";
import type { VirtualScrollProps } from "components";

// Generate sample data
const generateItems = (count: number, start = 0) => {
  return Array.from({ length: count }, (_, i) => ({
    id: start + i,
    name: `Item ${start + i + 1}`,
    description: `This is item number ${start + i + 1} in the list`,
    value: Math.random() * 100
  }));
};

// Sample item template
const DefaultItemTemplate = (item: any, index: number) => (
  <div style={{
    display: "flex",
    alignItems: "center",
    padding: "12px 16px",
    width: "100%",
    boxSizing: "border-box"
  }}>
    <span style={{
      fontSize: "14px",
      fontWeight: "500",
      color: "var(--text-color)",
      minWidth: "40px"
    }}>
      {index + 1}
    </span>
    <div style={{
      marginLeft: "12px",
      flex: 1
    }}>
      <div style={{
        fontSize: "14px",
        fontWeight: "500",
        color: "var(--text-color)"
      }}>
        {item.name}
      </div>
      <div style={{
        fontSize: "12px",
        color: "var(--text-muted)"
      }}>
        {item.description}
      </div>
    </div>
    <div style={{
      fontSize: "12px",
      color: "var(--primary)",
      fontWeight: "600"
    }}>
      {item.value.toFixed(1)}%
    </div>
  </div>
);

// Colorful item template
const ColorfulItemTemplate = (item: any, index: number) => (
  <div style={{
    display: "flex",
    alignItems: "center",
    padding: "12px 16px",
    width: "100%",
    boxSizing: "border-box",
    background: index % 2 === 0 ? "var(--bg-subtle)" : "transparent"
  }}>
    <div style={{
      width: "32px",
      height: "32px",
      borderRadius: "var(--radius-full)",
      background: `hsl(${index * 10 % 360}, 70%, 50%)`,
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      color: "white",
      fontWeight: "600",
      fontSize: "12px",
      marginRight: "12px",
      flexShrink: 0
    }}>
      {index + 1}
    </div>
    <div style={{
      flex: 1,
      fontSize: "14px",
      color: "var(--text-color)"
    }}>
      {item.name} - {item.description}
    </div>
  </div>
);

// Minimal item template
const MinimalItemTemplate = (item: any, index: number) => (
  <div style={{
    padding: "8px 16px",
    textAlign: "center",
    fontSize: "14px",
    color: "var(--text-color)",
    background: index % 3 === 0 ? "var(--bg-emphasis)" : "transparent"
  }}>
    Row {index + 1}
  </div>
);

// Card-style item template
const CardItemTemplate = (item: any, index: number) => (
  <div style={{
    padding: "16px",
    borderRadius: "var(--radius-md)",
    margin: "8px",
    background: "var(--bg)",
    boxShadow: "var(--shadow-sm)",
    border: "1px solid var(--border)"
  }}>
    <h3 style={{
      margin: "0 0 8px 0",
      fontSize: "16px",
      fontWeight: "600",
      color: "var(--text-color)"
    }}>
      {item.name}
    </h3>
    <p style={{
      margin: "0",
      fontSize: "12px",
      color: "var(--text-muted)"
    }}>
      {item.description}
    </p>
    <div style={{
      marginTop: "12px",
      display: "flex",
      justifyContent: "space-between",
      fontSize: "12px"
    }}>
      <span style={{ color: "var(--primary)" }}>ID: {item.id}</span>
      <span style={{ color: "var(--text-muted)" }}>{item.value.toFixed(1)}%</span>
    </div>
  </div>
);

// Infinite loading story with server simulation
const InfiniteLoadingStory = () => {
  const [items, setItems] = createSignal<any[]>([]);
  const [loading, setLoading] = createSignal(false);
  const [hasMore, setHasMore] = createSignal(true);
  const [page, setPage] = createSignal(1);
  
  // Simulate server fetch
  const fetchItems = async (pageNum: number) => {
    return new Promise<any[]>((resolve) => {
      setTimeout(() => {
        const newItems = generateItems(20, (pageNum - 1) * 20);
        resolve(newItems);
      }, 500);
    });
  };
  
  // Load initial data
  createEffect(() => {
    loadMore();
  }, []);
  
  const loadMore = async () => {
    if (!hasMore() || loading()) return;
    
    setLoading(true);
    try {
      const newItems = await fetchItems(page());
      setItems(prev => [...prev, ...newItems]);
      setPage(prev => prev + 1);
      setHasMore(newItems.length > 0);
    } catch (error) {
      console.error("Failed to load more items:", error);
    } finally {
      setLoading(false);
    }
  };
  
  const handleLazyLoad = ({ first, last }: { first: number; last: number }) => {
    // Load more data when user is near the end
    const threshold = 5; // Load more when 5 items from the end
    if (last >= items().length - threshold && hasMore() && !loading()) {
      loadMore();
    }
  };
  
  return (
    <div style={{
      display: "flex",
      flexDirection: "column",
      height: "100%"
    }}>
      <div style={{
        fontSize: "12px",
        color: "var(--text-muted)",
        marginBottom: "8px"
      }}>
        Infinite Loading Demo - Scroll down to load more
      </div>
      <VirtualScroll
        items={items()}
        itemTemplate={DefaultItemTemplate}
        itemSize={64}
        scrollHeight="400px"
        loading={loading()}
        loadingMessage="Loading more items..."
        onLazyLoad={handleLazyLoad}
        numToleratedItems={8}
        smoothScroll={true}
      />
      <div style={{
        marginTop: "8px",
        fontSize: "12px",
        color: "var(--text-muted)",
        textAlign: "center"
      }}>
        {items().length} items loaded | Has more: {hasMore() ? 'Yes' : 'No'}
      </div>
    </div>
  );
};

// Advanced infinite loading with search/filter
const AdvancedInfiniteLoadingStory = () => {
  const [items, setItems] = createSignal<any[]>([]);
  const [loading, setLoading] = createSignal(false);
  const [hasMore, setHasMore] = createSignal(true);
  const [page, setPage] = createSignal(1);
  const [searchTerm, setSearchTerm] = createSignal("");
  
  // Simulate server search
  const searchItems = async (pageNum: number, search: string) => {
    return new Promise<any[]>((resolve) => {
      setTimeout(() => {
        const filtered = generateItems(15, (pageNum - 1) * 15)
          .filter(item => search === "" || item.name.toLowerCase().includes(search.toLowerCase()));
        resolve(filtered);
      }, 800);
    });
  };
  
  // Reset and search
  const handleSearch = () => {
    setItems([]);
    setPage(1);
    setHasMore(true);
    loadMore();
  };
  
  // Load initial data
  createEffect(() => {
    loadMore();
  }, []);
  
  const loadMore = async () => {
    if (!hasMore() || loading()) return;
    
    setLoading(true);
    try {
      const newItems = await searchItems(page(), searchTerm());
      setItems(prev => [...prev, ...newItems]);
      setPage(prev => prev + 1);
      setHasMore(newItems.length > 0);
    } catch (error) {
      console.error("Failed to load more items:", error);
    } finally {
      setLoading(false);
    }
  };
  
  const handleLazyLoad = ({ first, last }: { first: number; last: number }) => {
    const threshold = 5;
    if (last >= items().length - threshold && hasMore() && !loading()) {
      loadMore();
    }
  };
  
  return (
    <div style={{
      display: "flex",
      flexDirection: "column",
      height: "100%"
    }}>
      <div style={{
        marginBottom: "12px",
        display: "flex",
        gap: "8px",
        alignItems: "center"
      }}>
        <input
          type="text"
          placeholder="Search..."
          value={searchTerm()}
          onInput={(e) => setSearchTerm(e.target.value)}
          style={{
            padding: "6px 12px",
            border: "1px solid var(--border)",
            borderRadius: "var(--radius-sm)",
            fontSize: "14px",
            flex: 1
          }}
        />
        <button
          onClick={handleSearch}
          style={{
            padding: "6px 16px",
            background: "var(--primary)",
            color: "white",
            border: "none",
            borderRadius: "var(--radius-sm)",
            cursor: "pointer",
            fontSize: "14px"
          }}
        >
          Search
        </button>
      </div>
      <VirtualScroll
        items={items()}
        itemTemplate={(item, index) => (
          <div style={{
            display: "flex",
            alignItems: "center",
            padding: "12px 16px",
            background: index % 2 === 0 ? "var(--bg-subtle)" : "transparent"
          }}>
            <span style={{ fontWeight: "500", marginRight: "12px" }}>{index + 1}</span>
            <div>
              <div style={{ fontWeight: "500" }}>{item.name}</div>
              <div style={{ fontSize: "12px", color: "var(--text-muted)" }}>
                {item.description}
              </div>
            </div>
          </div>
        )}
        itemSize={60}
        scrollHeight="350px"
        loading={loading()}
        loadingMessage={searchTerm() ? "Searching..." : "Loading more..."}
        onLazyLoad={handleLazyLoad}
        numToleratedItems={6}
        smoothScroll={true}
      />
      <div style={{
        marginTop: "8px",
        fontSize: "12px",
        color: "var(--text-muted)",
        textAlign: "center"
      }}>
        Showing {items().length} of {Math.min((page() - 1) * 15 + items().length, 100)} results
      </div>
    </div>
  );
};

export default {
  title: "Components/VirtualScroll",
  component: VirtualScroll,
  argTypes: {
    itemSize: {
      control: "number",
      description: "Height of each item in pixels"
    },
    numToleratedItems: {
      control: "number",
      description: "Number of items to render outside visible area"
    },
    scrollHeight: {
      control: "text",
      description: "Height of the scrollable area"
    },
    loading: {
      control: "boolean",
      description: "Whether to show loading state"
    },
    loadingMessage: {
      control: "text",
      description: "Message to display when loading"
    },
    loadingDelay: {
      control: "number",
      description: "Delay before showing loading (ms)"
    },
    smoothScroll: {
      control: "boolean",
      description: "Enable smooth scrolling"
    },
    snapToItems: {
      control: "boolean",
      description: "Snap to items when scrolling stops"
    }
  },
  parameters: {
    docs: {
      description: {
        component: "Data-driven Virtual Scroll component for efficiently rendering large lists. Based on PrimeVue's Loading and Lazy Virtual Scroller design.\n\n## Server-Side Loading\n\nUse the `onLazyLoad` callback to load data from a server as the user scrolls. The callback receives `{ first: number, last: number }` indicating the currently visible item range.\n\n### Basic Infinite Loading Pattern:\n\n```tsx\nconst MyVirtualList = () => {\n  const [items, setItems] = createSignal([]);\n  const [loading, setLoading] = createSignal(false);\n  const [page, setPage] = createSignal(1);\n\n  const loadMore = async () => {\n    if (loading()) return;\n    setLoading(true);\n    const newItems = await fetchData(page());\n    setItems(prev => [...prev, ...newItems]);\n    setPage(prev => prev + 1);\n    setLoading(false);\n  }\n\n  const handleLazyLoad = ({ first, last }) => {\n    const threshold = 10; // Load more when 10 items from end\n    if (last >= items().length - threshold) {\n      loadMore();\n    }\n  }\n\n  return (\n    <VirtualScroll\n      items={items()}\n      itemTemplate={renderItem}\n      onLazyLoad={handleLazyLoad}\n      loading={loading()}\n      loadingMessage="Loading..."\n    />\n  )\n}\n```\n\n### Advanced Features:\n\n- Use `smoothScroll={true}` for smooth scrolling behavior\n- Use `snapToItems={true}` to snap to item boundaries when scrolling stops\n- Adjust `numToleratedItems` to control how many extra items are rendered outside the viewport\n- The component efficiently handles large datasets by only rendering visible items"
      }
    }
  }
};

type Story = VirtualScrollProps & { children?: any };

// Basic usage with default template
export const Default: Story = {
  items: generateItems(1000),
  itemTemplate: DefaultItemTemplate,
  itemSize: 64,
  scrollHeight: "400px",
  numToleratedItems: 5
};

// Smooth scrolling enabled (default)
export const SmoothScrolling: Story = {
  items: generateItems(500),
  itemTemplate: ColorfulItemTemplate,
  itemSize: 56,
  scrollHeight: "400px",
  smoothScroll: true,
  numToleratedItems: 8
};

// Snap to items enabled
export const SnapToItems: Story = {
  items: generateItems(200),
  itemTemplate: MinimalItemTemplate,
  itemSize: 50,
  scrollHeight: "400px",
  snapToItems: true,
  numToleratedItems: 5
};

// Smooth scrolling with snapping
export const SmoothWithSnap: Story = {
  items: generateItems(300),
  itemTemplate: DefaultItemTemplate,
  itemSize: 60,
  scrollHeight: "400px",
  smoothScroll: true,
  snapToItems: true,
  numToleratedItems: 6
};

// No smooth scrolling, no snapping
export const BasicScrolling: Story = {
  items: generateItems(200),
  itemTemplate: DefaultItemTemplate,
  itemSize: 64,
  scrollHeight: "400px",
  smoothScroll: false,
  snapToItems: false,
  numToleratedItems: 5
};

// Minimal setup
export const Minimal: Story = {
  items: generateItems(500),
  itemTemplate: MinimalItemTemplate,
  itemSize: 40,
  scrollHeight: "300px"
};

// Colorful items
export const Colorful: Story = {
  items: generateItems(200),
  itemTemplate: ColorfulItemTemplate,
  itemSize: 56,
  scrollHeight: "400px",
  numToleratedItems: 8
};

// Card-style items
export const Cards: Story = {
  items: generateItems(200),
  itemTemplate: CardItemTemplate,
  itemSize: 100,
  scrollHeight: "500px",
  numToleratedItems: 3
};

// Loading state
export const Loading: Story = {
  items: generateItems(100),
  itemTemplate: DefaultItemTemplate,
  itemSize: 64,
  scrollHeight: "400px",
  loading: true,
  loadingMessage: "Loading data..."
};

// Loading with delay
export const LoadingWithDelay: Story = {
  items: generateItems(100),
  itemTemplate: DefaultItemTemplate,
  itemSize: 64,
  scrollHeight: "400px",
  loading: true,
  loadingMessage: "Please wait...",
  loadingDelay: 1000
};

// Large dataset (10,000 items)
export const LargeDataset: Story = {
  items: generateItems(10000),
  itemTemplate: DefaultItemTemplate,
  itemSize: 50,
  scrollHeight: "300px",
  numToleratedItems: 10,
  smoothScroll: true
};

// Small item size
export const SmallItems: Story = {
  items: generateItems(500),
  itemTemplate: MinimalItemTemplate,
  itemSize: 32,
  scrollHeight: "400px",
  numToleratedItems: 15
};

// Large item size
export const LargeItems: Story = {
  items: generateItems(100),
  itemTemplate: CardItemTemplate,
  itemSize: 120,
  scrollHeight: "500px",
  numToleratedItems: 2,
  smoothScroll: true,
  snapToItems: true
};

// Custom height
export const TallContainer: Story = {
  items: generateItems(300),
  itemTemplate: DefaultItemTemplate,
  itemSize: 48,
  scrollHeight: "600px",
  numToleratedItems: 8,
  smoothScroll: true
};

// Infinite loading from "server"
export const InfiniteLoading = InfiniteLoadingStory;

// Advanced infinite loading with search
export const AdvancedInfiniteLoading = AdvancedInfiniteLoadingStory;