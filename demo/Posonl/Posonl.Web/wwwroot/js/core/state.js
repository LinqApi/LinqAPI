export class StateManager {
    constructor(initialState = {}) {
        this.state = initialState;
        this.listeners = new Map();
    }
    getState() {
        return this.state;
    }
    setState(partialState) {
        this.state = { ...this.state, ...partialState };
        this.notify();
    }
    subscribe(key, listener) {
        if (typeof listener !== "function") {
            throw new Error("Listener must be a function");
        }
        if (!this.listeners.has(key)) {
            this.listeners.set(key, new Set());
        }
        this.listeners.get(key).add(listener);
        return () => {
            this.listeners.get(key).delete(listener);
        };
    }
    notify() {
        // For each key in state, notify corresponding listeners
        for (const [key, listenersSet] of this.listeners.entries()) {
            const value = this.state[key];
            listenersSet.forEach(listener => listener(value));
        }
    }
}
