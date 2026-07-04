const provider = document.querySelector("#provider");
const indexed = document.querySelector("#indexed");
const cached = document.querySelector("#cached");
const summary = document.querySelector("#summary");
const hits = document.querySelector("#hits");

function parsePairs(value) {
  return Object.fromEntries(
    value
      .split(";")
      .map(part => part.trim())
      .filter(Boolean)
      .map(part => {
        const [key, ...rest] = part.split("=");
        return [key.trim(), rest.join("=").trim()];
      })
      .filter(([key, val]) => key && val)
  );
}

async function refreshProgress() {
  const response = await fetch("/api/v1/vector-search/progress");
  if (!response.ok) return;
  const data = await response.json();
  provider.textContent = data.provider;
  indexed.textContent = data.indexedDocuments;
  cached.textContent = data.cachedQueries;
}

document.querySelector("#indexForm").addEventListener("submit", async event => {
  event.preventDefault();
  const payload = {
    documents: [{
      externalId: document.querySelector("#externalId").value,
      content: document.querySelector("#content").value,
      metadata: parsePairs(document.querySelector("#metadata").value)
    }]
  };
  const response = await fetch("/api/v1/vector-search/documents", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  const data = await response.json();
  summary.textContent = response.ok
    ? `Indexed ${data.upsertedCount} document at ${new Date(data.indexedAtUtc).toLocaleTimeString()}.`
    : data.detail || "Indexing failed.";
  await refreshProgress();
});

document.querySelector("#searchForm").addEventListener("submit", async event => {
  event.preventDefault();
  const payload = {
    query: document.querySelector("#query").value,
    mode: document.querySelector("#mode").value,
    topK: Number(document.querySelector("#topK").value),
    metadataFilters: parsePairs(document.querySelector("#filters").value),
    rerank: document.querySelector("#rerank").checked,
    useCache: document.querySelector("#useCache").checked
  };
  const response = await fetch("/api/v1/vector-search/search", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  const data = await response.json();
  if (!response.ok) {
    summary.textContent = data.detail || "Search failed.";
    return;
  }

  summary.textContent = `${data.hits.length} hits in ${Math.round(data.elapsedMilliseconds || 0)} ms. Cache: ${data.cacheHit ? "hit" : "miss"}.`;
  hits.innerHTML = data.hits.map(hit => `
    <article class="hit">
      <strong>${hit.externalId} · score ${hit.score}</strong>
      <p>${hit.content}</p>
      <div class="chips">
        <span class="chip">semantic ${hit.semanticScore}</span>
        <span class="chip">keyword ${hit.keywordScore}</span>
        ${Object.entries(hit.metadata || {}).map(([key, val]) => `<span class="chip">${key}: ${val}</span>`).join("")}
      </div>
    </article>
  `).join("");
  await refreshProgress();
});

refreshProgress();
