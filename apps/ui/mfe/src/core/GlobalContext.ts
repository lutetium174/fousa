export type Language = "en" | "fr";

export type GlobalState = {
  theme: "light" | "dark";
  language: Language;
  config: Record<string, unknown>;
  user: { id: string; name: string } | null;
};

export type GlobalContext = {
  getState: () => GlobalState;
  subscribe: (listener: (state: GlobalState) => void) => () => void;
  update: (fn: (prev: GlobalState) => GlobalState) => void;
};

class GlobalContextImpl implements GlobalContext {
  private state: GlobalState = {
    theme: "light",
    language: "en",
    config: {},
    user: null,
  };

  private listeners: Set<(state: GlobalState) => void> = new Set();

  getState(): GlobalState {
    return this.state;
  }

  subscribe(listener: (state: GlobalState) => void): () => void {
    this.listeners.add(listener);
    return () => {
      this.listeners.delete(listener);
    };
  }

  update(fn: (prev: GlobalState) => GlobalState): void {
    const newState = fn(this.state);
    if (newState !== this.state) {
      this.state = newState;
      this.notifyListeners();
    }
  }

  private notifyListeners(): void {
    this.listeners.forEach((listener) => {
      try {
        listener(this.state);
      } catch (error) {
        console.error("Error in global context listener:", error);
      }
    });
  }
}

export const globalContext: GlobalContext = new GlobalContextImpl();
