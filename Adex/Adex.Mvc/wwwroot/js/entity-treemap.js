(() => {
    const section = document.querySelector("[data-treemap-section]");
    if (!section || !window.d3) return;

    const container = section.querySelector("[data-treemap-container]");
    const viewport = section.querySelector("[data-treemap-viewport]");
    const data = JSON.parse(container.getAttribute("data-treemap") || "null");
    if (!data) return;

    const headerHeight = 30;
    const duration = window.matchMedia("(prefers-reduced-motion: reduce)").matches ? 0 : 750;

    // Chemin de zoom exprimé en indices d'enfants pour survivre au redimensionnement.
    let focusPath = [];
    let renderedWidth = 0;

    const dimensions = () => {
        const width = Math.max(240, Math.floor(viewport.clientWidth || 320));
        const height = Math.round(width < 600 ? width * 1.5 : width * 0.58);
        return { width, height };
    };

    const isOthers = (d) => d.data.name === "Autres" && !!d.children;

    const pathOf = (node) =>
        node.ancestors().reverse().slice(1).map((d) => d.parent.children.indexOf(d));

    const isOthersName = (d) => d.data.name === "Autres";

    const label = (node) => {
        const depth = node.depth;
        return depth === 0 ? node.data.name : isOthersName(node) ? `Autres (niveau ${depth})` : node.data.name;
    };

    // Pagination : racine, niveaux 1 et 2, « … », puis les deux niveaux précédant le niveau courant.
    const pageItems = (current) => {
        const items = [];
        for (let i = 0; i <= current; i++) {
            if (i <= 2 || i >= current - 2) {
                items.push(i);
            } else if (items[items.length - 1] !== "…") {
                items.push("…");
            }
        }
        return items;
    };

    const draw = () => {
        const { width, height } = dimensions();
        renderedWidth = width;
        viewport.replaceChildren();
        const pager = d3.select(viewport).append("nav")
            .attr("class", "treemap-pager")
            .attr("aria-label", "Niveaux de la carte");

        // Les enfants sont disposés aux proportions de la zone affichée : le zoom ne déforme jamais les cellules.
        const tile = (node, x0, y0, x1, y1) => {
            // Le poids d'« Autres » est artificiel : le total du nœud doit être recalculé à partir du poids de ses enfants.
            node.value = node.children.reduce((sum, child) => sum + child.value, 0);
            d3.treemapBinary(node, 0, 0, width, height);
            for (const child of node.children) {
                child.x0 = x0 + (child.x0 / width) * (x1 - x0);
                child.x1 = x0 + (child.x1 / width) * (x1 - x0);
                child.y0 = y0 + (child.y0 / height) * (y1 - y0);
                child.y1 = y0 + (child.y1 / height) * (y1 - y0);
            }
        };

        // « Autres » ne suit pas la proportionnalité : il pèse autant qu'une contrepartie moyenne de son niveau.
        // Seul son propre poids change, ses descendants gardent leurs montants réels.
        const weighted = d3.hierarchy(data.root).sum((d) => d.value || 0);
        weighted.each((node) => {
            if (!node.children) return;
            const others = node.children.filter(isOthers);
            const regular = node.children.filter((child) => !isOthers(child));
            if (others.length === 0 || regular.length === 0) return;
            const mean = regular.reduce((sum, child) => sum + child.value, 0) / regular.length;
            others.forEach((child) => { child.value = mean; });
        });

        // « Autres » reste toujours en dernier, les autres blocs sont triés par montant.
        const hierarchy = d3.treemap().tile(tile)(
            weighted.sort((a, b) => (isOthers(a) ? 1 : isOthers(b) ? -1 : b.value - a.value))
        );

        const x = d3.scaleLinear().rangeRound([0, width]);
        const y = d3.scaleLinear().rangeRound([0, height]);
        let clipSequence = 0;
        let climbing = false;

        const svg = d3.select(viewport)
            .append("svg")
            .attr("class", "treemap-svg")
            .attr("viewBox", [0, -headerHeight, width, height + headerHeight])
            .attr("role", "group")
            .attr("aria-labelledby", "treemap-title");

        const position = (group, root) => {
            group.selectAll("g")
                .attr("transform", (d) =>
                    d === root ? `translate(0,${-headerHeight})` : `translate(${x(d.x0)},${y(d.y0)})`)
                .select("rect")
                .attr("width", (d) => (d === root ? width : x(d.x1) - x(d.x0)))
                .attr("height", (d) => (d === root ? headerHeight : y(d.y1) - y(d.y0)));
        };

        const activate = (d, root) => {
            if (climbing) return;
            if (d === root) {
                if (d.parent) zoomOut(d);
            } else if (d.children) {
                zoomIn(d);
            } else if (d.data.url) {
                window.location.assign(d.data.url);
            }
        };

        const render = (group, root) => {
            const node = group.selectAll("g")
                .data([root, ...root.children])
                .join("g")
                .attr("class", (d) => (d === root && !d.parent ? "treemap-node treemap-node-static" : "treemap-node"))
                .attr("tabindex", (d) => (d === root && !d.parent ? null : 0))
                .attr("role", (d) => (d !== root && !d.children ? "link" : d === root && !d.parent ? null : "button"))
                .attr("aria-label", (d) =>
                    d === root
                        ? `${label(d)}${d.parent ? " : revenir au niveau supérieur" : ""}`
                        : `${d.data.name} : ${d.data.amountDisplay}`)
                .on("click", (event, d) => activate(d, root))
                .on("keydown", (event, d) => {
                    if (event.key === "Enter" || event.key === " ") {
                        event.preventDefault();
                        activate(d, root);
                    }
                });

            node.append("rect")
                .attr("id", (d) => (d.clipId = `treemap-clip-${++clipSequence}`) + "-rect")
                .attr("class", (d) => {
                    if (d === root) return "treemap-cell treemap-header";
                    return `treemap-cell ${d.children ? "treemap-group" : "treemap-leaf"}`;
                });

            node.append("clipPath")
                .attr("id", (d) => d.clipId)
                .append("use")
                .attr("href", (d) => `#${d.clipId}-rect`);

            node.append("text")
                .attr("class", (d) => (d === root ? "treemap-header-text" : "treemap-leaf-text"))
                .attr("clip-path", (d) => `url(#${d.clipId})`)
                .selectAll("tspan")
                .data((d) =>
                    d === root
                        ? [`${d.parent ? "‹ " : ""}${label(d)}`]
                        : [d.data.name, d.data.amountDisplay])
                .join("tspan")
                .attr("x", 8)
                .attr("y", (d, i, nodes) =>
                    d3.select(nodes[i].parentNode).datum() === root ? headerHeight / 2 + 5 : 16 + i * 15)
                .text((d) => d);

            group.call(position, root);
        };

        const focus = focusPath.reduce((node, index) => node.children?.[index] ?? node, hierarchy);
        x.domain([focus.x0, focus.x1]);
        y.domain([focus.y0, focus.y1]);
        let group = svg.append("g").call(render, focus);

        // Pagination des niveaux : accès direct à n'importe quel niveau parcouru.
        const updateLevels = (node) => {
            pager.style("display", node.depth > 0 ? null : "none");
            const trail = node.ancestors().reverse();
            pager.selectAll("*").remove();
            pageItems(node.depth).forEach((item, index) => {
                if (index > 0) pager.append("span").attr("class", "treemap-pager-sep").text("›");
                if (item === "…") {
                    pager.append("span").attr("class", "treemap-pager-gap").text("…");
                    return;
                }
                const target = trail[item];
                const current = target === node;
                pager.append("button")
                    .attr("type", "button")
                    .attr("class", "treemap-pager-item")
                    .attr("aria-current", current ? "page" : null)
                    .attr("title", label(target))
                    .text(item === 0 ? target.data.name : item)
                    .on("click", () => {
                        if (!current) zoomOut(node, target);
                    });
            });
        };
        updateLevels(focus);

        const zoomIn = (d) => {
            focusPath = pathOf(d);
            updateLevels(d);
            const group0 = group.attr("pointer-events", "none");
            const group1 = (group = svg.append("g").call(render, d));

            x.domain([d.x0, d.x1]);
            y.domain([d.y0, d.y1]);

            svg.transition()
                .duration(duration)
                .call((t) => group0.transition(t).remove().call(position, d.parent))
                .call((t) => group1.transition(t).attrTween("opacity", () => d3.interpolate(0, 1)).call(position, d));
        };

        // Remonte d'un niveau ; la promesse se résout à la fin de la transition.
        const zoomOutStep = (d, stepDuration, ease) => {
            const target = d.parent;
            focusPath = pathOf(target);
            updateLevels(target);
            const group0 = group.attr("pointer-events", "none");
            const group1 = (group = svg.insert("g", "*").call(render, target));

            x.domain([target.x0, target.x1]);
            y.domain([target.y0, target.y1]);

            const transition = svg.transition()
                .duration(stepDuration)
                .ease(ease)
                .call((t) => group0.transition(t).remove().attrTween("opacity", () => d3.interpolate(1, 0)).call(position, d))
                .call((t) => group1.transition(t).call(position, target));
            return transition;
        };

        // Vers un niveau précédent, on repasse par chaque niveau intermédiaire.
        const zoomOut = async (d, target = d.parent) => {
            if (climbing) return;
            climbing = true;
            const steps = d.depth - target.depth;
            const stepDuration = steps > 1 ? Math.max(250, duration / 2) : duration;
            // Vitesse constante pendant la remontée, arrivée progressive sur la dernière étape seulement.
            const easeFor = (index) =>
                steps === 1 ? d3.easeCubicInOut
                    : index === steps - 1 ? d3.easeCubicOut
                        : d3.easeLinear;
            try {
                let index = 0;
                for (let node = d; node !== target; node = node.parent) {
                    const transition = zoomOutStep(node, stepDuration, easeFor(index));
                    const last = index === steps - 1;
                    index++;
                    // L'étape suivante démarre avant la fin de celle-ci (elle reprend la position courante) : aucun arrêt entre deux niveaux.
                    await (last
                        ? transition.end().catch(() => {})
                        : new Promise((resolve) => setTimeout(resolve, stepDuration * 0.8)));
                }
            } finally {
                climbing = false;
            }
        };

    };

    draw();

    let timer;
    window.addEventListener("resize", () => {
        clearTimeout(timer);
        timer = setTimeout(() => {
            if (dimensions().width !== renderedWidth) draw();
        }, 150);
    });
})();
