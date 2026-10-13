(() => {
    const section = document.querySelector("[data-treemap-section]");
    if (!section || !window.d3) return;

    const container = section.querySelector("[data-treemap-container]");
    const viewport = section.querySelector("[data-treemap-viewport]");
    const data = JSON.parse(container.getAttribute("data-treemap") || "null");
    if (!data) return;

    const width = 960;
    const height = 560;

    const root = d3.treemap().size([width, height]).paddingInner(1.5)(
        d3.hierarchy(data.root)
            .sum((d) => d.value || 0)
            .sort((a, b) => b.value - a.value)
    );

    const open = (d) => window.location.assign(d.data.url);

    const svg = d3.select(viewport)
        .append("svg")
        .attr("class", "treemap-svg")
        .attr("viewBox", [0, 0, width, height])
        .attr("role", "group")
        .attr("aria-labelledby", "treemap-title");

    const node = svg.selectAll("g")
        .data(root.leaves())
        .join("g")
        .attr("class", "treemap-node")
        .attr("transform", (d) => `translate(${d.x0},${d.y0})`)
        .attr("tabindex", 0)
        .attr("role", "link")
        .attr("aria-label", (d) => `${d.data.name} : ${d.data.amountDisplay}`)
        .on("click", (event, d) => open(d))
        .on("keydown", (event, d) => {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                open(d);
            }
        });

    node.append("clipPath")
        .attr("id", (d, i) => `treemap-clip-${i}`)
        .append("rect")
        .attr("width", (d) => d.x1 - d.x0)
        .attr("height", (d) => d.y1 - d.y0);

    node.append("rect")
        .attr("class", "treemap-cell treemap-leaf")
        .attr("width", (d) => d.x1 - d.x0)
        .attr("height", (d) => d.y1 - d.y0);

    node.append("text")
        .attr("class", "treemap-leaf-text")
        .attr("clip-path", (d, i) => `url(#treemap-clip-${i})`)
        .selectAll("tspan")
        .data((d) => [d.data.name, d.data.amountDisplay])
        .join("tspan")
        .attr("x", 8)
        .attr("y", (d, i) => 16 + i * 15)
        .text((d) => d);
})();
