export interface EventBus {
    publish<T>(event: string, payload: T): void;
    subscribe<T>(event: string, handler: (payload: T) => void): () => void;
    once<T>(event: string, handler: (payload: T) => void): () => void;
}
