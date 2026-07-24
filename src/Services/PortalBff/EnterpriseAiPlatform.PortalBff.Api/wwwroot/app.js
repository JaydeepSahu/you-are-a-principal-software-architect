document.addEventListener("DOMContentLoaded", () => {
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
        });
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
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ prompt, targetModels: selectedModels, temperature: 0.7 })
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
            card.innerHTML = `
                <div class="result-header">
                    <strong>${res.modelId}</strong>
                    <span class="badge badge-azure">${res.providerName}</span>
                </div>
                <p class="result-text">${res.responseText}</p>
                <div class="result-metrics">
                    <span>Latency: <strong>${res.latencyMs} ms</strong></span>
                    <span>Tokens: <strong>${res.promptTokens + res.completionTokens}</strong></span>
                    <span>Cost: <strong>$${res.estimatedCostDollars}</strong></span>
                    <span>Quality: <strong>${res.qualityScore}</strong></span>
                </div>
            `;
            resultsGrid.appendChild(card);
        });
    }

    async function loadMarketplace() {
        const grid = document.getElementById("marketplace-grid");
        try {
            const res = await fetch("/api/v1/portal/marketplace/catalog");
            const data = await res.json();
            grid.innerHTML = "";
            data.forEach(item => {
                const card = document.createElement("div");
                card.className = "card";
                card.innerHTML = `
                    <h3>${item.name} <span class="badge badge-gemini">${item.category}</span></h3>
                    <p class="section-desc">${item.description}</p>
                    <div class="result-metrics">
                        <span>Score: <strong>${item.benchmarkQualityScore}</strong></span>
                        <span>Latency: <strong>${item.averageLatencyMs} ms</strong></span>
                    </div>
                `;
                grid.appendChild(card);
            });
        } catch (e) {
            grid.innerHTML = '<div class="empty-state">Failed to load marketplace catalog.</div>';
        }
    }

    async function loadGpuCluster() {
        const grid = document.getElementById("gpu-grid");
        try {
            const res = await fetch("/api/v1/portal/cluster/gpus");
            const data = await res.json();
            grid.innerHTML = "";
            data.forEach(node => {
                const card = document.createElement("div");
                card.className = "card";
                card.innerHTML = `
                    <h3>${node.nodeId} (${node.gpuModel})</h3>
                    <p class="section-desc">Loaded: ${node.loadedModels.join(", ")}</p>
                    <div class="result-metrics">
                        <span>VRAM: <strong>${node.usedVramMb}/${node.totalVramMb} MB</strong></span>
                        <span>Load: <strong>${node.gpuUtilizationPercentage}%</strong></span>
                    </div>
                `;
                grid.appendChild(card);
            });
        } catch (e) {
            grid.innerHTML = '<div class="empty-state">Failed to load GPU metrics.</div>';
        }
    }

    async function loadQuotas() {
        const container = document.getElementById("quota-container");
        try {
            const res = await fetch("/api/v1/portal/playground/quotas/summary");
            const data = await res.json();
            container.innerHTML = `
                <h2>Tenant Budget Summary: ${data.tenantId}</h2>
                <div class="metrics-summary-grid">
                    <div class="metric-card">
                        <span class="metric-label">Monthly Tokens</span>
                        <span class="metric-value">${(data.consumedTokens / 1000000).toFixed(1)}M / ${(data.monthlyTokenLimit / 1000000).toFixed(0)}M</span>
                    </div>
                    <div class="metric-card">
                        <span class="metric-label">Budget Dollars</span>
                        <span class="metric-value">$${data.consumedDollars} / $${data.monthlyBudgetDollars}</span>
                    </div>
                </div>
            `;
        } catch (e) {
            container.innerHTML = '<div class="empty-state">Failed to load quotas.</div>';
        }
    }
});
