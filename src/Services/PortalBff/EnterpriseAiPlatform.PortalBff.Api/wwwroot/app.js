let authToken = "";

document.addEventListener("DOMContentLoaded", async () => {
    // Acquire dev-token for local testing
    try {
        const tokenRes = await fetch("/api/v1/portal/playground/dev-token");
        if (tokenRes.ok) {
            const tokenData = await tokenRes.json();
            authToken = tokenData.token;
            console.log("Acquired dev-token for local testing.");
        }
    } catch (e) {
        console.warn("Could not acquire dev-token:", e);
    }

    // Load models from API on startup
    await loadModelsFromRegistry();

    // Navigation Tabs
    const tabButtons = document.querySelectorAll(".tab-btn");
    const tabContents = document.querySelectorAll(".tab-content");

    tabButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            tabButtons.forEach(b => b.classList.remove("active"));
            tabContents.forEach(c => c.classList.remove("active"));

            btn.classList.add("active");
            const target = btn.getAttribute("data-tab");
            document.getElementById(`tab-${target}`).classList.add("active");

            if (target === "marketplace") loadMarketplace();
            if (target === "gpu") loadGpuCluster();
            if (target === "quotas") loadQuotas();
            if (target === "analytics") loadAnalytics();
        });
    });

    // Temperature Slider
    const tempSlider = document.getElementById("temp-slider");
    const tempValueLabel = document.getElementById("temp-value");
    tempSlider.addEventListener("input", (e) => {
        tempValueLabel.textContent = parseFloat(e.target.value).toFixed(1);
    });

    // Run Comparison
    const runBtn = document.getElementById("run-compare-btn");
    const promptInput = document.getElementById("prompt-input");
    const resultsGrid = document.getElementById("compare-results-grid");

    runBtn.addEventListener("click", async () => {
        const prompt = promptInput.value.trim();
        if (!prompt) {
            alert("Please enter a prompt to compare.");
            return;
        }

        const selectedModels = Array.from(document.querySelectorAll("#model-checkboxes input:checked"))
            .map(cb => cb.value);

        if (selectedModels.length === 0) {
            alert("Please select at least one model.");
            return;
        }

        runBtn.disabled = true;
        runBtn.textContent = "Executing Comparison...";
        resultsGrid.innerHTML = '<div class="empty-state">Running side-by-side execution...</div>';

        try {
            const response = await fetch("/api/v1/portal/playground/compare", {
                method: "POST",
                headers: { 
                    "Content-Type": "application/json",
                    "Authorization": authToken ? `Bearer ${authToken}` : ""
                },
                body: JSON.stringify({ prompt, targetModels: selectedModels, temperature: parseFloat(tempSlider.value) })
            });

            if (!response.ok) throw new Error("Failed to execute comparison.");

            const data = await response.json();
            renderComparisonResults(data);
        } catch (err) {
            resultsGrid.innerHTML = `<div class="empty-state text-danger">Error: ${err.message}</div>`;
        } finally {
            runBtn.disabled = false;
            runBtn.textContent = "Run Comparison";
        }
    });

    function renderComparisonResults(results) {
        resultsGrid.innerHTML = "";
        results.forEach(res => {
            const card = document.createElement("div");
            card.className = "result-card";
            const providerBadgeClass = getProviderBadgeClass(res.providerName);
            card.innerHTML = `
                <div class="result-header">
                    <strong>${res.modelId}</strong>
                    <span class="badge ${providerBadgeClass}">${res.providerName || 'Unknown'}</span>
                </div>
                <div class="result-text">${marked.parse(res.responseText)}</div>
                <div class="result-metrics">
                    <span>Latency: <strong>${res.latencyMs} ms</strong></span>
                    <span>Tokens: <strong>${(res.promptTokens || 0) + (res.completionTokens || 0)}</strong></span>
                    <span>Cost: <strong>$${res.estimatedCostDollars || '0.00'}</strong></span>
                    <span>Quality: <strong>${res.qualityScore || 'N/A'}</strong></span>
                </div>
            `;
            resultsGrid.appendChild(card);
        });
    }

    // Dynamic Model Loading from ModelRegistry API
    async function loadModelsFromRegistry() {
        const container = document.getElementById("model-checkboxes");
        try {
            const res = await fetch("/api/v1/portal/models", {
                headers: { "Authorization": authToken ? `Bearer ${authToken}` : "" }
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();

            if (!data || !data.items || data.items.length === 0) {
                container.innerHTML = '<div class="empty-state">No models registered. Add models via the Model Registry API.</div>';
                return;
            }

            container.innerHTML = "";
            data.items.forEach(model => {
                const label = document.createElement("label");
                label.className = "checkbox-label";
                const badgeClass = getProviderBadgeClass(model.provider);
                const modelKey = `${model.provider}-${model.providerModelName}`.toLowerCase().replace(/[\s_.]+/g, '-');
                label.innerHTML = `
                    <input type="checkbox" id="model-${modelKey}" value="${modelKey}" checked>
                    <span class="badge ${badgeClass}">${model.displayName}</span>
                `;
                container.appendChild(label);
            });
        } catch (e) {
            console.warn("Could not load models from registry, using fallback:", e);
            container.innerHTML = `
                <label class="checkbox-label">
                    <input type="checkbox" value="azure-gpt-4o" checked>
                    <span class="badge badge-azure">Azure OpenAI GPT-4o</span>
                </label>
                <label class="checkbox-label">
                    <input type="checkbox" value="anthropic-claude-3-5" checked>
                    <span class="badge badge-anthropic">Claude 3.5 Sonnet</span>
                </label>
                <label class="checkbox-label">
                    <input type="checkbox" value="gemini-1-5-pro" checked>
                    <span class="badge badge-gemini">Gemini 1.5 Pro</span>
                </label>
                <label class="checkbox-label">
                    <input type="checkbox" value="vllm-deepseek-coder" checked>
                    <span class="badge badge-vllm">Self-Hosted DeepSeek (GPU)</span>
                </label>`;
        }
    }

    // Marketplace Catalog (real-time from API)
    async function loadMarketplace() {
        const grid = document.getElementById("marketplace-grid");
        try {
            const res = await fetch("/api/v1/portal/marketplace/catalog", {
                headers: { "Authorization": authToken ? `Bearer ${authToken}` : "" }
            });
            const data = await res.json();
            grid.innerHTML = "";
            if (!data || data.length === 0) {
                grid.innerHTML = '<div class="empty-state">No marketplace entries available.</div>';
                return;
            }
            data.forEach(item => {
                const card = document.createElement("div");
                card.className = "card";
                card.innerHTML = `
                    <h3>${item.name} <span class="badge badge-gemini">${item.category}</span></h3>
                    <p class="section-desc">${item.description}</p>
                    <div class="result-metrics">
                        <span>Quality Score: <strong>${item.benchmarkQualityScore || 'N/A'}</strong></span>
                        <span>Avg Latency: <strong>${item.averageLatencyMs || 'N/A'} ms</strong></span>
                        <span>Publisher: <strong>${item.publisher || 'Platform'}</strong></span>
                    </div>
                `;
                grid.appendChild(card);
            });
        } catch (e) {
            grid.innerHTML = '<div class="empty-state">Failed to load marketplace catalog.</div>';
        }
    }

    // GPU Cluster Status (real-time from API)
    async function loadGpuCluster() {
        const grid = document.getElementById("gpu-grid");
        try {
            const res = await fetch("/api/v1/portal/cluster/gpus", {
                headers: { "Authorization": authToken ? `Bearer ${authToken}` : "" }
            });
            const data = await res.json();
            grid.innerHTML = "";
            if (!data || data.length === 0) {
                grid.innerHTML = '<div class="empty-state">No GPU nodes configured. Configure LocalModel:GpuNodes in the environment.</div>';
                return;
            }
            data.forEach(node => {
                const utilizationColor = node.gpuUtilizationPercentage > 80 ? 'text-danger' : 'text-success';
                const card = document.createElement("div");
                card.className = "card";
                card.innerHTML = `
                    <h3>${node.nodeId} (${node.gpuModel})</h3>
                    <p class="section-desc">Loaded: ${node.loadedModels?.join(", ") || 'None'}</p>
                    <div class="result-metrics">
                        <span>VRAM: <strong>${node.usedVramMb}/${node.totalVramMb} MB</strong></span>
                        <span>Load: <strong class="${utilizationColor}">${node.gpuUtilizationPercentage}%</strong></span>
                        <span>Status: <strong class="text-success">${node.status || 'Online'}</strong></span>
                    </div>
                `;
                grid.appendChild(card);
            });
        } catch (e) {
            grid.innerHTML = '<div class="empty-state">Failed to load GPU metrics.</div>';
        }
    }

    // Token Budgets & Quotas (real-time from CostOptimization API)
    async function loadQuotas() {
        const container = document.getElementById("quota-container");
        try {
            const res = await fetch("/api/v1/portal/playground/quotas/summary", {
                headers: { "Authorization": authToken ? `Bearer ${authToken}` : "" }
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            const tokenPct = data.monthlyTokenLimit > 0 ? ((data.consumedTokens / data.monthlyTokenLimit) * 100).toFixed(1) : 0;
            const budgetPct = data.monthlyBudgetDollars > 0 ? ((data.consumedDollars / data.monthlyBudgetDollars) * 100).toFixed(1) : 0;
            const tokenColor = tokenPct > 75 ? 'text-danger' : 'text-success';
            const budgetColor = budgetPct > 75 ? 'text-danger' : 'text-success';

            container.innerHTML = `
                <h2>Tenant Budget Summary</h2>
                <div class="metrics-summary-grid">
                    <div class="metric-card">
                        <span class="metric-label">Monthly Token Usage</span>
                        <span class="metric-value ${tokenColor}">${(data.consumedTokens / 1000000).toFixed(1)}M / ${(data.monthlyTokenLimit / 1000000).toFixed(0)}M</span>
                    </div>
                    <div class="metric-card">
                        <span class="metric-label">Monthly Budget</span>
                        <span class="metric-value ${budgetColor}">$${Number(data.consumedDollars).toLocaleString()} / $${Number(data.monthlyBudgetDollars).toLocaleString()}</span>
                    </div>
                    <div class="metric-card">
                        <span class="metric-label">Token Utilization</span>
                        <span class="metric-value ${tokenColor}">${tokenPct}%</span>
                    </div>
                    <div class="metric-card">
                        <span class="metric-label">Budget Utilization</span>
                        <span class="metric-value ${budgetColor}">${budgetPct}%</span>
                    </div>
                </div>
            `;
        } catch (e) {
            container.innerHTML = '<div class="empty-state">Failed to load quota data. Ensure the CostOptimization backend is configured.</div>';
        }
    }

    // Real-time Analytics from Metering API
    async function loadAnalytics() {
        try {
            const res = await fetch("/api/v1/portal/analytics/overview", {
                headers: { "Authorization": authToken ? `Bearer ${authToken}` : "" }
            });
            if (res.ok) {
                const data = await res.json();
                // Update DOM with real metering data
                const totalRecords = data.items ? data.items.length : 0;
                let totalTokens = 0;
                let totalCost = 0;
                let latencies = [];

                if (data.items) {
                    data.items.forEach(record => {
                        if (record.dimension === "PromptTokens" || record.dimension === "CompletionTokens") {
                            totalTokens += record.value;
                        }
                        if (record.dimension === "TotalCost") {
                            totalCost += record.value;
                        }
                        if (record.dimension === "LatencyMs") {
                            latencies.push(record.value);
                        }
                    });
                }

                const p95Latency = latencies.length > 0
                    ? latencies.sort((a, b) => a - b)[Math.floor(latencies.length * 0.95)] || 0
                    : 0;

                document.getElementById("metric-total-requests").textContent = totalRecords.toLocaleString();
                document.getElementById("metric-total-tokens").textContent = totalTokens.toLocaleString();
                document.getElementById("metric-cost-saved").textContent = `$${totalCost.toFixed(2)}`;
                document.getElementById("metric-p95-latency").textContent = `${Math.round(p95Latency)} ms`;
            } else {
                setAnalyticsFallback("Service unavailable");
            }
        } catch (e) {
            setAnalyticsFallback("Failed to connect");
        }
    }

    function setAnalyticsFallback(msg) {
        document.getElementById("metric-total-requests").textContent = msg;
        document.getElementById("metric-total-tokens").textContent = msg;
        document.getElementById("metric-cost-saved").textContent = msg;
        document.getElementById("metric-p95-latency").textContent = msg;
    }

    function getProviderBadgeClass(provider) {
        if (!provider) return "badge-azure";
        const p = provider.toLowerCase();
        if (p.includes("azure") || p.includes("openai")) return "badge-azure";
        if (p.includes("anthropic") || p.includes("claude")) return "badge-anthropic";
        if (p.includes("gemini") || p.includes("google")) return "badge-gemini";
        if (p.includes("vllm") || p.includes("deepseek") || p.includes("self")) return "badge-vllm";
        return "badge-azure";
    }
});
