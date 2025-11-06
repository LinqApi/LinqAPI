export class LinqApiClient {
    constructor({
        apiPrefix,
        controllerSuffix = "Linq",
        beforeSend,
        afterReceive,
        beforeError,
        getAuthToken,
        staticHeaders,
        debugView
    }) {
        this.apiPrefix = apiPrefix?.replace(/\/$/, "") || "";
        this.controllerSuffix = controllerSuffix;
        this.beforeSend = beforeSend || null;
        this.afterReceive = afterReceive || null;
        this.beforeError = beforeError || null;
        this.getAuthToken = getAuthToken || null;
        this.staticHeaders = staticHeaders || {};
        this.debugView = debugView || null;
    }

    // ---- URL helpers (yeni, temiz) ----
    _baseUrl(controllerName) {
        // örn: https://host/api/userprofileLinq
        return `${this.apiPrefix}/${controllerName}${this.controllerSuffix}`;
    }

    _itemUrl(controllerName, id) {
        return `${this._baseUrl(controllerName)}/${encodeURIComponent(id)}`;
    }

    _actionUrl(controllerName, segment) {
        // filterpaged, properties gibi ek action uçları
        return `${this._baseUrl(controllerName)}/${segment}`;
    }

    // ---- high-level API ----

    async getProperties(controllerName) {
        const url = this._actionUrl(controllerName, "properties");
        const ctx = { op: "getProperties", url, controller: controllerName };
        return this._get(url, ctx);
    }

    async getPage(controllerName, payload) {
        const url = this._actionUrl(controllerName, "filterpaged");
        const ctx = { op: "getPage", url, controller: controllerName };

        this._dbg("info", "➡️ POST page", { url, body: payload });

        const res = await fetch(url, {
            method: "POST",
            headers: {
                ...await this._buildHeaders(ctx),
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            this._dbg("error", "❌ POST page failed", {
                url,
                status: res.status,
                statusText: res.statusText
            });
            if (this.beforeError) {
                this.beforeError({ status: res.status }, ctx);
            }
            throw new Error("getPage failed with status " + res.status);
        }

        const data = await res.json();

        this._dbg("info", "⬅️ POST page ok", { url, data });

        return {
            items: data.items || [],
            totalRecords: data.totalRecords ?? 0
        };
    }

    // CREATE = POST /{controller}Linq
    async createRow(controllerName, dto) {
        const url = this._baseUrl(controllerName);
        const ctx = { op: "createRow", url, controller: controllerName };
        return this._sendJson(url, "POST", dto, ctx);
    }

    // UPDATE = PUT /{controller}Linq/{id}
    async updateRow(controllerName, id, dto) {
        const url = this._itemUrl(controllerName, id);
        const ctx = { op: "updateRow", url, controller: controllerName, id };
        return this._sendJson(url, "PUT", dto, ctx);
    }

    // DELETE = DELETE /{controller}Linq/{id}
    async deleteRow(controllerName, id) {
        const url = this._itemUrl(controllerName, id);
        const ctx = { op: "deleteRow", url, controller: controllerName, id };

        this._dbg("info", "➡️ DELETE", { url });

        const res = await fetch(url, {
            method: "DELETE",
            headers: await this._buildHeaders(ctx)
        });

        const text = await res.text();
        const json = this._safeParseJson(text);

        if (!res.ok) {
            const errObj = { status: res.status, body: json };
            this._dbg("error", "❌ DELETE failed", { url, errObj });
            if (this.beforeError) this.beforeError(errObj, ctx);
            throw errObj;
        }

        this._dbg("info", "⬅️ DELETE ok", { url });
        return true;
    }

    // ---- internals ----
    async _sendJson(url, method, bodyObj, ctx) {
        // interceptor: beforeSend
        let outgoing = bodyObj;
        if (this.beforeSend) {
            outgoing = this.beforeSend(outgoing, ctx);
        }

        this._dbg("info", `➡️ ${method}`, { url, body: outgoing });

        const res = await fetch(url, {
            method,
            headers: {
                "Content-Type": "application/json",
                ...await this._buildHeaders(ctx)
            },
            body: JSON.stringify(outgoing)
        });

        const text = await res.text();
        const json = this._safeParseJson(text);

        if (!res.ok) {
            const errObj = { status: res.status, body: json };
            this._dbg("error", `❌ ${method} failed`, { url, errObj });
            if (this.beforeError) this.beforeError(errObj, ctx);
            throw errObj;
        }

        // interceptor: afterReceive
        const finalData = this.afterReceive
            ? this.afterReceive(json, ctx)
            : json;

        this._dbg("info", `⬅️ ${method} ok`, { url, data: finalData });

        return finalData;
    }

    async _get(url, ctx) {
        const headers = await this._buildHeaders(ctx);

        this._dbg("info", "➡️ GET", { url });

        const res = await fetch(url, { method: "GET", headers });

        const text = await res.text();
        const json = this._safeParseJson(text);

        if (!res.ok) {
            const errObj = { status: res.status, body: json };
            this._dbg("error", "❌ GET failed", { url, errObj });
            if (this.beforeError) this.beforeError(errObj, ctx);
            throw errObj;
        }

        const finalData = this.afterReceive
            ? this.afterReceive(json, ctx)
            : json;

        this._dbg("info", "⬅️ GET ok", { url, data: finalData });

        return finalData;
    }

    async _buildHeaders(ctx) {
        const headers = { ...this.staticHeaders };
        const token = this.getAuthToken ? await this.getAuthToken() : null;
        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }
        return headers;
    }

    _safeParseJson(txt) {
        if (!txt) return {};
        try { return JSON.parse(txt); }
        catch { return { raw: txt }; }
    }

    _dbg(level, message, data) {
        if (this.debugView) {
            this.debugView.log(level, message, data);
        }
        if (level === "error") {
            console.error("[ApiClient]", message, data || "");
        } else if (level === "warn") {
            console.warn("[ApiClient]", message, data || "");
        } else {
            console.log("[ApiClient]", message, data || "");
        }
    }
}
