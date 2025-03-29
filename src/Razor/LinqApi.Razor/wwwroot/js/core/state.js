// core/state.js
// StateManager.js
export class StateManager {
    constructor(initialState = {}) {
        this.state = initialState;
        this.listeners = new Set();
    }
    getState() {
        return this.state;
    }
    setState(partialState) {
        this.state = { ...this.state, ...partialState };
        this.notify();
    }
    subscribe(listener) {
        this.listeners.add(listener);
        return () => this.listeners.delete(listener);
    }
    notify() {
        this.listeners.forEach(listener => listener(this.state));
    }
}
