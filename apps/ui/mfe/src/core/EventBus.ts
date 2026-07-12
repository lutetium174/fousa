export interface EventBus {
  publish<T>(event: string, payload: T): void;
  subscribe<T>(event: string, handler: (payload: T) => void): () => void;
  once<T>(event: string, handler: (payload: T) => void): () => void;
}

class EventBusImpl implements EventBus {
  private handlers: Map<string, Set<(payload: unknown) => void>> = new Map();

  publish<T>(event: string, payload: T): void {
    const eventHandlers = this.handlers.get(event);
    if (eventHandlers) {
      eventHandlers.forEach((handler) => {
        try {
          handler(payload);
        } catch (error) {
          console.error(`Error in event handler for "${event}":`, error);
        }
      });
    }
  }

  subscribe<T>(event: string, handler: (payload: T) => void): () => void {
    if (!this.handlers.has(event)) {
      this.handlers.set(event, new Set());
    }
    const eventHandlers = this.handlers.get(event)!;
    eventHandlers.add(handler as (payload: unknown) => void);

    return () => {
      eventHandlers.delete(handler as (payload: unknown) => void);
      if (eventHandlers.size === 0) {
        this.handlers.delete(event);
      }
    };
  }

  once<T>(event: string, handler: (payload: T) => void): () => void {
    const wrappedHandler = (payload: T) => {
      unsubscribe();
      handler(payload);
    };
    const unsubscribe = this.subscribe(event, wrappedHandler);
    return unsubscribe;
  }
}

export const eventBus: EventBus = new EventBusImpl();
