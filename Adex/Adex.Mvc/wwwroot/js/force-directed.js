function chart() {
    var width = 1800, height = 1000;

    const links = data.links.map(d => Object.create(d));
    const nodes = data.nodes.map(d => Object.create(d));

    var attractForce = d3.forceManyBody().strength(getAttrStrength).distanceMax(300).distanceMin(250);
    var repelForce = d3.forceManyBody().strength(getRepelStrength).distanceMax(100).distanceMin(0);

    const simulation = d3.forceSimulation(nodes).alphaDecay(0.1)
        .force("link", d3.forceLink(links).id(getId).distance(getDistance))
        .force("charge", d3.forceManyBody().strength(getStrength))
        .force("collision", d3.forceCollide().radius(getRadius))
        //.force("attractForce", attractForce)
        .force("repelForce", repelForce)
        .force("center", d3.forceCenter(width / 2, height / 2))
        //.force("x", d3.forceX())
        //.force("y", d3.forceY())
        ;

    const svg = d3.select("div#container").append("svg")
        .attr("preserveAspectRatio", "xMinYMin meet")
        .attr("viewBox", [0, 0, width, height])
        .classed("svg-content", true)
        ;

    const info = svg.append("g")
        .append("text")
        .attr("id", "info")
        .attr("x", 20)
        .attr("y", 30)
        .text("Survolez une bulle")
        ;

    const link = svg.append("g")
        .attr("stroke", "#999")
        .attr("stroke-opacity", 0.6)
        .selectAll("line")
        .data(links)
        .join("line")
        //.attr("stroke-width", d => Math.sqrt(d.nbLinks))
        ;

    const node = svg.append("g")
        .attr("stroke", "#fff")
        .attr("stroke-width", 1.5)
        .selectAll("circle")
        .data(nodes)
        .join("circle")
        .attr("r", getRadius)
        .attr("fill", color)
        .on("mouseover", popupInfos)
        .on("mouseout", handleMouseOut)
        .call(drag(simulation))
        ;

    link.append("title").text(d => 'nb links: ' + f.format(d.nbLinks) + ', amount: ' + f.format(d.amount));

    simulation
        .nodes(nodes)
        .on("tick", () => {
            link
                .attr("x1", d => d.source.x)
                .attr("y1", d => d.source.y)
                .attr("x2", d => d.target.x)
                .attr("y2", d => d.target.y);

            node
                .attr("cx", d => d.x)
                .attr("cy", d => d.y)
                ;
        });

    //invalidation.then(() => simulation.stop());
}

const f = new Intl.NumberFormat('fr-FR', { maximumSignificantDigits: 3 });

const scale = d3.scaleOrdinal(d3.schemeCategory10).domain(["2", "0", "3", "1"]);

function drag(simulation) {

    function dragstarted(d) {
        if (!d3.event.active) {
            simulation.alphaTarget(0.1).restart();
        }
        d.fx = d.x;
        d.fy = d.y;
    }

    function dragged(d) {
        d.fx = d3.event.x;
        d.fy = d3.event.y;
    }

    function dragended(d) {
        if (!d3.event.active) simulation.alphaTarget(0);
        d.fx = null;
        d.fy = null;
    }

    return d3.drag()
        .on("start", dragstarted)
        .on("drag", dragged)
        .on("end", dragended);
}

function getRadius(d, b, nodes) {
    var r = Math.round(Math.sqrt(d.amount / 5000));
    //console.log(d.amount + '  => ' + r);
    return r;
}

function getEmphasisedRadius(d, b, nodes) {
    var r = Math.round(Math.sqrt((d.amount) / 4000));
    //console.log(d.amount + '  => ' + r);
    return r;
}

function getAttrStrength(d) {
    var r = Math.round(Math.log(d.amount) * Math.log(d.amount) * Math.log(d.amount) / 100);
    //console.log(d.amount + ' => ' + r);
    return r;
}

function getRepelStrength(d) {
    var r = -Math.round(Math.log(d.amount) * Math.log(d.amount));
    //console.log(d.amount + ' => ' + r);
    return r;
}

function getStrength(d, b, nodes) {
    var r = -Math.round(Math.log(d.amount) * Math.log(d.amount) * Math.log(d.amount) / 500);
    //console.log(d.amount + ' => ' + r);
    return r;
}

function getDistance(link, b, nodes) {
    var rs = getRadius(link.source);
    var rt = getRadius(link.target);
    var r = Math.round((rs + rt) * 1.5);
    //console.log(link.source.amount + '/' + rs + ', ' + link.target.amount + '/' + rt + ' => ' + r);
    return r;
}

function getId(node, b, nodes) {
    return node.id;
}

function color(d) {
    return scale(d.group);
}

var nodeInfo = null;

function popupInfos(d, i) {
    if (!nodeInfo || nodeInfo.id != d.id) {
        nodeInfo = d;
        d3.select(this)
            .attr("fill", "orange")
            //.attr("r", getEmphasisedRadius(d))
            ;
        d3.json("api/Beneficiary/Read/" + d.id).then(function (obj) {
            d3.select("#info").text("");
            var counter = 1;
            Object.entries(obj).map(item => {
                var info = d3.select("#info");
                info.append("tspan").attr("x", 20).attr("y", 30 + 20 * counter).text(item[0] + ': ' + item[1]);
                counter++;
            })
        });
    }
}

function handleMouseOut(d, i) {
    d3.select(this)
        .attr("fill", color)
        //.attr("r", getRadius(d))
        ;
}

var data = {};

d3.json("data/data.json").then(function (obj) {
    data = obj;
    chart();
});